using System;
using System.Linq;
using System.Windows.Forms;
using Holsee.RxLab.DictionarySuggestService;

namespace Holsee.RxLab
{
    public static class RxAsyncMethodPattern
    {
        public static void NonReactiveDictionarySuggestSevice()
        {
            var svc = new DictServiceSoapClient("DictServiceSoap");

            //Starts a webservice call
            svc.BeginMatchInDict("wn", "react", "prefix",
                                 //AsyncCallback delegate
                                 iar =>
                                     {
                                         DictionaryWord[] words = svc.EndMatchInDict(iar);
                                         foreach (DictionaryWord word in words)
                                         {
                                             Console.WriteLine(word.Word);
                                         }
                                     },
                                 null
                );

            //The above operation is quite "clumsy", 
            //composition with our other Async data sources (TextBox) becomes difficult.
            //It is also not clear how one can cancel existing requests, 
            //such that the Callback procedure is guaranteed not to be called anymore.
            //The data aspect of the Async call not being immediately apparent.

            Console.ReadLine(); //Wait to allow user to see output.
        }

        public static void ReactiveDictionarySuggestSevice()
        {
            var svc = new DictServiceSoapClient("DictServiceSoap");

            //Converting the above fragment to Rx isn't very hard, using the FromAnsycPattern method 
            //which takes a whole bunch of generic overloads for various Begin* method param counts.
            //The generic params passed to this bridge are the types of the Begin* method params, 
            //as well as the return type of End*.

            Func<string, string, string, IObservable<DictionaryWord[]>> matchInDict = Observable.FromAsyncPattern
                <string, string, string, DictionaryWord[]>(
                    svc.BeginMatchInDict,
                    svc.EndMatchInDict
                );

            //The result of this bridging is a Func delegate thats takes the web service params and 
            //produces and observable sequence that will receive the result.

            IObservable<DictionaryWord[]> resultsFromDictSvc = matchInDict("wn", "react", "prefix");
            IDisposable subscription = resultsFromDictSvc.Subscribe(
                words =>
                    {
                        foreach (DictionaryWord word in words)
                        {
                            Console.WriteLine(word.Word);
                        }
                    });

            Console.ReadLine(); //Wait to allow user to see output.

            subscription.Dispose(); //Unsubscribe (could just use a using block)
        }

        public static void SearchDictionaryAsyncFromUserThrottledAndDistinctInputThenUpdateUI()
        {
            //Set up UI with a TextBox Input and ListBox for results
            var txt = new TextBox();
            var list = new ListBox
            {
                Top = txt.Height + 10,
                Height = 250,
                Width = 290
            };
            var frm = new Form
            {
                Controls = { txt, list }
            };

            //Create an instance of our Async WebService Proxy
            var svc = new DictServiceSoapClient("DictServiceSoap");

            //Turn user input into a teamed sequence of strings
            IObservable<string> textChanged = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox)evt.Sender).Text);

            //Create an observable for input from the user
            IObservable<string> input = textChanged
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(1))
                .Do(Console.WriteLine);

            //Wrap the Begin End Async Method Pattern for Dictionary web service
            Func<string, string, string, IObservable<DictionaryWord[]>> matchInDict =
                Observable.FromAsyncPattern<string, string, string, DictionaryWord[]>(
                    svc.BeginMatchInDict,
                    svc.EndMatchInDict
                    );

            //Create an observable for result of Async Dictionary web service call
            //The grand composition connecting the user input with the web service
            IObservable<DictionaryWord[]> resultFromDictSvc = input.SelectMany(
                term => matchInDict("wn", term, "prefix")
                );

            //Synchronize with the UI thread and populate the ListBox or signal an Error
            using (resultFromDictSvc.ObserveOn(list).Subscribe(
                dictionaryWords =>
                {
                    list.Items.Clear();

                    list.Items.AddRange((dictionaryWords
                                            .Select(dictionaryWord => dictionaryWord.Word))
                                            .ToArray());
                },
                ex => MessageBox.Show(String.Format("An error occurred: {0}", ex.Message), frm.Text)))
            {
                Application.Run(frm);
            } //Proper disposal happens upon exiting the application
        }

        public static void SearchDictionaryAsyncFromUserThrottledAndDistinctInputThenUpdateUIWithCancellationTakeUntil()
        {
            //Set up UI with a TextBox Input and ListBox for results
            var txt = new TextBox();
            var list = new ListBox
                           {
                               Top = txt.Height + 10,
                               Height = 250,
                               Width = 290
                           };
            var frm = new Form
                          {
                              Controls = {txt, list}
                          };

            //Create an instance of our Async WebService Proxy
            var svc = new DictServiceSoapClient("DictServiceSoap");

            //Turn user input into a teamed sequence of strings
            IObservable<string> textChanged = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text);

            //Create an observable for input from the user
            IObservable<string> input = textChanged
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(1))
                .Do(Console.WriteLine);

            //Wrap the Begin End Async Method Pattern for Dictionary web service
            Func<string, string, string, IObservable<DictionaryWord[]>> matchInDict =
                Observable.FromAsyncPattern<string, string, string, DictionaryWord[]>(
                    svc.BeginMatchInDict,
                    svc.EndMatchInDict
                    );

            //Create an observable for result of Async Dictionary web service call
            //The grand composition connecting the user input with the web service
            IObservable<DictionaryWord[]> resultFromDictSvc = input.SelectMany(
                term => matchInDict("wn", term, "prefix")
                            .Finally(() => Console.WriteLine("Disposed request for {0}", term)) // * Highlights Disposal
                            .TakeUntil(input) // * We Take Only Until a new Input terms comes in
                );

            //Synchronize with the UI thread and populate the ListBox or signal an Error
            using (resultFromDictSvc.ObserveOn(list).Subscribe(
                dictionaryWords =>
                    {
                        list.Items.Clear();

                        list.Items.AddRange((dictionaryWords
                                                .Select(dictionaryWord => dictionaryWord.Word))
                                                .ToArray());
                    },
                ex => MessageBox.Show(String.Format("An error occurred: {0}", ex.Message), frm.Text)))
            {
                Application.Run(frm);
            } //Proper disposal happens upon exiting the application
        }

        public static void SearchDictionaryAsyncFromUserThrottledAndDistinctInputThenUpdateUIWithCancellationSwitch()
        {
            //Set up UI with a TextBox Input and ListBox for results
            var txt = new TextBox();
            var list = new ListBox
            {
                Top = txt.Height + 10,
                Height = 250,
                Width = 290
            };
            var frm = new Form
            {
                Controls = { txt, list }
            };

            //Create an instance of our Async WebService Proxy
            var svc = new DictServiceSoapClient("DictServiceSoap");

            //Turn user input into a teamed sequence of strings
            IObservable<string> textChanged = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox)evt.Sender).Text);

            //Create an observable for input from the user
            IObservable<string> input = textChanged
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(1))
                .Do(Console.WriteLine);

            //Wrap the Begin End Async Method Pattern for Dictionary web service
            Func<string, string, string, IObservable<DictionaryWord[]>> matchInDict =
                Observable.FromAsyncPattern<string, string, string, DictionaryWord[]>(
                    svc.BeginMatchInDict,
                    svc.EndMatchInDict
                    );

            //This is an alternative composition which does nto leverage SelectMany for the projection
            //We use Switch() which cancel's out one's existing subscriptions and hops to a new one upon new input.
            // ---o-----o--------------o
            //    |     |              |
            //    V     |              |                         |
            //====---o--X---..         |                         S
            //    =  |  |              |                         W                         
            //    =  V  V              |                         I
            //    ===O  ----o------o---X--..                     T
            //       =      |      |   |                         C
            //       =      V      V   V                         T
            //       =======O======O   ---o----o----o--..        |         
            //                     =      |    |    |            V
            //                     =      V    V    V
            //                     =======O====O====O=== IObservable<T>

            IObservable<DictionaryWord[]> resultFromDictSvc = (from term in input
                                                               select matchInDict("wn", term, "prefix"))
                                                               .Switch();

            //Synchronize with the UI thread and populate the ListBox or signal an Error
            using (resultFromDictSvc.ObserveOn(list).Subscribe(
                dictionaryWords =>
                {
                    list.Items.Clear();

                    list.Items.AddRange((dictionaryWords
                                            .Select(dictionaryWord => dictionaryWord.Word))
                                            .ToArray());
                },
                ex => MessageBox.Show(String.Format("An error occurred: {0}", ex.Message), frm.Text)))
            {
                Application.Run(frm);
            } //Proper disposal happens upon exiting the application
        }
    }
}
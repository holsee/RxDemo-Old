using System;
using System.Linq;
using Holsee.RxLab.DictionarySuggestService;

namespace Holsee.RxLab
{
    public static class RxAsyncMethodPattern
    {
        public static void NonReactiveDictionarySuggestSevice()
        {
            var svc = new DictionarySuggestService.DictServiceSoapClient("DictServiceSoap");

            //Starts a webservice call
            svc.BeginMatchInDict("wn", "react", "prefix",
                 //AsyncCallback delegate
                 iar =>
                     {
                         var words = svc.EndMatchInDict(iar);
                         foreach (var word in words)
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
            
            var matchInDict = Observable.FromAsyncPattern<string, string, string, DictionaryWord[]>(
                svc.BeginMatchInDict,
                svc.EndMatchInDict
                );

            //The result of this bridging is a Func delegate thats takes the web service params and 
            //produces and observable sequence that will receive the result.

            var result = matchInDict("wn", "react", "prefix");
            var subscription = result.Subscribe(words =>
                                                    {
                                                        foreach (var word in words)
                                                        {
                                                            Console.WriteLine(word.Word);
                                                        }
                                                    });

            
            Console.ReadLine(); //Wait to allow user to see output.

            subscription.Dispose(); //Free the subscription resources
        }


    }
}
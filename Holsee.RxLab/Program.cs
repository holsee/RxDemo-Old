using System;
using System.Collections.Generic;
using System.Disposables;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Holsee.RxLab
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //TryCatch(TextInputDistinctWithDo);
        }

        #region Rx


        /// <summary>
        /// A demonstration of the majority of the Observable static Factory Methods.
        /// </summary>
        private static void ObservableFactoryMethods()
        {
            //IObservable<int> source = new[] {1, 2, 3, 4}.ToObservable();
            //IObservable<int> source = Observable.Empty<int>();
            //IObservable<int> source = Observable.Never<int>();
            //IObservable<int> source = Observable.Throw<int>(new Exception("Ooops"));
            //IObservable<int> source = Observable.Return(42);
            //IObservable<int> source = Observable.Range(0, 12000);
            //IObservable<int> source = Observable.Generate(
            //    0,              //Initial State
            //    i => i < 5,     //Condition
            //    i => i * i,     //Selector
            //    i => i + 1      //Iterator
            //);

            IObservable<int> source = Observable.GenerateWithTime(
                0, //Initial State
                i => i < 5, //Condition
                i => i * i, //Selector
                i => TimeSpan.FromSeconds(1), //Interval
                i => i + 1 //Iterator
                );


            IDisposable subscription = source.Subscribe(
                x => Console.WriteLine("OnNext: {0}", x),
                ex => Console.WriteLine("OnError: {0}", ex),
                () => Console.WriteLine("OnCompleted"));


            Console.WriteLine("Press enter to unsubscribe...");
            Console.ReadLine();

            subscription.Dispose();

            //Console.WriteLine("Run started");
            ////Will not run async 
            //source.Run(
            //    x => Console.WriteLine("OnNext: {0}", x),
            //    ex => Console.WriteLine("OnError: {0}", ex),
            //    () => Console.WriteLine("OnCompleted")
            //  );

            //Console.WriteLine("Run Finished");
            //Console.ReadLine();
        }

        #endregion

        #region Rx Events Demos

        /// <summary>
        /// Refactored the Logging code into a signle custom operator "LogTimestampedValues"
        /// </summary>
        public static void TextInputDistinctWithThrottleAndCustomTimestampOperator()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .LogTimestampedValues(inp => Console.WriteLine("D: {0} - {1}", inp.Timestamp.Second, inp.Value))
                .Throttle(TimeSpan.FromSeconds(1))
                .LogTimestampedValues(inp => Console.WriteLine("T: {0} - {1}", inp.Timestamp.Second, inp.Value));

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// To illustrate Throttle's effect, let's use the Do operator in 
        /// conjunction with two specialized projection operators called Timestamp
        /// and RemoveTimestamp.
        /// </summary>
        public static void TextInputDistinctWithThrottleAndTimestamps()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .Timestamp() //Turns the Observable<T> into an Observable<Timestamped<T>>
                .Do(inp => Console.WriteLine("D: {0} - {1}", inp.Timestamp.Second, inp.Value))
                .RemoveTimestamp() //Turns the Observable<Timestamped<T>> into an Observable<T>
                .Throttle(TimeSpan.FromSeconds(1))
                .Timestamp()
                .Do(inp => Console.WriteLine("T: {0} - {1}", inp.Timestamp.Second, inp.Value))
                .RemoveTimestamp();

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// This demonstration introduces the Throttle operator, which will prevent 
        /// the event being pushed until 1 second after the last event has been fired.
        /// </summary>
        private static void TextInputDistinctWithThrottle()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .Do(inp => Console.WriteLine("Before DistinctUntilChanged: {0}", inp)) //Fires regardless of throttle
                .DistinctUntilChanged() //Accepts an IEqualityComparer<T>
                .Throttle(TimeSpan.FromSeconds(1)); //Waits 1 second after last event fired before acting

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// Inserted Do into the Reactive Expression to outline some, unexpected behaviour.
        /// </summary>
        private static void TextInputDistinctWithDo()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .Do(inp => Console.WriteLine("Before DistinctUntilChanged: {0}", inp))
                .DistinctUntilChanged() //Accepts an IEqualityComparer<T>
                .Do(inp => Console.WriteLine("NOT called after DistinctUntilChanged: {0}", inp));

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// This version of the TextInputFunction will NOT react to the 
        /// change if the value is the same as the previous value.
        /// This allows us to avoid redundant events, 
        /// in turn allowing us to avoid redundant calls.
        /// </summary>
        private static void TextInputDistinct()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .DistinctUntilChanged(); //Accepts an IEqualityComparer<T>

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// A basic demonstration of capturing TextChanged Events from a TextBox.
        /// </summary>
        private static void TextInput()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox)evt.Sender).Text);

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// A demonstration of CompositeDisposable for efficient resource release 
        /// when working with subscriptions over numerous IObservables. 
        /// This example also leverages more meaningful projects via the Select LINQ operator.
        /// </summary>
        private static void MouseMovesAndTextInput()
        {
            var txt = new TextBox();
            var frm = new Form
                          {
                              Controls = {txt}
                          };

            IObservable<Point> moves =
                Observable.FromEvent<MouseEventArgs>(frm, "MouseMove")
                    .Select(evt => evt.EventArgs.Location);

            IObservable<string> input =
                Observable.FromEvent<EventArgs>(txt, "TextChanged")
                    .Select(evtArg => ((TextBox) evtArg.Sender).Text);

            IDisposable movesSub = moves.Subscribe(
                loc => Console.WriteLine("Mouse at: {0}", loc)
                );

            IDisposable txtSub = input.Subscribe(
                text => Console.WriteLine("User wrote: {0}", text)
                );

            using (new CompositeDisposable(movesSub, txtSub))
            {
                Application.Run(frm);
            }
        }

        /// <summary>
        /// A demostration of the reactive extesions appraoch to dealing with Events.
        /// </summary>
        private static void UsingRxOverEvents()
        {
            var lbl = new Label();
            var frm = new Form
                          {
                              Controls = {lbl}
                          };

            IObservable<IEvent<MouseEventArgs>> moves =
                Observable.FromEvent<MouseEventArgs>(frm, "MouseMove");

            using (moves.Subscribe(evt =>
                                       {
                                           lbl.Text = String.Format("X: {0}, Y: {1}",
                                                                    evt.EventArgs.X,
                                                                    evt.EventArgs.Y);

                                           lbl.Location = new Point(evt.EventArgs.X, evt.EventArgs.Y);
                                       }))
            {
                Application.Run(frm);
            } //Proper clean-up is now a lot easier...!
        }

        /// <summary>
        /// A demonstration of traditional callback handlers for dealing with Events.
        /// </summary>
        private static void UsingEventHandlers()
        {
            var lbl = new Label();
            var frm = new Form
                          {
                              Controls = {lbl}
                          };

            frm.MouseMove += (sender, evtargs) =>
                                 {
                                     lbl.Text = String.Format("X: {0}, Y: {1}",
                                                              evtargs.X,
                                                              evtargs.Y);

                                     lbl.Location = new Point(evtargs.X, evtargs.Y);
                                 }; //We don't unsubscribe D= !

            Application.Run(frm);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Simply wraps the <paramref name="action"/> in a Try Catch
        /// </summary>
        /// <param name="action"></param>
        public static void TryCatch(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
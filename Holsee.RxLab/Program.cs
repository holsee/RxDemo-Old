using System;
using System.Collections.Generic;
using System.Disposables;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Holsee.RxLab
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TryCatchError(TextInputDistinctWithThrottle);
        }

        #region Rx Demos
        
        private static void TextInputDistinctWithThrottle()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            var input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .Do(inp => Console.WriteLine("Before DistinctUntilChanged: {0}", inp)) //Fires regardless of throttle
                .DistinctUntilChanged() //Accepts an IEqualityComparer<T>
                .Throttle(TimeSpan.FromSeconds(1)); //Waits 1 second after last event fired before acting

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        private static void TextInputDistinctWithDo()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            var input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
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
        /// This version of the TextInputFunction will not react to the change if the value is the same.
        /// This means we can avoid calls being made with the same value.
        /// </summary>
        private static void TextInputDistinct()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            var input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                        .Select(evt => ((TextBox)evt.Sender).Text)
                        .DistinctUntilChanged(); //Accepts an IEqualityComparer<T>

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        private static void TextInput()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            var input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox)evt.Sender).Text);

            using (input.Subscribe(inp => Console.WriteLine("User wrote: {0}", inp)))
            {
                Application.Run(frm);
            }
        }

        private static void MouseMovesAndTextInput()
        {
            var txt = new TextBox();
            var frm = new Form
            {
                Controls = { txt }
            };

            var moves =
                Observable.FromEvent<MouseEventArgs>(frm, "MouseMove")
                    .Select(evt => evt.EventArgs.Location);

            var input =
                Observable.FromEvent<EventArgs>(txt, "TextChanged")
                    .Select(evtArg => ((TextBox)evtArg.Sender).Text);

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

        private static void UsingRxOverEvents()
        {
            var lbl = new Label();
            var frm = new Form
            {
                Controls = { lbl }
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

        private static void UsingEventHandlers()
        {
            var lbl = new Label();
            var frm = new Form
            {
                Controls = { lbl }
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

        private static void ExerciseOne()
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

        #region Helpers

        public static void TryCatchError(Action action)
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
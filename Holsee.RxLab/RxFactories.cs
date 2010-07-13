using System;
using System.Linq;

namespace Holsee.RxLab
{
    public static class RxFactories
    {
        /// <summary>
        /// A demonstration of the majority of the Observable static Factory Methods.
        /// </summary>
        public static void ObservableFactoryMethods()
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
                i => i*i, //Selector
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
    }
}
using System;
using Holsee.RxLab.Helpers;

namespace Holsee.RxLab
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ErrorHandling.TryCatch<Exception>(
                //RxFactories.ObservableFactoryMethods

                //RxLinqToEvents.UsingEventHandlers
                //RxLinqToEvents.UsingRxOverEvents
                //RxLinqToEvents.MouseMovesAndTextInput

                //RxLinqToEvents.TextInput
                //RxLinqToEvents.TextInputDistinct
                //RxLinqToEvents.TextInputDistinctWithDo
                //RxLinqToEvents.TextInputDistinctWithThrottle
                //RxLinqToEvents.TextInputDistinctWithThrottleAndTimestamps
                //RxLinqToEvents.TextInputDistinctWithThrottleAndCustomTimestampOperator

                //RxThreadSync.UIThreadUpdatingWindowsForms

                //RxAsyncMethodPattern.NonReactiveDictionarySuggestSevice
                //RxAsyncMethodPattern.ReactiveDictionarySuggestSevice
       
                RxAsyncMethodPattern.SearchDictionaryAsyncFromUserThrottledAndDistinctInputThenUpdateUI
                
                );
        }
    }
}
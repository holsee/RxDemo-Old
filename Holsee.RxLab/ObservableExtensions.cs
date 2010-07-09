using System;
using System.Linq;

namespace Holsee.RxLab
{
    public static class ObservableExtensions
    {
        /// <summary>
        /// Converts <paramref name="source"/> to IObservable of Timestamped of <typeparamref name="T"/>, 
        /// performs <paramref name="onNext"/>, then converts <paramref name="source"/> back.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <returns></returns>
        public static IObservable<T> LogTimestampedValues<T>(this IObservable<T> source, Action<Timestamped<T>> onNext)
        {
            return source.Timestamp().Do(onNext).RemoveTimestamp();
        }
    }
}
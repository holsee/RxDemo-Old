using System;

namespace Holsee.RxLab
{
    public static class RxAsyncMethodPattern
    {
        public static void ReactiveDictionarySuggestSevice()
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
            //the data asoect of the async call not being immediately apparent.

            Console.ReadLine(); //Wait to allow user to see output.
        }
    }
}
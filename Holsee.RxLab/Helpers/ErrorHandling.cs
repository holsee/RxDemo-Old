using System;
using System.Windows.Forms;

namespace Holsee.RxLab.Helpers
{
    public class ErrorHandling
    {
        /// <summary>
        /// Simply wraps the <paramref name="action"/> in a Try Catch
        /// </summary>
        /// <param name="action"></param>
        public static void TryCatch<T>(Action action) where T : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (T ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to Exit Application...");
                Console.ReadLine();
                Application.Exit();
            }
        }
    }
}
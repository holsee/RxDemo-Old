using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Holsee.RxLab
{
    public static class RxThreadSync
    {
        /// <summary>
        /// This demonstration shows how to avoid trying ot update the UI thread,
        /// from the Background thread in which OnNext will be called on.
        /// </summary>
        public static void UIThreadUpdatingWindowsForms()
        {
            var txt = new TextBox();
            var lbl = new Label
                          {
                              Location = new Point(100, 100)
                          };
            var frm = new Form
                          {
                              Controls = {txt, lbl}
                          };

            IObservable<string> input = Observable.FromEvent<EventArgs>(txt, "TextChanged")
                .Select(evt => ((TextBox) evt.Sender).Text)
                .DistinctUntilChanged();


            //NOTE: In WPF you would use the .ObserveOnDispatcher() instead
            using (input.ObserveOn(lbl).Subscribe(inp => lbl.Text = inp))
            {
                Application.Run(frm);
            }
        }
    }
}
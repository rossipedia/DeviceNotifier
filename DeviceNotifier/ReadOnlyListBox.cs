using System.Windows.Forms;

namespace DeviceNotifier
{
    internal class ReadOnlyListBox : ListBox
    {
        protected override void DefWndProc(ref Message m)
        {
            // Block any messages to the selection area from the 
            // mouse or keyboard. Let all other messages pass 
            // through to the Windows default implementation of DefWndProc.
            if (((m.Msg <= 0x0200 || m.Msg >= 0x020E)
                 && (m.Msg <= 0x0100 || m.Msg >= 0x0109)
                 && m.Msg != 0x2111
                 && m.Msg != 0x87))
            {
                base.DefWndProc(ref m);
            }
        }
    }
}

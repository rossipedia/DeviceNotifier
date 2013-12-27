using System;
using System.Windows.Forms;

namespace DeviceNotifier
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var context = new DeviceNotifierApplicationContext())
            {
                Application.Run(context);
            }
        }
    }
}
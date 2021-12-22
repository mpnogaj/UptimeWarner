using System;
using System.Windows.Forms;

namespace UptimeWarner
{
    internal static class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }
    }
}
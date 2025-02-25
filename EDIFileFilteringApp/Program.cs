using System;
using System.Windows.Forms;

namespace EDIFileFilteringApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Görsel stili etkinleştir
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Form1'i ana form olarak başlat
            Application.Run(new Form1());
        }
    }
}


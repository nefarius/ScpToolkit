using System;
using System.Windows.Forms;
using System.Security.Principal;

namespace ScpDriver 
{
    static class Program 
    {
        [STAThread]
        static void Main(String[] args) 
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScpForm());
        }
    }
}

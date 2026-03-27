using System;
using System.Threading;
using System.Windows.Forms;

namespace himchistka
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.Run(new Form1());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Произошла ошибка: {e.Exception.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var message = exception?.Message ?? "Неизвестная ошибка";
            MessageBox.Show($"Критическая ошибка: {message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

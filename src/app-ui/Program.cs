using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI;

internal static class Program
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;

    [STAThread]
    static void Main()
    {
        // Crear un mutex único para la aplicación
        bool createdNew;
        var mutex = new Mutex(true, "GymRoutineGenerator_SingleInstance", out createdNew);

        if (!createdNew)
        {
            // Ya hay una instancia ejecutándose, activar esa ventana
            var currentProcess = Process.GetCurrentProcess();
            var runningProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                .FirstOrDefault(p => p.Id != currentProcess.Id);

            if (runningProcess != null)
            {
                var handle = runningProcess.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                }
            }
            return;
        }

        try
        {
            // Configurar agrupamiento en barra de tareas ANTES de crear cualquier ventana
            TaskbarGroupingHelper.InitializeApplicationGrouping();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            Application.Run(new MainForm());
        }
        finally
        {
            // Liberar el mutex cuando la aplicación se cierre
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
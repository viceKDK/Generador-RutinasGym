using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.UI.Dispatching;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace GymRoutineGenerator.UI
{
    public static class WinUIProgram
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Initialize COM wrappers for WinUI 3
            WinRT.ComWrappersSupport.InitializeComWrappers();

            if (DecideRedirection())
            {
                // Another instance is active; this process will exit after redirection
                return;
            }

            // Start WinUI application
            Application.Start(_ =>
            {
                var ctx = DispatcherQueue.GetForCurrentThread();
                SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(ctx));
                new App();
            });
        }

        private static bool DecideRedirection()
        {
            try
            {
                // Ensure a single instance by key
                string key = "main";
                var current = AppInstance.GetCurrent();
                var args = current.GetActivatedEventArgs();
                var instance = AppInstance.FindOrRegisterForKey(key);

                if (!instance.IsCurrent)
                {
                    // Redirect activation to the primary instance and exit
                    instance.RedirectActivationToAsync(args).AsTask().Wait();
                    return true;
                }

                // Bring existing window to front on activation
                current.Activated += (_, __) =>
                {
                    try
                    {
                        if (App.MainWindow is not null)
                        {
                            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                            ShowWindow(hwnd, SW_RESTORE);
                            SetForegroundWindow(hwnd);
                            App.MainWindow.Activate();
                        }
                    }
                    catch { }
                };
            }
            catch
            {
                // Fallback: if AppInstance fails, continue without single-instance
            }
            return false;
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
    }
}

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GymRoutineGenerator.UI
{
    /// <summary>
    /// Helper class para configurar el Application User Model ID (AppUserModelID)
    /// Esto hace que todas las ventanas de la aplicación se agrupen bajo un solo icono en la barra de tareas
    /// </summary>
    public static class TaskbarGroupingHelper
    {
        // AppUserModelID único para nuestra aplicación
        private const string APP_ID = "GymRoutineGenerator.MainApp.v1";

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_SETICON = 0x0080;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        private static bool _isInitialized = false;

        /// <summary>
        /// Configura el AppUserModelID para el proceso actual.
        /// Debe llamarse UNA VEZ al inicio de la aplicación en Program.cs
        /// </summary>
        public static void InitializeApplicationGrouping()
        {
            if (_isInitialized)
                return;

            try
            {
                SetCurrentProcessExplicitAppUserModelID(APP_ID);
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine($"[TaskbarGrouping] AppUserModelID configurado: {APP_ID}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskbarGrouping] Error configurando AppUserModelID: {ex.Message}");
            }
        }

        /// <summary>
        /// Configura una ventana Form para que use el icono de la aplicación principal
        /// y se agrupe correctamente en la barra de tareas
        /// </summary>
        /// <param name="form">Formulario a configurar</param>
        public static void ConfigureFormGrouping(Form form)
        {
            if (form == null)
                return;

            try
            {
                // Usar el mismo icono que la aplicación principal
                if (form.Icon == null || form.ShowIcon)
                {
                    try
                    {
                        var mainIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                        if (mainIcon != null)
                        {
                            form.Icon = mainIcon;
                        }
                    }
                    catch
                    {
                        // Si falla, continuar sin icono
                    }
                }

                // Asegurar que la ventana muestre el icono
                form.ShowIcon = true;
                form.ShowInTaskbar = true;

                System.Diagnostics.Debug.WriteLine($"[TaskbarGrouping] Formulario configurado: {form.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskbarGrouping] Error configurando formulario {form.Text}: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica el icono de la aplicación a una ventana después de que se haya creado
        /// </summary>
        /// <param name="form">Formulario al que aplicar el icono</param>
        public static void ApplyApplicationIcon(Form form)
        {
            if (form == null || !form.IsHandleCreated)
                return;

            try
            {
                var mainIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (mainIcon != null)
                {
                    SendMessage(form.Handle, WM_SETICON, (IntPtr)ICON_SMALL, mainIcon.Handle);
                    SendMessage(form.Handle, WM_SETICON, (IntPtr)ICON_BIG, mainIcon.Handle);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskbarGrouping] Error aplicando icono: {ex.Message}");
            }
        }
    }
}

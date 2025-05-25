using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HTPCAVRVolume
{
    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        public event EventHandler VolumeUpPressed;
        public event EventHandler VolumeDownPressed;
        public event EventHandler VolumeMutePressed;

        // Déclaration du délégué
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public GlobalKeyboardHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                switch (key)
                {
                    case Keys.VolumeUp:
                        VolumeUpPressed?.Invoke(this, EventArgs.Empty);
                        return (IntPtr)1;  // Bloque la touche, ne la propage PAS à Windows

                    case Keys.VolumeDown:
                        VolumeDownPressed?.Invoke(this, EventArgs.Empty);
                        return (IntPtr)1;  // Bloque la touche

                    case Keys.VolumeMute:
                        VolumeMutePressed?.Invoke(this, EventArgs.Empty);
                        return (IntPtr)1;  // Bloque la touche
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        #region PInvoke

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}

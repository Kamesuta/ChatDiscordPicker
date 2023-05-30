using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChatPresetTool
{
    public static class GlobalHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

        private static IntPtr _hookId = IntPtr.Zero;

        public delegate void KeyEvent(int vkCode);

        public static event KeyEvent KeyDownEvent;
        public static event KeyEvent KeyUpEvent;

        public static void EnableHook()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            {
                using (var currentModule = currentProcess.MainModule)
                {
                    var moduleHandle = NativeMethods.GetModuleHandle(currentModule?.ModuleName);
                    _hookId = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallbackProc, moduleHandle, 0);
                }
            }
        }

        public static void DisableHook()
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        // これを保持しておかないとdelegateがガベコレされて
        // System.ExecutionEngineExceptionという意味の分からない例外を吐いて落ちるようになる。
        // https://qiita.com/mitsu_at3/items/94807ee0b3bf34ffb6b2
        private static readonly NativeMethods.LowLevelKeyboardProc _hookCallbackProc = HookCallback;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyDownEvent?.Invoke(vkCode);
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyUpEvent?.Invoke(vkCode);
                }
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
using System;
using System.Text;

namespace ChatPresetTool
{
    public static class ActiveWindow
    {
        public static IntPtr GetActiveWindow()
        {
            return NativeMethods.GetForegroundWindow();
        }

        public static bool SetActiveWindow(IntPtr windowHandle)
        {
            return NativeMethods.SetForegroundWindow(windowHandle);
        }

        public static string GetClassName(IntPtr windowHandle)
        {
            var strTitle = string.Empty;
            var stringBuilder = new StringBuilder(256);
            if (NativeMethods.GetClassName(windowHandle, stringBuilder, stringBuilder.Capacity) > 0)
            {
                strTitle = stringBuilder.ToString();
            }
            return strTitle;
        }

        public static string GetWindowTitle(IntPtr windowHandle)
        {
            var strTitle = string.Empty;
            // Obtain the length of the text   
            var intLength = NativeMethods.GetWindowTextLength(windowHandle) + 1;
            var stringBuilder = new StringBuilder(intLength);
            if (NativeMethods.GetWindowText(windowHandle, stringBuilder, stringBuilder.Capacity) > 0)
            {
                strTitle = stringBuilder.ToString();
            }
            return strTitle;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCRatings
{
    class Native
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public static void MouseDragCapture(IntPtr handle)
        {
            ReleaseCapture();
            SendMessage(handle, Native.WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }
    }
}

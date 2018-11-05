using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using D = System.Diagnostics.Debug;
using Mat = OpenCvSharp.Mat;
using OCV = OpenCvSharp;

namespace Ghost
{
    public class Screen
    {
        #region legacy
        [DllImport("User32.dll", SetLastError = true, EntryPoint = "PrintWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        #endregion

        private IntPtr _trg;

        public Screen(Process process)
        {
            _trg = process.MainWindowHandle;
        }

        public bool TryCapture(out Mat shot)
        {
            return TryCapture(_trg, out shot);
        }
        public static bool TryCapture(in IntPtr handle, out Mat shot)
        {
            shot = new Mat();
            using (System.Drawing.Graphics gfxWin = System.Drawing.Graphics.FromHwnd(handle))
            {
                var rect = System.Drawing.Rectangle.Round(gfxWin.VisibleClipBounds);
                
                if (rect.Width == 0
                 || rect.Height == 0)
                {
                    D.Write("Target window is minimized");
                    shot.Dispose();
                    return false;
                }

                using (var bmp = new System.Drawing.Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                using (var gfxBmp = System.Drawing.Graphics.FromImage(bmp))
                {
                    int flag = (Environment.OSVersion.Version.Major * 100
                              + Environment.OSVersion.Version.Minor) > 603
                              ? 0x3 : 0x1;
                    IntPtr hdcBitmap = gfxBmp.GetHdc();
                    bool isPrinted = _PrintWindow(handle, hdcBitmap, 0x3);
                    gfxBmp.ReleaseHdc(hdcBitmap);

                    if (!isPrinted)
                    {
#if DEBUG
                        int err = Marshal.GetLastWin32Error();
                        var exception = new System.ComponentModel.Win32Exception(err);
                        D.Write(exception.Message);
#endif
                        shot.Dispose();
                        return false;
                    }

                    shot = OCV.Extensions.BitmapConverter.ToMat(bmp);
                }
            }

            return true;
        }
    }
}

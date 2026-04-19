using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AiPlays.Pilot.Services
{
    public class ScreenshotService
    {
        #region dll windows

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        #endregion

        private readonly string _screenshotPath;
        private Process? _emulatorProcess;

        public ScreenshotService(IOptions<EmulatorSettings> options)
        {
            _screenshotPath = options.Value.ScreenshotPath;
        }

        public void SetEmulatorProcess(Process process)
        {
            _emulatorProcess = process;
        }

        public async Task<byte[]> TakeScreenshot()
        {
            Directory.CreateDirectory(_screenshotPath);

            if (_emulatorProcess != null && !_emulatorProcess.HasExited)
            {
                var handle = _emulatorProcess.MainWindowHandle;

                if (GetWindowRect(handle, out RECT windowRect) && GetClientRect(handle, out RECT clientRect))
                {
                    int fullWidth = windowRect.Right - windowRect.Left;
                    int fullHeight = windowRect.Bottom - windowRect.Top;

                    int clientWidth = clientRect.Right - clientRect.Left;
                    int clientHeight = clientRect.Bottom - clientRect.Top - 21;

                    if (fullWidth > 0 && fullHeight > 0)
                    {
                        // Full windoww
                        using (Bitmap fullBmp = new Bitmap(fullWidth, fullHeight, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(fullBmp))
                            {
                                IntPtr hdc = g.GetHdc();
                                PrintWindow(handle, hdc, 2); // PW_RENDERFULLCONTENT
                                g.ReleaseHdc(hdc);
                            }

                            
                            int offsetX = (fullWidth - clientWidth) / 2;
                            int offsetY = (fullHeight - clientHeight) - offsetX;

                            // game window
                            using (Bitmap croppedBmp = fullBmp.Clone(new Rectangle(offsetX, offsetY, clientWidth, clientHeight), fullBmp.PixelFormat))
                            {
                                using (var ms = new MemoryStream())
                                {
                                    croppedBmp.Save(ms, ImageFormat.Png);
                                    byte[] bytes = ms.ToArray();

                                    //string fileName = $"snap_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                                    //string filePath = Path.Combine(_screenshotPath, fileName);

                                    //await File.WriteAllBytesAsync(filePath, bytes);

                                    return bytes;
                                }
                            }
                        }
                    }
                }
            }

            return Array.Empty<byte>();
        }
    }
}
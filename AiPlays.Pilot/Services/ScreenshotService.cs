using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace AiPlays.Pilot.Services
{
    public class ScreenshotService : BackgroundService
    {
        #region dll windows

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await TakeScreenshot(stoppingToken);
        }

        public void SetEmulatorProcess(Process process)
        {
            _emulatorProcess = process;
        }

        private async Task TakeScreenshot(CancellationToken stoppingToken)
        {
            Directory.CreateDirectory("Screenshots");

            while (!stoppingToken.IsCancellationRequested)
            {
                if(_emulatorProcess != null)
                {
                    var handle = _emulatorProcess.MainWindowHandle;
                   
                    if(GetWindowRect(handle, out RECT rect))
                    {
                        int width = rect.Right - rect.Left;
                        int height = rect.Bottom - rect.Top;

                        if (width <= 0 || height <= 0) return;

                        using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                IntPtr hdc = g.GetHdc();
                                PrintWindow(handle, hdc, 2); // 2 = PW_RENDERFULLCONTENT
                                g.ReleaseHdc(hdc);
                            }

                            string fileName = $"snap_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                            bmp.Save(Path.Combine(_screenshotPath, fileName), ImageFormat.Png);
                        }

                    }

                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

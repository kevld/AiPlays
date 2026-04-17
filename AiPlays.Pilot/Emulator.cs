using AiPlays.Core.Enums;
using AiPlays.Pilot.Services;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsInput.Native;

namespace AiPlays.Pilot
{
    public class Emulator
    {

        private readonly string _mgbaPath;
        private readonly string _romPath;
        private readonly CommandQueueService _commandQueue;
        private readonly ScreenshotService _screenshotService;

        private Process? _process;

        #region "Windows"

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private IntPtr _windowHandle;

        public IntPtr WindowHandle => _windowHandle;

        #endregion

        public Emulator(CommandQueueService commandQueue, ScreenshotService screenshotService, IOptions<EmulatorSettings> options)
        {
            _commandQueue = commandQueue;
            _screenshotService = screenshotService;
            _mgbaPath = options.Value.MgbaPath;
            _romPath = options.Value.RomPath;

            commandQueue.OnCommandProcessed += SendCommand;
        }

        public void Start()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _mgbaPath,
                    Arguments = _romPath,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            _process.Start();

            if (_process != null)
            {
                _process.WaitForInputIdle();

                // Try multiple times to get the main window handle
                int attempts = 0;
                while (_process.MainWindowHandle == IntPtr.Zero && attempts < 10)
                {
                    Thread.Sleep(200);
                    _process.Refresh();
                    attempts++;
                }

                _windowHandle = _process.MainWindowHandle;
                Console.WriteLine($"Handled on : {_windowHandle}");

                _screenshotService.SetEmulatorProcess(_process);

                // Load State
                Console.WriteLine("Emulator started. Waiting 5s...");
                Thread.Sleep(5000);
                Console.WriteLine("Load state 1");
                SendCommand(VirtualKeyCode.F1);
            }

        }

        public void SendCommand(VirtualKeyCode command)
        {
            if (_windowHandle == IntPtr.Zero) return;

            uint vk = (uint)command;
            uint scanCode = MapVirtualKey(vk, 0); // MAPVK_VK_TO_VSC

            // LParam 
            // KeyDown (Bit 30 & 31 to 0)
            IntPtr lParamDown = (IntPtr)((scanCode << 16) | 1);
            // KeyUp (Bit 30 & 31 to 1 => 0xC0000000)
            IntPtr lParamUp = (IntPtr)((scanCode << 16) | 0xC0000001);

            PostMessage(_windowHandle, WM_KEYDOWN, (IntPtr)vk, lParamDown);

            Task.Delay(25).ContinueWith(_ =>
            {
                PostMessage(_windowHandle, WM_KEYUP, (IntPtr)vk, lParamUp);
            });
        }

        public void HandleManualInput(string input)
        {
            GbaKey? key;

            #region input

            switch (input.ToLower())
            {
                case "a":
                    key = GbaKey.A;
                    break;
                case "b":
                    key = GbaKey.B;
                    break;
                case "l":
                    key = GbaKey.L;
                    break;
                case "r":
                    key = GbaKey.R;
                    break;
                case "start":
                    key = GbaKey.Start;
                    break;
                case "select":
                    key = GbaKey.Select;
                    break;
                case "up":
                    key = GbaKey.ArrowUp;
                    break;
                case "right":
                    key = GbaKey.ArrowRight;
                    break;
                case "down":
                    key = GbaKey.ArrowDown;
                    break;
                case "left":
                    key = GbaKey.ArrowLeft;
                    break;
                default:
                    Console.WriteLine("Invalid input. Please enter a valid command.");
                    return;
            }

            #endregion

            if (key.HasValue)
                _commandQueue.AddCommand(key.Value);
        }

        public void AddCommandToQueue(GbaKey action)
        {
            if (action != GbaKey.None)
            {
                _commandQueue.AddCommand(action);
            }
        }

        public async Task<byte[]> TakeScreenshot()
        {
            var result = await _screenshotService.TakeScreenshot();
            return result;
        }

        internal void SendGbaCommand(GbaKey action)
        {
            VirtualKeyCode? command = null;

            #region enum

            switch (action)
            {
                // Check mGBA keymap
                case GbaKey.A:
                    command = VirtualKeyCode.VK_X;
                    break;
                case GbaKey.B:
                    command = VirtualKeyCode.VK_Z;
                    break;
                case GbaKey.L:
                    command = VirtualKeyCode.VK_A;
                    break;
                case GbaKey.R:
                    command = VirtualKeyCode.VK_S;
                    break;
                case GbaKey.Start:
                    command = VirtualKeyCode.RETURN;
                    break;
                case GbaKey.Select:
                    command = VirtualKeyCode.BACK;
                    break;
                case GbaKey.ArrowUp:
                    command = VirtualKeyCode.UP;
                    break;
                case GbaKey.ArrowRight:
                    command = VirtualKeyCode.RIGHT;
                    break;
                case GbaKey.ArrowDown:
                    command = VirtualKeyCode.DOWN;
                    break;
                case GbaKey.ArrowLeft:
                    command = VirtualKeyCode.LEFT;
                    break;
                case GbaKey.None:
                    command = VirtualKeyCode.F1;
                    break;
                default:
                    break;
            }

            #endregion
            if (command.HasValue)
                SendCommand(command.Value);
        }
    }
}

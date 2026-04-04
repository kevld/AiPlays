using AiPlays.Pilot.Enums;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using WindowsInput.Native;

namespace AiPlays.Pilot.Services
{
    public class CommandQueueService : BackgroundService
    {
        private ConcurrentQueue<VirtualKeyCode> _commands;

        public event Action<VirtualKeyCode>? OnCommandProcessed;

        public CommandQueueService()
        {
            _commands = new ConcurrentQueue<VirtualKeyCode>();
        }

        private void AddCommandToQueue(VirtualKeyCode command)
        {
            _commands.Enqueue(command);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ProcessCommandQueue(stoppingToken);
        }

        private async Task ProcessCommandQueue(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                if(_commands.TryDequeue(out VirtualKeyCode command))
                {
                    OnCommandProcessed?.Invoke(command);
                }

                await Task.Delay(200, stoppingToken);
            }
        }

        public void AddCommand(GbaKey key)
        {
            VirtualKeyCode? command = null;

            #region enum

            switch (key)
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
                default:
                    break;
            }

            #endregion

            if (command.HasValue)
                AddCommandToQueue(command.Value);
        }
    }
}

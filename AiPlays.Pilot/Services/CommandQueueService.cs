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

        public void AddCommandToQueue(VirtualKeyCode command)
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
    }
}

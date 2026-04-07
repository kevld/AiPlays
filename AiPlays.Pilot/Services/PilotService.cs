using AiPlays.Core.Grpc;
using AiPlays.Core.Interfaces;

namespace AiPlays.Pilot.Services
{
    public class PilotService : IPilot
    {
        private readonly Emulator _emulator;

        public PilotService(Emulator emulator)
        {
            _emulator = emulator;
        }

        public void SendInput(GbaKey action)
        {
            SendInputAsync(action).GetAwaiter().GetResult();
        }

        public async Task SendInputAsync(GbaKey action)
        {
            _emulator.SendGbaCommand(action);

            await Task.Delay(250);
        }

        public byte[] TakeScreenshot()
        {
            return TakeScreenshotAsync().GetAwaiter().GetResult();
        }

        public async Task<byte[]> TakeScreenshotAsync()
        {
            var result = await _emulator.TakeScreenshot();
            return result ?? Array.Empty<byte>();
        }
    }
}

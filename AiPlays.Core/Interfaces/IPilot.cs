using AiPlays.Core.Grpc;

namespace AiPlays.Core.Interfaces
{
    public interface IPilot
    {
        public void SendInput(GbaKey action);
        public Task SendInputAsync(GbaKey action);
        public byte[] TakeScreenshot();
        public Task<byte[]> TakeScreenshotAsync();

    }
}

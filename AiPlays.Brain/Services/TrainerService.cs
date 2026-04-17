using AiPlays.Core.Data;
using AiPlays.Core.Enums;
using AiPlays.Core.Interfaces;
using Google.Protobuf;

namespace AiPlays.Brain.Services
{
    public class TrainerService
    {
        private readonly IPilot _pilot;
        private readonly IPerception _perception;

        public TrainerService(IPilot pilot, IPerception perception)
        {
            _pilot = pilot;
            _perception = perception;
        }

        public async Task<Observation> Step(GbaKey key)
        {
            Console.WriteLine($"At {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: Action : {key.ToString()}");
            // 1 : Action (input)
            await _pilot.SendInputAsync(key);

            // Wait for animation
            await Task.Delay(200);

            // 2 : Screenshot -> Current state (output)
            byte[] screenshot = await _pilot.TakeScreenshotAsync();

            //TODO : replace exploring with full state (fight, etc...)
            GameState gameState = _perception.GetGameState(screenshot);

            return new Observation()
            {
                ImageData = screenshot,
                GameState = gameState,
            };
        }
    }
}

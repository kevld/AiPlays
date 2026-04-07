using AiPlays.Core.Grpc;
using AiPlays.Core.Interfaces;
using Google.Protobuf;
using Grpc.Core;

namespace AiPlays.Brain.Services
{
    public class TrainerApiService : TrainerApi.TrainerApiBase
    {
        private readonly IPilot _pilot;
        private readonly IPerception _perception;

        public TrainerApiService(IPilot pilot, IPerception perception)
        {
            _pilot = pilot;
            _perception = perception;
        }

        public override async Task<Observation> Step(ActionRequest request, ServerCallContext context)
        {
            // 1 : Action (input)
            await _pilot.SendInputAsync(request.GbaKey);

            // Wait for animation
            await Task.Delay(500);

            // 2 : Screenshot -> Current state (output)
            byte[] screenshot = await _pilot.TakeScreenshotAsync();
            
            //TODO : replace exploring with full state (fight, etc...)
            GameState gameState = _perception.GetGameState(screenshot);

            return new Observation()
            {
                ImageData = ByteString.CopyFrom(screenshot),
                GameState = gameState,
            };
        }
    }
}

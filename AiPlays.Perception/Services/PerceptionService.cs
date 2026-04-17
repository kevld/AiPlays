using AiPlays.Core.Enums;
using AiPlays.Core.Extensions;
using AiPlays.Core.Interfaces;
using AiPlays_Perception;

namespace AiPlays.Perception.Services
{
    public class PerceptionService : IPerception
    {
        public GameState GetGameState(byte[] screenshot)
        {
            string predictedLabel = PredictState(screenshot);
            return EnumExtension.ToEnum<GameState>(predictedLabel);
        }

        public async Task<GameState> GetGameStateAsync(byte[] screenshot)
        {
            return await Task.Run(() => GetGameState(screenshot));
        }

        private string PredictState(byte[] screenshot)
        {
            string predictedLabel = string.Empty;

            if (screenshot.Length > 0)
            {
                var sampleData = new GameStateClassifier.ModelInput()
                {
                    ImageSource = screenshot
                };

                var prediction = GameStateClassifier.Predict(sampleData);
                predictedLabel = prediction.PredictedLabel;
            }

            Console.WriteLine($"At {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}, predicted : {predictedLabel}");

            return predictedLabel;
        }
    }
}

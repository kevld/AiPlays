using AiPlays.Core.Extensions;
using AiPlays.Core.Grpc;
using AiPlays.Pilot.Services;
using AiPlays_Perception;
using Microsoft.Extensions.Options;

namespace AiPlays.Brain
{
    public class Brain
    {
        private readonly string _screenshotPath;
        private readonly CommandQueueService _commandQueueService;

        public Brain(IOptions<BrainSettings> options, CommandQueueService commandQueueService)
        {
            _screenshotPath = options.Value.ScreenshotPath;
            _commandQueueService = commandQueueService;
        }

        public void Start()
        {
            Console.WriteLine("Brain started.");
            Loop();
        }

        private void Loop()
        {
            for (int i = 0; i < 30; i++)
            {
                GameState gameState = GetState();

                switch (gameState)
                {
                    case GameState.Cutscene:
                    case GameState.Dialogue:
                        GbaKey gbaKey = GbaKey.A;
                        _commandQueueService.AddCommand(gbaKey);
                        Console.WriteLine("Cutscene/Dialogue : A");
                        // Press a
                        break;
                    case GameState.Exploration:
                        Console.WriteLine("Exploration");
                        break;
                    case GameState.Fight:
                        Console.WriteLine("Fight");
                        break;
                    case GameState.Interaction:
                        Console.WriteLine("Interaction");
                        break;
                    case GameState.Menu:
                        Console.WriteLine("Menu");
                        break;
                    default:
                        Console.WriteLine("Unkown");
                        break;
                }

                Thread.Sleep(1000);
            }

        }

        private GameState GetState()
        {
            string predictedLabel = PredictState();
            return EnumExtension.ToEnum<GameState>(predictedLabel);
        }

        private byte[] GetLatestScreenshot()
        {
            byte[] screenshot = [];

            string? latestScrenshot = Directory.GetFiles(_screenshotPath, "*.png")
                .OrderByDescending(f => File.GetCreationTime(f))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(latestScrenshot))
            {
                try
                {
                    screenshot = File.ReadAllBytes(latestScrenshot);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return screenshot;
        }

        private string PredictState()
        {
            string predictedLabel = string.Empty;

            byte[] screenshot = GetLatestScreenshot();

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

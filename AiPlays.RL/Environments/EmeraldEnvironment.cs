using AiPlays.Brain.Services;
using AiPlays.Core.Data;
using AiPlays.Core.Enums;
using AiPlays.RL.Data;
using RLMatrix.Toolkit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TorchSharp;
using System.IO;

namespace AiPlays.RL.Environments
{
    [RLMatrixEnvironment]
    public partial class EmeraldEnvironment
    {
        private ObservableState? _previousState;
        private ObservableState? _newState;
        private Observation _previousObservation;
        private Observation _newObservation;
        private byte[,] _previousFrame;
        private byte[,] _newFrame;

        public float TotalReward { get; private set; } = 0f;

        private GbaKey _keyPressed;

        private bool _isDone;

        [RLMatrixObservation]
        public float[] PreviousState() => new float[2] { _previousState?.HasMoved ?? 0, _previousState?.GameState ?? 0};

        [RLMatrixObservation]
        public float[] NewState() => new float[2] { _newState?.HasMoved ?? 0, _newState?.GameState ?? 0 };

        [RLMatrixObservation]
        public int KeyPressed() => (int)_keyPressed;

        private readonly GbaKey[] _availableActions = [
            GbaKey.A,
            GbaKey.B,
            GbaKey.Select,
            GbaKey.Start,
            GbaKey.ArrowRight,
            GbaKey.ArrowLeft,
            GbaKey.ArrowUp,
            GbaKey.ArrowDown,
        ];

        private readonly TrainerService _trainerService;

        public EmeraldEnvironment(TrainerService trainerService)
        {
            _trainerService = trainerService;
            _previousState = null;
            _newState = null;
        }

        [RLMatrixActionDiscrete(8)]
        public void MakeAction(int action)
        {
            _keyPressed = _availableActions[action];
            Console.WriteLine($"Key pressed : {_keyPressed}");

            _newObservation = _trainerService.Step(_keyPressed).GetAwaiter().GetResult();
            //t.GetAwaiter().GetResult();
            //_newObservation = _trainerService.Step(_keyPressed).Result;
            //_newObservation = await _trainerService.Step(_keyPressed);
            _previousState = _newState;
            _newFrame = ProcessImage(_newObservation.ImageData);
            //SaveFrameAsPng(_newFrame, $"transformed/frame_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
            _newState = new ObservableState()
            {
                GameState = (int)_newObservation.GameState,
                HasMoved = HasMoved() ? 1 : 0
            };

            _previousObservation = _newObservation;
            _previousFrame = _newFrame;

        }

        private static void SaveFrameAsPng(byte[,] frame, string path)
        {
            int height = frame.GetLength(0);
            int width = frame.GetLength(1);

            using var image = new Image<L8>(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    image[x, y] = new L8(frame[y, x]);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            image.Save(path); // format inferred from extension
        }

        private bool HasMoved()
        {
            if (_previousFrame == null || _newFrame == null) return false;

            int height = _newFrame.GetLength(0);
            int width = _newFrame.GetLength(1);

            int significantChanges = 0;
            int threshold = 25;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (Math.Abs((int)_newFrame[y, x] - (int)_previousFrame[y, x]) > threshold)
                    {
                        significantChanges++;
                    }
                }
            }

            // return true if average per-pixel change indicates movement
            float percentChanged = (significantChanges / (float)(width * height)) * 100;
            return percentChanged > 15.0f;
        }

        private static byte[,] ProcessImage(byte[] imageData)
        {
            const int size = 84;
            var output = new byte[size, size];

            using var image = Image.Load<Rgba32>(imageData);
            image.Mutate(x => x.Resize(size, size).Grayscale());

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Rgba32 p = image[x, y];
                    // Since image is grayscale, R == G == B; take R
                    output[y, x] = p.R;
                }
            }

            return output;
        }



        [RLMatrixReward]
        public float GiveReward()
        {
            float reward = 0;

            if (_newObservation == null && _newObservation != null)
            {
                reward += .1f;
            }

            if (_newObservation != null && _newObservation != null)
            {
                if (_newObservation.GameState == GameState.Exploration)
                {
                    reward += 1f;
                }

                if (_keyPressed == GbaKey.Select && _newObservation.GameState == GameState.Dialogue)
                {
                    reward -= 20f;
                    _isDone = true;
                }

            }

            TotalReward += reward;
            return reward;
        }

        [RLMatrixDone]
        public bool IsDone() => _isDone;

        [RLMatrixReset]
        public async Task ResetEnvironment()
        {
            _previousState = null;
            _newState = null;
            _isDone = false;
            TotalReward = 0f;
            await Task.Delay(1000);
            await _trainerService.Step(GbaKey.None);
            await Task.Delay(1000);

        }

    }
}

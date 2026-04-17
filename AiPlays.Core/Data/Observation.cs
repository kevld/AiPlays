using AiPlays.Core.Enums;

namespace AiPlays.Core.Data
{
    public class Observation
    {
        public byte[]? ImageData { get; set; }
        public GameState GameState { get; set; }

    }
}

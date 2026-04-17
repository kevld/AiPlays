using AiPlays.Core.Enums;

namespace AiPlays.Core.Interfaces
{
    public interface IPerception
    {
        public GameState GetGameState(byte[] screenshot);

        public Task<GameState> GetGameStateAsync(byte[] screenshot);
    }
}

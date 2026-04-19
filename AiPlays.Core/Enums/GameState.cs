/// <summary>
/// Represents different states of the game.
/// </summary>
namespace AiPlays.Core.Enums
{
    /// <summary>
    /// Enumerates various game states.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// The initial state of the game.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// State during cutscenes.
        /// </summary>
        Cutscene = 1,

        /// <summary>
        /// State during dialogue.
        /// </summary>
        Dialogue = 2,

        /// <summary>
        /// State during exploration.
        /// </summary>
        Exploration = 3,

        /// <summary>
        /// State during fights.
        /// </summary>
        Fight = 4,

        /// <summary>
        /// State during interactions.
        /// </summary>
        Interaction = 5,

        /// <summary>
        /// State during menus.
        /// </summary>
        Menu = 6
    }
}

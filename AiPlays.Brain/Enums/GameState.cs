using System;
using System.Collections.Generic;
using System.Text;

namespace AiPlays.Brain.Enums
{
    public enum GameState
    {
        Unknown = 0,
        Cutscene,
        Dialogue,
        Exploration,
        Fight,
        Interaction,
        Menu
    }
}

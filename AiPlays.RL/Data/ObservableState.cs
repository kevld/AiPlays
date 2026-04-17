using AiPlays.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiPlays.RL.Data
{
    public class ObservableState
    {
        public int HasMoved { get; set; }
        public int GameState { get; set; }
    }
}

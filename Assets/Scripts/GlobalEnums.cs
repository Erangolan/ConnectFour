using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEnums
{
    public enum SlotState { Empty, Red, Yellow, Black };

    public enum MatchState { NoWinner, Winner, Tie };

    public enum Opponent { AI, Player };
}

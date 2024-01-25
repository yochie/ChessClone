using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static PlayerColor GetOpponentColor(PlayerColor playerColor)
    {
        if (playerColor == PlayerColor.white)        
            return PlayerColor.black;        
        else
            return PlayerColor.white;
    }
}

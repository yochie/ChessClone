using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPieceType
{
    public PieceTypeID ForPieceTypeID { get; set; }
    public List<BoardPosition> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition);
}

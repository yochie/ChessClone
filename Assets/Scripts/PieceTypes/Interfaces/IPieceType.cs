using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPieceType
{
    public PieceTypeID ForPieceTypeID { get; set; }
    public List<Move> GetPossibleMovesFrom(SyncedGameState gameState, BoardPosition fromPosition);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamePieceState
{
    public bool PositionHoldsAPiece(BoardPosition position);

    public GamePieceID GetPieceAtPosition(BoardPosition fromPosition);

    public bool IsOwnerOfPieceAtPosition(BoardPosition position, PlayerColor playerColor);

    public List<Move> GetPossibleMovesFrom(BoardPosition boardPosition);
}

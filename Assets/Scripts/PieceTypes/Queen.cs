using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Queen : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<Move> validMoves = new();

        return validMoves;
    }
}

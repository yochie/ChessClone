using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Knight : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition, bool threateningMovesOnly = false)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<Move> possibleMoves = new();
        List<Vector2Int> offsets = new() { 
            new(2, 1),
            new(2, -1),
            new(-2, 1),
            new(-2, -1),
            new(1, 2),
            new(1, -2),
            new(-1, 2),
            new(-1, -2)
        };
        foreach (Vector2Int offset in offsets)
        {
            BoardPosition pos = fromPosition.Add(offset);
            if (!pos.IsOnBoard())
                continue;

            if (gameState.PositionHoldsAPiece(pos))
            {
                //can eat that piece
                if (!gameState.IsOwnerOfPieceAtPosition(pos, moverColor))
                    possibleMoves.Add(new Move(fromPosition, pos, eats: true, eatPosition: pos));
            }
            else
            {
                if (!threateningMovesOnly)
                    possibleMoves.Add(new Move(fromPosition, pos, eats: false));
            }

        }
        return possibleMoves;
    }
}

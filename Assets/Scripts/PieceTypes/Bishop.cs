using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Bishop : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<Move> validMoves = new();
        List<Vector2Int> directions = new() { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
        foreach (Vector2Int direction in directions)
        {
            for (BoardPosition pos = fromPosition.Add(direction); pos.IsOnBoard(); pos = pos.Add(direction))
            {
                if (gameState.PositionHoldsAPiece(pos))
                {
                    //can eat that piece
                    if (!gameState.IsOwnerOfPieceAtPosition(pos, moverColor))
                        validMoves.Add(new Move(fromPosition, pos, eats: true, eatPosition: pos));
                    break;
                }
                else
                {
                    validMoves.Add(new Move(fromPosition, pos, eats: false));
                }
            }
        }
        return validMoves;
    }
}

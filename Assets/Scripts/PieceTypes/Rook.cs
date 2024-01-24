using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Rook : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(SyncedGameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<Move> possibleMoves = new();
        List<Vector2Int> directions = new() { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        foreach(Vector2Int direction in directions)
        {
            for (BoardPosition pos = fromPosition.Add(direction); pos.IsOnBoard(); pos = pos.Add(direction))
            {
                if (gameState.PositionHoldsAPiece(pos))
                {
                    //can eat that piece
                    if (!gameState.IsOwnerOfPieceAtPosition(pos, moverColor))
                        possibleMoves.Add(new Move(fromPosition, pos, eats : true, eatPosition: pos));
                    break;
                }
                else
                {
                    possibleMoves.Add(new Move(fromPosition, pos, eats: false));
                }
            }
        }

        return possibleMoves;
    }
}

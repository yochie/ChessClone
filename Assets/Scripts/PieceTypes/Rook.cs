using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Rook : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<BoardPosition> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<BoardPosition> validMoves = new();
        List<Vector2Int> directions = new() { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        foreach(Vector2Int direction in directions)
        {
            //go right until you hit a piece
            IEnumerable<int> horizontalRange = Enumerable.Range(0, BoardPosition.maxX);
            IEnumerable<int> verticalRange = Enumerable.Range(0, BoardPosition.maxY);
            for (BoardPosition pos = fromPosition.Add(direction);
                horizontalRange.Contains(pos.xPosition) && verticalRange.Contains(pos.yPosition);
                pos.Add(direction))
            {
                if (gameState.PositionHoldsAPiece(pos))
                {
                    //can eat that piece
                    if (!gameState.IsOwnerOfPieceAtPosition(pos, moverColor))
                        validMoves.Add(pos);
                    break;
                }
                else
                {
                    validMoves.Add(pos);
                }
            }
        }

        return validMoves;
    }
}

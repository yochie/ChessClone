using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class King : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<BoardPosition> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<BoardPosition> validMoves = new();

        //go right until you hit a piece
        for (int x = fromPosition.xPosition + 1; x <= BoardPosition.maxX; x++)
        {
            BoardPosition pos = new BoardPosition((short) x, fromPosition.yPosition);
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

        //go left until you hit a piece
        for (int x = fromPosition.xPosition - 1; x >= 0; x--)
        {
            BoardPosition pos = new BoardPosition((short) x, fromPosition.yPosition);
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

        //go down until you hit a piece
        for (int y = fromPosition.yPosition - 1; y >= 0; y--)
        {
            BoardPosition pos = new BoardPosition(fromPosition.xPosition, (short) y);
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

        //go down until you hit a piece
        for (int y = fromPosition.yPosition + 1; y <= BoardPosition.maxY; y++)
        {
            BoardPosition pos = new BoardPosition(fromPosition.xPosition, (short) y);
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

        return validMoves;
    }
}

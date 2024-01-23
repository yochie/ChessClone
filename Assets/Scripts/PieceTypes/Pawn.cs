using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Pawn : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<BoardPosition> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<BoardPosition> validMoves = new();
        GamePieceID pawnID = gameState.GetPieceAtPosition(fromPosition);
        int yDirection = moverColor == PlayerColor.white ? 1 : -1;

        //Can move forward by one if its empty
        BoardPosition destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + yDirection);
        if (!gameState.PositionHoldsAPiece(destinationPosition))
            validMoves.Add(destinationPosition);

        //Can move forward by two if its empty on first move
        if (!gameState.HasPawnMoved(pawnID))
        {
            destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + 2*yDirection);
            if (!gameState.PositionHoldsAPiece(destinationPosition))
                validMoves.Add(destinationPosition);
        }

        //Can move to diagonal by two if eating piece
        List<int> xDirections = new() { -1, 1};
        foreach (int xDirection in xDirections)
        {
            destinationPosition = new BoardPosition(fromPosition.xPosition + xDirection, fromPosition.yPosition + yDirection);
            if (gameState.PositionHoldsAPiece(destinationPosition) &&
                !gameState.IsOwnerOfPieceAtPosition(destinationPosition, moverColor))
                validMoves.Add(destinationPosition);
        }

        return validMoves;
    }
}

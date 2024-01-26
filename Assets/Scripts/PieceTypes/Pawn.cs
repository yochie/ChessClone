using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Pawn : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition, bool threateningMovesOnly = false)
    {
        PlayerColor moverColor = gameState.GetPieceAtPosition(fromPosition).color;
        List<Move> possibleMoves = new();
        GamePieceID pawnID = gameState.GetPieceAtPosition(fromPosition);
        int yDirection = moverColor == PlayerColor.white ? 1 : -1;

        //Forward 1
        BoardPosition destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + yDirection);
        if (destinationPosition.IsOnBoard() && !gameState.PositionHoldsAPiece(destinationPosition))
            possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: false));

        //Forward 2
        if (!gameState.HasPieceMoved(pawnID))
        {
            BoardPosition intermediateTile = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + yDirection);
            destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + 2*yDirection);
            if (intermediateTile.IsOnBoard() &&
                !gameState.PositionHoldsAPiece(intermediateTile) &&
                destinationPosition.IsOnBoard() &&
                !gameState.PositionHoldsAPiece(destinationPosition))
                possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: false));
        }

        //Diagonal 1
        if (!threateningMovesOnly)
            return possibleMoves;

        List<int> xDirections = new() { -1, 1};
        foreach (int xDirection in xDirections)
        {
            destinationPosition = new BoardPosition(fromPosition.xPosition + xDirection, fromPosition.yPosition + yDirection);
            if (destinationPosition.IsOnBoard() && 
                gameState.PositionHoldsAPiece(destinationPosition) &&
                !gameState.IsOwnerOfPieceAtPosition(destinationPosition, moverColor))
                possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: true, eatPosition: destinationPosition));
        }

        return possibleMoves;
    }
}

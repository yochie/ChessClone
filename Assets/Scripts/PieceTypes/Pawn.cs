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


        if (!threateningMovesOnly)
        {
            //Forward 1
            BoardPosition destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + yDirection);
            if (destinationPosition.IsOnBoard() && !gameState.PositionHoldsAPiece(destinationPosition))
                possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: false));

            //Forward 2
            if (!gameState.HasPieceMoved(pawnID))
            {
                BoardPosition intermediateTile = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + yDirection);
                destinationPosition = new BoardPosition(fromPosition.xPosition, fromPosition.yPosition + 2 * yDirection);
                if (intermediateTile.IsOnBoard() &&
                    !gameState.PositionHoldsAPiece(intermediateTile) &&
                    destinationPosition.IsOnBoard() &&
                    !gameState.PositionHoldsAPiece(destinationPosition))
                    possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: false));
            }
        }

        //Diagonal 1
        List<int> xDirections = new() { -1, 1};
        foreach (int xDirection in xDirections)
        {
            BoardPosition destinationPosition = new BoardPosition(fromPosition.xPosition + xDirection, fromPosition.yPosition + yDirection);
            //Normal eat
            if (destinationPosition.IsOnBoard() && 
                gameState.PositionHoldsAPiece(destinationPosition) &&
                !gameState.IsOwnerOfPieceAtPosition(destinationPosition, moverColor))
            {
                possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: true, eatPosition: destinationPosition));
                continue;
            }

            //En passant eat
            BoardPosition enPassantEatsPosition = fromPosition.Add(new(xDirection, 0));
            if (destinationPosition.IsOnBoard() &&
                !gameState.PositionHoldsAPiece(destinationPosition) &&
                enPassantEatsPosition.IsOnBoard() &&
                gameState.PositionHoldsAPiece(enPassantEatsPosition))
            {
                GamePieceID eatenPiece = gameState.GetPieceAtPosition(enPassantEatsPosition);

                //Debug.LogFormat("Considering En passant for {0} at pos {1} eating {2} at pos {3}", pawnID, fromPosition, eatenPiece, enPassantEatsPosition);

                if (eatenPiece.typeID != PieceTypeID.pawn)
                {
                    continue;
                }
                if (eatenPiece.color == moverColor)
                {
                    continue;
                }
                if (!gameState.DidPawnJustAdvanceBy2(eatenPiece))
                {
                    continue;
                }
                possibleMoves.Add(new Move(fromPosition, destinationPosition, eats: true, eatPosition: enPassantEatsPosition));
            }

        }

        return possibleMoves;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class King : ScriptableObject, IPieceType
{
    [field: SerializeField]
    public PieceTypeID ForPieceTypeID { get; set; }

    public List<Move> GetPossibleMovesFrom(GameState gameState, BoardPosition fromPosition, bool threateningMovesOnly = false)
    {
        GamePieceID kingPieceID = gameState.GetPieceAtPosition(fromPosition);
        PlayerColor moverColor = kingPieceID.color;
        
        List<Move> possibleMoves = new();

        List<Vector2Int> directions = new()
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1),
            new(1, 1),
            new(-1, -1),
            new(1, -1),
            new(-1, 1)
        };
        foreach (Vector2Int direction in directions)
        {

            BoardPosition pos = fromPosition.Add(direction);
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

        //skip Castling when checking for threatening moves only
        //this avoids infinite recursion when analyzing moves that would threaten castling positions
        if (threateningMovesOnly)
            return possibleMoves;

        //Castling
        foreach (CastleType castyleType in new List<CastleType> { CastleType.kingside, CastleType.queenside })
        {
            Move? castleMove = GetCastleMove(castyleType, kingPieceID, fromPosition, gameState);
            if (castleMove == null)
                continue;
            else
                possibleMoves.Add(castleMove.GetValueOrDefault());
        }

        return possibleMoves;
    }

    private Move? GetCastleMove(CastleType castleType, GamePieceID kingPieceID, BoardPosition fromKingPosition, GameState gameState)
    {
        if (gameState.HasPieceMoved(kingPieceID))
            return null;
        if (gameState.GetCheckedPlayers().Contains(kingPieceID.color))
            return null;

        Vector2Int distanceToRook;
        Vector2Int dir;
        if (castleType == CastleType.kingside) {
            distanceToRook = new Vector2Int(3, 0);
            dir = new Vector2Int(1, 0);
        } else
        {
            distanceToRook = new Vector2Int(-4, 0);
            dir = new Vector2Int(-1, 0);
        }

        //validate rook ID and make sure it hasnt moved
        BoardPosition rookPosition = fromKingPosition.Add(distanceToRook);
        if (!gameState.PositionHoldsAPiece(rookPosition))
            return null;
        GamePieceID rookPieceID = gameState.GetPieceAtPosition(rookPosition);
        if (rookPieceID.typeID != PieceTypeID.rook ||
            rookPieceID.color != kingPieceID.color ||
            gameState.HasPieceMoved(rookPieceID))
            return null;

        //All positions king and rook must be empty
        for (BoardPosition position = fromKingPosition.Add(dir); !position.Equals(rookPosition); position = position.Add(dir))
        {
            if (gameState.PositionHoldsAPiece(position))
                return null;
        }

        //All positions moved through by king must be unthreatened
        BoardPosition testingPosition = fromKingPosition;
        for (int i = 0; i < 2; i++)
        {
            testingPosition = testingPosition.Add(dir);
            if (gameState.KingThreatenedAtPosition(Utility.GetOpponentColor(kingPieceID.color), fromKingPosition, testingPosition))
                return null;
        }

        BoardPosition kingDestination = fromKingPosition.Add(2 * dir);
        BoardPosition rookDestination = kingDestination.Add(-1 * dir);
        Move castlingMove = new Move(from: fromKingPosition,
                                     to: kingDestination,
                                     eats: false,
                                     includesSecondaryMove: true,
                                     from2: rookPosition,
                                     to2: rookDestination);
        
        return castlingMove;
    }
}


public enum CastleType
{
    kingside, queenside
}
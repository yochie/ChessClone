using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private PieceTypeData pieceTypes;

    private Dictionary<BoardPosition, GamePieceID> gamePieces;

    private Dictionary<GamePieceID, bool> pawnHasMoved;

    private PlayerColor playerTurn;

    private Dictionary<BoardPosition, List<Move>> possibleMoves;

    private Dictionary<PlayerColor, bool> playerChecked;

    public GameState(PieceTypeData pieceTypes,
                     Dictionary<BoardPosition, GamePieceID> gamePieces,
                     Dictionary<GamePieceID, bool> pawnHasMoved,
                     PlayerColor playerTurn,
                     Dictionary<BoardPosition, List<Move>> possibleMoves,
                     Dictionary<PlayerColor, bool> playerChecked)
    {
        this.pieceTypes = pieceTypes;
        this.gamePieces = gamePieces;
        this.pawnHasMoved = pawnHasMoved;
        this.playerTurn = playerTurn;
        this.possibleMoves = possibleMoves;
        this.playerChecked = playerChecked;
    }
}

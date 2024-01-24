using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private PieceTypeData pieceTypes;

    private Dictionary<BoardPosition, GamePieceID> gamePieces;

    private Dictionary<GamePieceID, bool> pawnHasMoved;

    private PlayerColor playerTurn;


    private readonly Dictionary<BoardPosition, List<Move>> possibleMoves;

    private readonly Dictionary<PlayerColor, bool> playerChecked;

    public GameState(SyncedGameState syncedGameState)
    {

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using System;

//Raw data that defines game logical state
public class SyncedGameState : NetworkBehaviour, IGamePieceState
{
    #region Synced vars
    [SyncVar]
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    private readonly SyncDictionary<BoardPosition, GamePieceID> gamePieces = new();

    //TODO : use different structure to avoid need to replacing whole lists at every update
    //seems fine for now, not that many possible moves each turn and they are almost all changing on update anyways...
    private readonly SyncDictionary<BoardPosition, List<Move>> possibleMoves = new();
    #endregion

    #region State management

    //will perform deep cloning on arguments to avoid passing references
    [Server]
    public void Init(GameState serverGameState)
    {
        this.playerTurn = serverGameState.PlayerTurn;

        foreach (var (position, gamePieceID) in serverGameState.GetGamePiecesClone())
        {
            this.gamePieces.Add(position, gamePieceID);
        }

        foreach (var (position, moveList) in serverGameState.GetPossibleMovesClone())
        {
            //create new list to avoid passing reference
            this.possibleMoves.Add(position, new List<Move>(moveList));
        }
    }

    [Server]

    public void UpdateState(GameState serverGameState)
    {
        this.playerTurn = serverGameState.PlayerTurn;

        Dictionary<BoardPosition, GamePieceID> serverGamePieces = serverGameState.GetGamePiecesClone();
        //remove any gamepiece no longer needed
        foreach (var position in this.gamePieces.Keys.ToList())
        {
            if (!serverGamePieces.ContainsKey(position))
                this.gamePieces.Remove(position);
        }
        //add or update gamepieces from server state
        foreach (var (position, gamePieceID) in serverGamePieces)
        {
            if (this.gamePieces.ContainsKey(position))
                this.gamePieces[position] = gamePieceID;
            else
                this.gamePieces.Add(position, gamePieceID);
        }

        Dictionary<BoardPosition, List<Move>> serverPossibleMoves = serverGameState.GetPossibleMovesClone();
        //remove any moves from positions no longer needed
        foreach (var position in this.possibleMoves.Keys.ToList())
        {
            if (!serverPossibleMoves.ContainsKey(position))
                this.possibleMoves.Remove(position);
        }

        //add or update gamepieces from server state
        foreach (var (position, movelist) in serverPossibleMoves)
        {
            //create new lists to avoid passing ref
            if (this.possibleMoves.ContainsKey(position))
                this.possibleMoves[position] = new (movelist);
            else
                this.possibleMoves.Add(position, new(movelist));
        }
    }
    #endregion

    #region IGamePieceState

    public bool PositionHoldsAPiece(BoardPosition position)
    {
        return this.gamePieces.ContainsKey(position);
    }

    public GamePieceID GetPieceAtPosition(BoardPosition fromPosition)
    {
        if (!this.gamePieces.ContainsKey(fromPosition))
        {
            Debug.Log("No piece at requested position. Make sure you validate before sending request.");
            return new GamePieceID(PlayerColor.white, PieceTypeID.none, -1);
        }
        return gamePieces[fromPosition];
    }

    public bool IsOwnerOfPieceAtPosition(BoardPosition position, PlayerColor playerColor)
    {
        if (!this.gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece at given position to determine owner");
            return false;
        }
        return this.gamePieces[position].color == playerColor;
    }

    public List<Move> GetPossibleMovesFrom(BoardPosition boardPosition)
    {
        if (!this.possibleMoves.ContainsKey(boardPosition))
        {
            Debug.Log("No possible moves set for given position.");
            return new List<Move>();
        }
        return this.possibleMoves[boardPosition];
    }

    #endregion
}
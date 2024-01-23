using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using System;

//Raw data that defines game logical state
public class GameState : NetworkBehaviour
{
    private readonly SyncDictionary<BoardPosition, GamePieceID> gamePieces = new();
    
    [SyncVar]
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    public void Init(Dictionary<BoardPosition, GamePieceID> gamePieces, PlayerColor playerTurn)
    {
        foreach(var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
        }
        this.playerTurn = playerTurn;
    }

    #region State modifiers
    [Server]
    internal void MovePiece(BoardPosition from, BoardPosition to)
    {
        if (!gamePieces.ContainsKey(from))
        {
            Debug.Log("Couldn't find piece for tile");
        }
        GamePieceID toMove = this.gamePieces[from];
        this.gamePieces.Remove(from);
        this.gamePieces[to] = toMove;
    }

    [Server]
    internal void DeletePieceAt(BoardPosition position)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece to remove");
        }
        this.gamePieces.Remove(position);
    }

    [Server]
    public void ChangeTurn()
    {
        if (this.playerTurn == PlayerColor.white)
            this.playerTurn = PlayerColor.black;
        else
            this.playerTurn = PlayerColor.white;
    }
    #endregion

    #region Utility/Getters
    public bool PositionHoldsAPiece(BoardPosition position)
    {
        return this.gamePieces.ContainsKey(position);
    }

    public bool IsOwnerOfPieceAtPosition(BoardPosition position, PlayerColor playerColor)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece for tile");
            return false;
        }
        GamePieceID tilePiece = this.gamePieces[position];

        if (tilePiece.color == playerColor)
            return true;
        else
            return false;
    }

    internal bool IsValidMove(BoardPosition startPosition, BoardPosition endPosition)
    {
        //todo : validate move
        if (startPosition.Equals(endPosition))
        {
            Debug.Log("Invalid move : start == end");
            return false;
        }
        return true;
    }
    #endregion


}

public readonly struct GamePieceID
{
    public readonly PlayerColor color;
    public readonly PieceTypeID typeID;
    public readonly int index;

    public GamePieceID(PlayerColor ownerColor, PieceTypeID typeID, int index)
    {
        this.color = ownerColor;
        this.typeID = typeID;
        this.index = index;
    }
}

public enum PlayerColor
{
    white, black
}

public enum PieceTypeID
{
    rook, knight, bishop, king, queen, pawn
}
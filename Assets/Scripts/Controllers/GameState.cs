using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using System;

//Raw data that defines game logical state
public class GameState : NetworkBehaviour
{
    [SerializeField]
    private PieceTypeData pieceTypes;

    private readonly SyncDictionary<BoardPosition, GamePieceID> gamePieces = new();

    [SyncVar]
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    private readonly SyncDictionary<BoardPosition, List<BoardPosition>> possibleMoves = new();

    [Server]
    public void Init(Dictionary<BoardPosition, GamePieceID> gamePieces, PlayerColor playerTurn)
    {
        foreach(var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
        }
        this.playerTurn = playerTurn;
        this.UpdatePossibleMoves();
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
        this.UpdatePossibleMoves();
    }

    [Server]
    internal void DeletePieceAt(BoardPosition position)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece to remove");
        }
        this.gamePieces.Remove(position);
        this.UpdatePossibleMoves();
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

        if(!possibleMoves.ContainsKey(startPosition))
        {
            Debug.Log("Invalid move : start position is not set as possible move");
            return false;
        }

        if (!possibleMoves[startPosition].Contains(endPosition))
        {
            Debug.Log("Invalid move : end position is not set as possible move from start");
            return false;
        }
        return true;
    }

    internal GamePieceID GetPieceAtPosition(BoardPosition fromPosition)
    {
        if (!this.gamePieces.ContainsKey(fromPosition))
        {
            Debug.Log("No piece at requested position. Make sure you validate before sending request.");
            return new GamePieceID(PlayerColor.white, PieceTypeID.none, -1);
        }
        return gamePieces[fromPosition];
    }

    internal List<BoardPosition> GetPossibleMovesFrom(BoardPosition boardPosition)
    {

        if (!this.possibleMoves.ContainsKey(boardPosition))
        {
            Debug.Log("No possible moves set for given position.");
            return new List<BoardPosition>();
        }
        return this.possibleMoves[boardPosition];
    }

    public void UpdatePossibleMoves()
    {
        this.possibleMoves.Clear();
        foreach(var (position, gamePieceID) in this.gamePieces)
        {
            IPieceType pieceType = this.pieceTypes.GetPieceTypeForByID(gamePieceID.typeID);
            List<BoardPosition> possibleMovesFromPosition = pieceType.GetPossibleMovesFrom(this, position);
            //TODO: filter out moves that would put you in check mate
            this.possibleMoves.Add(position, possibleMovesFromPosition);
        }
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

    public override string ToString()
    {
        return string.Format("{0} {1} ({2})", color, typeID, index);
    }
}

public enum PlayerColor
{
    white, black
}

public enum PieceTypeID
{
    rook, knight, bishop, king, queen, pawn, none
}
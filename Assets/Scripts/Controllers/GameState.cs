using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct GameState
{
    public Dictionary<BoardPosition, GamePiece> gamePieces;
    public PlayerColor playerTurn;

    public GameState(Dictionary<BoardPosition, GamePiece> gamePieces, PlayerColor playerTurn)
    {
        this.gamePieces = gamePieces;
        this.playerTurn = playerTurn;
    }

    public bool TileHoldsPiece(BoardPosition position)
    {
        return this.gamePieces.ContainsKey(position);
    }

    internal bool IsTilePieceOwner(BoardPosition boardPosition, PlayerColor playerColor)
    {
        if (!gamePieces.ContainsKey(boardPosition))
        {
            Debug.Log("Couldn't find piece for tile");
            return false;
        }
        GamePiece tilePiece = this.gamePieces[boardPosition];

        if (tilePiece.ownerColor == playerColor)
            return true;
        else
            return false;
    }

    internal GameState Move(BoardPosition from, BoardPosition to)
    {
        if (!gamePieces.ContainsKey(from))
        {
            Debug.Log("Couldn't find piece for tile");
            return this;
        }
        GamePiece toMove = this.gamePieces[from];
        this.gamePieces.Remove(from);
        toMove.position = to;
        this.gamePieces[to] = toMove;
        return this;
    }

    public GameState ChangeTurn()
    {
        if (this.playerTurn == PlayerColor.white)
            this.playerTurn = PlayerColor.black;
        else
            this.playerTurn = PlayerColor.white;
        return this;
    }
}


public struct GamePiece
{
    public readonly PlayerColor ownerColor;
    public readonly PieceTypeID typeID;
    public readonly int index;
    public BoardPosition position;

    public GamePiece(PlayerColor ownerColor, PieceTypeID typeID, int index, BoardPosition position)
    {
        this.ownerColor = ownerColor;
        this.typeID = typeID;
        this.index = index;
        this.position = position;
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
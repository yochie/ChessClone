using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{

    private Dictionary<BoardPosition, BoardPiece> pieces;
    private Dictionary<BoardPosition, BoardTile> tiles;

    private void Awake()
    {
        this.tiles = new();
        foreach(BoardTile tile in this.GetComponentsInChildren<BoardTile>())
        {
            this.tiles[tile.GetBoardPosition()] = tile;
        }

        this.pieces = new();
        foreach (BoardPiece piece in this.GetComponentsInChildren<BoardPiece>())
        {
            this.pieces[piece.GetComponentInParent<BoardTile>().GetBoardPosition()] = piece;
        }

    }

    public void HighligthTiles(List<BoardPosition> positions){
        foreach(BoardPosition position in positions)
        {
            this.tiles[position].Highlight(true);
        }
    }
    public void ClearHighligths()
    {
        foreach (BoardTile tile in tiles.Values)
        {
            tile.Highlight(false);
        }
    }

    internal void MovePieceSpriteTo(BoardPosition boardPosition, Vector3 worldPosition)
    {
        if (!this.pieces.ContainsKey(boardPosition))
            return;

        this.pieces[boardPosition].transform.position = worldPosition;
    }
}

public readonly struct BoardPosition
{
    public readonly int xPosition;
    public readonly int yPosition;

    public BoardPosition(int x, int y)
    {
        this.xPosition = x;
        this.yPosition = y;
    }
}
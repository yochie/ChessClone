using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardView : MonoBehaviour
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

    internal void MovePieceSpriteTo(BoardPosition boardPositionStart, Vector3 worldPositionEnd)
    {
        if (!this.pieces.ContainsKey(boardPositionStart))
            return;

        this.pieces[boardPositionStart].transform.position = worldPositionEnd;
    }

    internal void MovePieceSpriteToTile(BoardPosition boardPositionStart, BoardPosition boardPositionEnd)
    {
        if (!this.pieces.ContainsKey(boardPositionStart))
            return;

        if (!this.tiles.ContainsKey(boardPositionEnd))
        {
            Debug.Log("Couldn't find tile for end board position");
            return;
        }
        Vector3 worldPositionEnd = this.tiles[boardPositionEnd].transform.position;
        this.pieces[boardPositionStart].transform.position = worldPositionEnd;
    }

    public Dictionary<BoardPosition, GamePiece> GetBoardViewPieces()
    {
        Dictionary<BoardPosition, GamePiece> gamePieces = new();
        foreach(var (position, piece) in this.pieces)
        {
            gamePieces[position] = new GamePiece(piece.GetOwnerID(), piece.GetPieceTypeID(), piece.GetIndex(), position);
        }
        return gamePieces;
    }

    internal void UpdatePiecePosition(BoardPosition from, BoardPosition to)
    {
        BoardPiece toMove = this.pieces[from];
        this.pieces.Remove(from);
        toMove.transform.parent = this.tiles[to].transform;
        this.pieces[to] = toMove;
    }
}
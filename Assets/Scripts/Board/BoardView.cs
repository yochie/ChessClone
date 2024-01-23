using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages board tile and piece sprites (positions, colors, etc)
public class BoardView : MonoBehaviour
{
    //Stores some state to allow client side visual/ui changes without having to go through server for all operations
    //Server should still validate any operation on the logical game state before it goes through
    private Dictionary<BoardPosition, BoardPiece> pieces;

    private Dictionary<BoardPosition, BoardTile> tiles;

    //Init client side date
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

    #region Client-side state getters

    //used to setup initial game state from scene setup on host
    public Dictionary<BoardPosition, GamePieceID> GetBoardViewState()
    {
        Dictionary<BoardPosition, GamePieceID> gamePieces = new();
        foreach (var (position, piece) in this.pieces)
        {
            gamePieces[position] = new GamePieceID(piece.GetOwnerID(), piece.GetPieceTypeID(), piece.GetIndex());
        }
        return gamePieces;
    }
    #endregion

    #region Client-side state setters
    internal void UpdatePiecePosition(BoardPosition startPosition, BoardPosition endPosition)
    {
        BoardPiece toMove = this.pieces[startPosition];
        this.pieces.Remove(startPosition);
        toMove.transform.parent = this.tiles[endPosition].transform;
        this.pieces[endPosition] = toMove;
    }

    internal void RemovePieceAtPosition(BoardPosition positionToRemove)
    {
        this.pieces.Remove(positionToRemove);
    }
    #endregion

    #region Visual modifications
    public void HighligthTiles(List<BoardPosition> positions, Color color){
        foreach(BoardPosition position in positions)
        {
            this.tiles[position].Highlight(color);
        }
    }
    public void ClearHighligths()
    {
        foreach (BoardTile tile in tiles.Values)
        {
            tile.UnHighlight();
        }
    }

    internal void MovePieceSpriteToWorldPosition(BoardPosition spriteStoredAtPosition, Vector3 destinationWorldPosition)
    {
        if (!this.pieces.ContainsKey(spriteStoredAtPosition))
            return;

        this.pieces[spriteStoredAtPosition].transform.position = destinationWorldPosition;
    }

    internal void MovePieceSpriteToBoardPosition(BoardPosition spriteStoredAtPosition, BoardPosition destinationBoardPosition)
    {
        if (!this.pieces.ContainsKey(spriteStoredAtPosition))
            return;

        if (!this.tiles.ContainsKey(destinationBoardPosition))
        {
            Debug.Log("Couldn't find tile for end board position");
            return;
        }
        Vector3 worldPositionEnd = this.tiles[destinationBoardPosition].transform.position;
        this.pieces[spriteStoredAtPosition].transform.position = worldPositionEnd;
    }

    internal void DestroyPieceSprite(BoardPosition eatPosition)
    {
        if(!this.pieces.ContainsKey(eatPosition))
            return;

        Destroy(this.pieces[eatPosition].gameObject);

    }
    #endregion
}
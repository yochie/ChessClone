using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInputHandler : MonoBehaviour
{
    [SerializeField]
    private BoardView boardView;

    private bool draggingBoardPiece = false;
    private BoardTile hoveredTile;

    public void OnTileBeginDrag(BoardTile tile)
    {
        this.boardView.HighligthTiles(new List<BoardPosition>() { tile.GetBoardPosition() });
        //TODO : replace with check on gamestate of board to see if tile contains piece and its your turn
        if (GameController.Singleton.TileHoldsPiece(tile.GetBoardPosition()) &&
            GameController.Singleton.ItsMyTurn() &&
            GameController.Singleton.IOwnPieceAtTile(tile.GetBoardPosition())
            )
        {
            this.draggingBoardPiece = true;            
        }
        else
        {
            Debug.Log("Drag blocked");
            Debug.Log(GameController.Singleton.TileHoldsPiece(tile.GetBoardPosition()));
            Debug.Log(GameController.Singleton.ItsMyTurn());
            Debug.Log(GameController.Singleton.IOwnPieceAtTile(tile.GetBoardPosition()));
        }
    }

    public void OnTileDrag(BoardTile tile, Vector3 pointerWorldPosition)
    {
        if(this.draggingBoardPiece)
            this.boardView.MovePieceSpriteTo(tile.GetBoardPosition(), pointerWorldPosition);
    }

    public void OnTileEndDrag(BoardTile startTile)
    {
        this.boardView.ClearHighligths();
        if (this.draggingBoardPiece)
        {
            BoardPosition startPosition = startTile.GetBoardPosition();
            BoardPosition endPosition = this.hoveredTile.GetBoardPosition();
            if (GameController.IsValidMove(startPosition, endPosition))
                GameController.Singleton.CmdTryMove(startPosition, endPosition);
            this.draggingBoardPiece = false;
        }
    }

    internal void SetHovered(BoardTile boardTile)
    {
        this.hoveredTile = boardTile;
    }
}

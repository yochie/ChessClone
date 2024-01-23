using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BoardInputHandler : NetworkBehaviour
{
    [SerializeField]
    private BoardView boardView;

    [SerializeField]
    private GameState gameState;

    public bool InputAllowed { get; set; }

    public BoardTile HoveredTile { get; set; }

    private bool draggingBoardPiece = false;
    

    public void Awake()
    {
        this.InputAllowed = false;
    }


    public void OnTileBeginDrag(BoardTile tile)
    {        
        if (this.InputAllowed && 
            this.gameState.PlayerTurn == GameController.Singleton.LocalPlayer.PlayerColor &&
            this.gameState.PositionHoldsAPiece(tile.GetBoardPosition()) &&            
            this.gameState.IsOwnerofPieceAtPosition(tile.GetBoardPosition(), GameController.Singleton.LocalPlayer.PlayerColor))
        {
            //TODO: Get list of possible moves for piece at tile
            this.boardView.HighligthTiles(new List<BoardPosition>() { tile.GetBoardPosition() });
            this.draggingBoardPiece = true;
        }
        else
        {
            Debug.Log("Dragging not allowed");
        }
    }

    public void OnTileDrag(BoardTile tile, Vector3 pointerWorldPosition)
    {
        if(this.draggingBoardPiece)
            this.boardView.MovePieceSpriteToWorldPosition(tile.GetBoardPosition(), pointerWorldPosition);
    }

    public void OnTileEndDrag(BoardTile startTile)
    {
        this.boardView.ClearHighligths();
        if (this.draggingBoardPiece)
        {
            BoardPosition startPosition = startTile.GetBoardPosition();
            BoardPosition endPosition = this.HoveredTile.GetBoardPosition();
            if (GameController.Singleton.IsValidMove(this.gameState, startPosition, endPosition))
                GameController.Singleton.CmdTryMove(startPosition, endPosition);
            else
            {
                this.boardView.MovePieceSpriteToBoardPosition(startTile.GetBoardPosition(), startTile.GetBoardPosition());
            }
            this.draggingBoardPiece = false;
        }
    }

    [ClientRpc]
    public void RpcSetInputAllowed()
    {
        this.InputAllowed = true;
    }

}

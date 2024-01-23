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
            this.gameState.IsOwnerOfPieceAtPosition(tile.GetBoardPosition(), GameController.Singleton.LocalPlayer.PlayerColor))
        {
            //TODO: Get list of possible moves for piece at tile + create ghost version of piece
            this.boardView.HighligthTiles(new List<BoardPosition>() { tile.GetBoardPosition() });
            this.draggingBoardPiece = true;
        }
        else
        {
            Debug.Log("Dragging not allowed");
            //Debug.Log(this.InputAllowed);
            //Debug.Log(this.gameState.PlayerTurn == GameController.Singleton.LocalPlayer.PlayerColor);
            //Debug.Log(this.gameState.PositionHoldsAPiece(tile.GetBoardPosition()));
            //Debug.Log(this.gameState.IsOwnerOfPieceAtPosition(tile.GetBoardPosition(), GameController.Singleton.LocalPlayer.PlayerColor));
        }
    }

    public void OnTileDrag(BoardTile tile, Vector3 pointerWorldPosition)
    {
        //makes piece sprite follow mouse cursor
        if(this.draggingBoardPiece)
            this.boardView.MovePieceSpriteToWorldPosition(tile.GetBoardPosition(), pointerWorldPosition);
    }

    public void OnTileEndDrag(BoardTile startTile)
    {        
        if (this.draggingBoardPiece)
        {
            this.boardView.ClearHighligths();
            BoardPosition startPosition = startTile.GetBoardPosition();
            if (this.HoveredTile == null)
            {
                this.AbortDrag(startPosition);
                return;
            }
            BoardPosition endPosition = this.HoveredTile.GetBoardPosition();
            if (this.gameState.IsValidMove(startPosition, endPosition))
            {                
                GameController.Singleton.CmdTryMove(startPosition, endPosition);
                this.draggingBoardPiece = false;
                return;
            }
            else
            {
                this.AbortDrag(startPosition);
                return;
            }
        }
    }

    private void AbortDrag(BoardPosition returnTo)
    {
        this.boardView.MovePieceSpriteToBoardPosition(returnTo, returnTo);
        this.draggingBoardPiece = false;
    }

    [ClientRpc]
    public void RpcSetInputAllowed()
    {
        Debug.Log("Input allowed");
        this.InputAllowed = true;
    }

}

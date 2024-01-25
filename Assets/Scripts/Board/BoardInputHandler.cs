using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class BoardInputHandler : NetworkBehaviour
{
    [SerializeField]
    private BoardView boardView;

    [SerializeField]
    private SyncedGameState syncedGameState;

    public bool InputAllowed { get; set; }

    public BoardTile HoveredTile { get; set; }

    private bool draggingBoardPiece = false;
    

    public void Awake()
    {
        this.InputAllowed = false;
    }

    public void OnTileBeginDrag(BoardTile tile)
    {
        BoardPosition tilePosition = tile.GetBoardPosition();
        if (this.InputAllowed && 
            this.syncedGameState.PlayerTurn == GameController.Singleton.LocalPlayer.PlayerColor &&
            this.syncedGameState.PositionHoldsAPiece(tilePosition) &&            
            this.syncedGameState.IsOwnerOfPieceAtPosition(tilePosition, GameController.Singleton.LocalPlayer.PlayerColor))
        {
            //highlight start position
            this.boardView.HighligthTiles(new List<BoardPosition>() { tilePosition }, Color.blue);
            //highlight possible moves           
            List<Move> possibleMoves = this.syncedGameState.GetPossibleMovesFrom(tilePosition);
            this.boardView.HighligthTiles(possibleMoves.Select(move => move.to).ToList(), Color.green);

            this.draggingBoardPiece = true;
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
        if (!this.draggingBoardPiece)
            return;

        this.boardView.ClearHighligths();
        BoardPosition startPosition = startTile.GetBoardPosition();

        if (this.HoveredTile == null)
        {
            this.AbortDrag(startPosition);
            return;
        }
        BoardPosition endPosition = this.HoveredTile.GetBoardPosition();

        List<Move> possibleMoves = this.syncedGameState.GetPossibleMovesFrom(startPosition);
        List<Move> movesToDestination = possibleMoves.Where(move => move.to.Equals(endPosition)).ToList();
        if(movesToDestination.Count == 0)
        {
            this.AbortDrag(startPosition);
            return;
        } else if (movesToDestination.Count > 1)
        {
            Debug.Log("More than one move leads to dest, defaulting to first");
        }
        Move move = movesToDestination[0];
        GameController.Singleton.CmdTryMove(move);
        this.draggingBoardPiece = false;
        return;        
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

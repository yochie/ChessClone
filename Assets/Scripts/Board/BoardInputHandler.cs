using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInputHandler : MonoBehaviour
{
    [SerializeField]
    private Board board;

    private bool draggingBoardPiece = false;

    public void OnTileBeginDrag(BoardTile tile)
    {
        this.board.HighligthTiles(new List<BoardPosition>() { tile.GetBoardPosition() });
        //TODO : replace with check on gamestate of board to see if tile contains piece
        if (true)
        {
            draggingBoardPiece = true;            
        }
    }

    public void OnTileDrag(BoardTile tile, Vector3 pointerWorldPosition)
    {
        if(draggingBoardPiece)
            this.board.MovePieceSpriteTo(tile.GetBoardPosition(), pointerWorldPosition);
    }

    public void OnTileEndDrag(BoardTile tile)
    {
        this.board.ClearHighligths();
        if (draggingBoardPiece)
            this.board.MovePieceSpriteTo(tile.GetBoardPosition(), tile.transform.position);

    }
}

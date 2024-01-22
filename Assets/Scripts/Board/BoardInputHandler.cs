using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInputHandler : MonoBehaviour
{
    [SerializeField]
    private Board board;

    public void OnTileBeginDrag(BoardTile tile)
    {
        this.board.HighligthTiles(new List<BoardPosition>() { tile.GetPosition() });
    }

    public void OnTileEndDrag(BoardTile tile)
    {
        this.board.ClearHighligths();
    }
}

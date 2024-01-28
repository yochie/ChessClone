using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages board tile and piece sprites (positions, colors, etc)
public class BoardView : MonoBehaviour
{
    [SerializeField]
    private PieceTypeData pieceTypeData;

    //Stores some state to allow client side visual/ui changes without having to go through server for all operations
    //Server should still validate any operation on the logical game state before it goes through
    private Dictionary<BoardPosition, BoardPiece> pieces;

    private Dictionary<BoardPosition, BoardTile> tiles;

    //Init client side state from scene setup
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

    //used to setup initial server game state from scene setup on host
    public Dictionary<BoardPosition, GamePieceID> GetBoardViewState()
    {
        Dictionary<BoardPosition, GamePieceID> gamePieces = new();
        foreach (var (position, piece) in this.pieces)
        {
            gamePieces[position] = new GamePieceID(piece.GetOwnerColor(), piece.GetPieceTypeID(), piece.GetIndex());
        }
        return gamePieces;
    }

    private BoardPiece GetKing(PlayerColor color)
    {
        foreach (BoardPiece boardPiece in this.pieces.Values)
        {
            if (boardPiece.GetPieceTypeID() == PieceTypeID.king && boardPiece.GetOwnerColor() == color)
            {
                return boardPiece;
            }
        }

        Debug.Log("Couldn't find king board piece");
        return null;
    }
    #endregion

    #region Client-side state setters
    public void PostMoveUpdates(Move move, List<PlayerColor> checkedPlayers, GamePieceID promotedPieceID)
    {
        Debug.LogFormat("Updating for move {0} with {1} checked players", move, checkedPlayers.Count);
        if (move.eats)
        {
            this.DestroyPieceSprite(move.eatPosition);
            this.RemovePieceAtPosition(move.eatPosition);
        }
        if (move.requiresPromotion && promotedPieceID.typeID != PieceTypeID.none)
        {
            this.ReplacePiece(move.from, this.pieces[move.from], promotedPieceID);
        }

        this.MovePieceSpriteToBoardPosition(move.from, move.to);
        this.UpdatePiecePosition(move.from, move.to);
        if (move.includesSecondaryMove)
        {
            this.MovePieceSpriteToBoardPosition(move.from2, move.to2);
            this.UpdatePiecePosition(move.from2, move.to2);
        }

        this.ClearChecked();
        foreach(PlayerColor checkedPlayer in checkedPlayers)
        {
            this.SetChecked(checkedPlayer);
        }
    }

    private void ReplacePiece(BoardPosition replaceAtPosition, BoardPiece previousBoardPiece, GamePieceID newPieceID)
    {
        this.pieces.Remove(replaceAtPosition);
        Destroy(previousBoardPiece.gameObject);
        BoardPiece boardPiece = Instantiate(this.pieceTypeData.GetBoardPiecePrefab(newPieceID.typeID, newPieceID.color), this.tiles[replaceAtPosition].transform);
        boardPiece.SetIndex(newPieceID.index);
        this.pieces.Add(replaceAtPosition, boardPiece);
    }

    private void UpdatePiecePosition(BoardPosition startPosition, BoardPosition endPosition)
    {
        BoardPiece toMove = this.pieces[startPosition];
        this.pieces.Remove(startPosition);
        toMove.transform.parent = this.tiles[endPosition].transform;
        this.pieces[endPosition] = toMove;
    }

    private void RemovePieceAtPosition(BoardPosition positionToRemove)
    {
        this.pieces.Remove(positionToRemove);
    }
    #endregion

    #region Visual updates

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

    //for following mouse while dragging
    internal void MovePieceSpriteToWorldPosition(BoardPosition spriteStoredAtPosition, Vector3 destinationWorldPosition)
    {
        if (!this.pieces.ContainsKey(spriteStoredAtPosition))
            return;

        this.pieces[spriteStoredAtPosition].transform.position = destinationWorldPosition;
    }

    //for snapping to positions on board
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

    private void DestroyPieceSprite(BoardPosition eatPosition)
    {
        if(!this.pieces.ContainsKey(eatPosition))
            return;

        Destroy(this.pieces[eatPosition].gameObject);

    }

    private void SetChecked(PlayerColor checkedPlayer)
    {
        BoardPiece kingPiece = this.GetKing(checkedPlayer);
        kingPiece.SetChecked(isChecked: true);
    }

    //removes all checked indicators from board state
    private void ClearChecked()
    {
        List<PlayerColor> colors = new() { PlayerColor.white, PlayerColor.black };
        foreach(PlayerColor color in colors)
        {
            BoardPiece kingPiece = this.GetKing(color);
            kingPiece.SetChecked(isChecked: false);
        }
    }

    //rotates each tile on board (camera is rotated seperately) 
    //for displaying player's pieces at bottom of screen
    internal void Rotate()
    {
        foreach (BoardTile tile in this.tiles.Values)
        {
            tile.transform.Rotate(new Vector3(0, 0, 180));
        }
    }
    #endregion
}
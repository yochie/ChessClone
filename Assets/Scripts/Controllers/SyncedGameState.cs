using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using System;

//Raw data that defines game logical state
public class SyncedGameState : NetworkBehaviour
{
    [SerializeField]
    private PieceTypeData pieceTypes;

    #region Synced
    private readonly SyncDictionary<BoardPosition, GamePieceID> gamePieces = new();

    private readonly SyncDictionary<GamePieceID, bool> pawnHasMoved = new();

    [SyncVar]
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    private readonly SyncDictionary<BoardPosition, List<Move>> possibleMoves = new();

    private readonly SyncDictionary<PlayerColor, bool> playerChecked = new();
    #endregion

    #region State modifiers
    [Server]
    public void Init(Dictionary<BoardPosition, GamePieceID> gamePieces, PlayerColor playerTurn)
    {
        foreach (var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
            if (gamePieceID.typeID == PieceTypeID.pawn)
                this.pawnHasMoved.Add(gamePieceID, false);
        }
        this.playerTurn = playerTurn;
        this.playerChecked.Add(PlayerColor.white, false);
        this.playerChecked.Add(PlayerColor.black, false);
        this.UpdatePossibleMoves();
    }

    [Server]
    public void MovePiece(Move move)
    {
        if (!gamePieces.ContainsKey(move.from))
        {
            Debug.Log("Couldn't find piece to move");
            return;
        }

        if (move.eats)
        {
            BoardPosition toEat = move.eatPosition;
            this.DeletePieceAt(toEat);
        }

        GamePieceID toMove = this.gamePieces[move.from];
        this.gamePieces.Remove(move.from);
        this.gamePieces[move.to] = toMove;

        if (toMove.typeID == PieceTypeID.pawn)
            this.pawnHasMoved[toMove] = true;

        //just for computing checked state, will be changed again to set possible moves for new turn
        this.UpdatePossibleMoves();

        this.UpdateCheckedState();

        this.ChangeTurn();

        this.UpdatePossibleMoves();

    }

    [Server]
    private void DeletePieceAt(BoardPosition position)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece to remove");
        }
        this.gamePieces.Remove(position);
    }

    [Server]
    private void ChangeTurn()
    {
        if (this.playerTurn == PlayerColor.white)
            this.playerTurn = PlayerColor.black;
        else
            this.playerTurn = PlayerColor.white;
    }

    [Server]
    private void UpdatePossibleMoves()
    {
        this.possibleMoves.Clear();
        Dictionary<BoardPosition, List<Move>> toMakePossible = new();
        foreach (var (position, gamePieceID) in this.gamePieces)
        {
            if (gamePieceID.color != this.playerTurn)
                continue;
            IPieceType pieceType = this.pieceTypes.GetPieceTypeForByID(gamePieceID.typeID);
            List<Move> possibleMovesFromPosition = pieceType.GetPossibleMovesFrom(this, position);

            toMakePossible.Add(position, possibleMovesFromPosition);
        }

        //TODO: filter out moves that would put you in check + those that leave you checked

        //Remove moves that leave checked
        if (this.playerChecked[this.playerTurn])
        {
            foreach(var pair in toMakePossible.ToList())
            {

            }
        }

        //Remove moves that would put you in check


        //Copy result to possible moves
        foreach(var pair in toMakePossible)
        {
            this.possibleMoves[pair.Key] = pair.Value;
        }
    }

    [Server]
    private void UpdateCheckedState() {
        List<PlayerColor> toCheck = new();
        foreach(List<Move> moveList in this.possibleMoves.Values)
        {
            foreach (Move move in moveList)
            {
                if (!move.eats)
                    continue;

                if (!this.PositionHoldsAPiece(move.eatPosition))
                {
                    Debug.Log("ERROR: Possible move eats peace at position where nothing is stored...");
                    continue;
                }

                GamePieceID eatenPiece = this.GetPieceAtPosition(move.eatPosition);
                if (eatenPiece.typeID == PieceTypeID.king)
                {
                    toCheck.Add(eatenPiece.color);
                }
            }
        }

        foreach(PlayerColor color in this.playerChecked.Keys.ToList())
        {
            this.playerChecked[color] = toCheck.Contains(color);
        }
        
        //TODO : remove any move that doesnt clear cheked from possible moves
    }

    internal List<PlayerColor> GetCheckedPlayers()
    {
        return this.playerChecked.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }
    #endregion

    #region Utility/Getters
    public bool PositionHoldsAPiece(BoardPosition position)
    {
        return this.gamePieces.ContainsKey(position);
    }

    public bool IsOwnerOfPieceAtPosition(BoardPosition position, PlayerColor playerColor)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece for tile");
            return false;
        }
        GamePieceID tilePiece = this.gamePieces[position];

        if (tilePiece.color == playerColor)
            return true;
        else
            return false;
    }

    internal bool IsValidMove(Move move)
    {
        if (move.from.Equals(move.to))
        {
            Debug.Log("Invalid move : start == end");
            return false;
        }

        if(!possibleMoves.ContainsKey(move.from))
        {
            Debug.Log("Invalid move : start position is not set as possible move");
            return false;
        }

        if (!possibleMoves[move.from].Contains(move))
        {
            Debug.Log("Invalid move : move is not set as possible move from start");
            return false;
        }
        return true;
    }

    internal GamePieceID GetPieceAtPosition(BoardPosition fromPosition)
    {
        if (!this.gamePieces.ContainsKey(fromPosition))
        {
            Debug.Log("No piece at requested position. Make sure you validate before sending request.");
            return new GamePieceID(PlayerColor.white, PieceTypeID.none, -1);
        }
        return gamePieces[fromPosition];
    }

    internal List<Move> GetPossibleMovesFrom(BoardPosition boardPosition)
    {

        if (!this.possibleMoves.ContainsKey(boardPosition))
        {
            Debug.Log("No possible moves set for given position.");
            return new List<Move>();
        }
        return this.possibleMoves[boardPosition];
    }

    public bool HasPawnMoved(GamePieceID pawnID)
    {
        if(!this.pawnHasMoved.ContainsKey(pawnID))
        {
            Debug.LogFormat("Couldn't determine if pawn {0} has moved, it wasn't in dictionary", pawnID);
            return false;
        }
        return this.pawnHasMoved[pawnID];
    }

    public bool IsPlayerChecked(PlayerColor playerColor)
    {
        return this.playerChecked[playerColor];
    }
    #endregion
}
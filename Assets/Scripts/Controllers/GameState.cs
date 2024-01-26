using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class GameState : IGamePieceState
{
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    private Dictionary<BoardPosition, GamePieceID> gamePieces;

    private Dictionary<BoardPosition, List<Move>> possibleMoves;

    private Dictionary<GamePieceID, bool> pieceHasMoved;

    private Dictionary<PlayerColor, bool> playerCheckStates;

    private Dictionary<PlayerColor, bool> playerCheckMateStates;
    private bool draw;

    private PieceTypeData pieceTypeData;

    //Fresh copy constructor, used for initial setup
    public GameState(PlayerColor playerTurn, Dictionary<BoardPosition, GamePieceID> gamePieces, PieceTypeData pieceTypeData)
    {
        this.playerTurn = playerTurn;

        this.gamePieces = new();
        this.pieceHasMoved = new();
        foreach (var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
            this.pieceHasMoved.Add(gamePieceID, false);
        }
        this.playerCheckStates = new();
        this.playerCheckStates.Add(PlayerColor.white, false);
        this.playerCheckStates.Add(PlayerColor.black, false);
        this.playerCheckMateStates = new();
        this.playerCheckMateStates.Add(PlayerColor.white, false);
        this.playerCheckMateStates.Add(PlayerColor.black, false);
        this.draw = false;
        this.possibleMoves = new();
        this.pieceTypeData = pieceTypeData;
        this.UpdatePossibleMoves();
    }

    //Fully specified constructor, used for cloning
    public GameState(PlayerColor playerTurn,
                     Dictionary<BoardPosition, GamePieceID> gamePieces,
                     Dictionary<BoardPosition, List<Move>> possibleMoves,
                     Dictionary<GamePieceID, bool> pawnHasMoved,                                          
                     Dictionary<PlayerColor, bool> playerChecked,
                     Dictionary<PlayerColor, bool> playerCheckMated,
                     bool draw,
                     PieceTypeData pieceTypeData)
    {
        this.playerTurn = playerTurn;
        this.gamePieces = gamePieces;
        this.possibleMoves = possibleMoves;
        this.pieceHasMoved = pawnHasMoved;
        this.playerCheckStates = playerChecked;
        this.playerCheckMateStates = playerCheckMated;
        this.draw = draw;
        this.pieceTypeData = pieceTypeData;
    }

    #region Clone

    private GameState Clone()
    {
        //since these dictionaries only store value types, fine to clone using constructor
        //dont have their own function since they are kept private
        Dictionary<GamePieceID, bool> pieceHasMovedClone = new(this.pieceHasMoved);
        Dictionary<PlayerColor, bool> playerCheckedClone = new(this.playerCheckStates);
        Dictionary<PlayerColor, bool> playerCheckMatedClone = new(this.playerCheckMateStates);

        return new GameState(this.playerTurn, this.GetGamePiecesClone(), this.GetPossibleMovesClone(), pieceHasMovedClone, playerCheckedClone, playerCheckMatedClone, this.draw, this.pieceTypeData);
    }

    public Dictionary<BoardPosition, GamePieceID> GetGamePiecesClone() 
    {
        //GamePieceID is value type (struct), so this is enough for deep copy
        return new Dictionary<BoardPosition, GamePieceID> (this.gamePieces); 
    }

    public Dictionary<BoardPosition, List<Move>> GetPossibleMovesClone()
    {
        Dictionary<BoardPosition, List<Move>> clone = new();
        foreach (var (position, moveList) in this.possibleMoves)
        {
            //create new list to avoid passing reference
            //Move is value type (struct) so thats enough for deep copy
            clone.Add(position, new List<Move>(moveList));
        }
        return clone;
    }
    #endregion

    #region State modifiers
    [Server]
    public void DoMove(Move move)
    {
        this.ApplyMoveToBoardState(move);

        this.UpdateCheckState();

        this.SwapTurn();

        this.UpdatePossibleMoves();

        this.UpdateGameEndStates();
    }

    private void UpdateGameEndStates()
    {
        bool noPossibleMoves = !this.AnyPossibleMove();
        if (noPossibleMoves && this.GetCheckedPlayers().Contains(this.playerTurn))
        {
            this.playerCheckMateStates[this.playerTurn] = true;
        } else if (noPossibleMoves)
        {
            this.draw = true;
        }
    }

    [Server]
    private void ApplyMoveToBoardState(Move move)
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
        this.pieceHasMoved[toMove] = true;

        //SECONDARY MOVE (castling)
        if (!move.includesSecondaryMove)
            return;

        if (!gamePieces.ContainsKey(move.from2))
        {
            Debug.Log("Couldn't find piece to do secondary move");
            return;
        }

        GamePieceID secondaryToMove = this.gamePieces[move.from2];
        this.gamePieces.Remove(move.from2);
        this.gamePieces[move.to2] = secondaryToMove;
        this.pieceHasMoved[secondaryToMove] = true;
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
    private void SwapTurn()
    {
        if (this.playerTurn == PlayerColor.white)
            this.playerTurn = PlayerColor.black;
        else
            this.playerTurn = PlayerColor.white;
    }

    //Should only be used when simulating game state (ie for checking if move will cause self-check, etc)
    //DoMove() should already take care of swapping turns during natural execution
    [Server]
    private void SetPlayerTurn(PlayerColor playerTurn)
    {
        this.playerTurn = playerTurn;
    }

    [Server]
    private void UpdatePossibleMoves(bool allowSelfChecking = false, bool threateningMovesOnly = false)
    {
        this.possibleMoves.Clear();
        Dictionary<BoardPosition, List<Move>> potentialMoves = new();
        foreach (var (position, gamePieceID) in this.gamePieces)
        {
            if (gamePieceID.color != this.playerTurn)
                continue;
            IPieceType pieceType = pieceTypeData.GetPieceTypeByID(gamePieceID.typeID);
            List<Move> possibleMovesFromPosition = pieceType.GetPossibleMovesFrom(this, position, threateningMovesOnly);

            potentialMoves.Add(position, possibleMovesFromPosition);
        }

        //Remove any move that would result in check state for current player
        //only exception is for when simulating states to determine whether a move would cause self check
        //in that case, consider self checking moves legal by opponent
        if (!allowSelfChecking)
        {
            foreach (var (position, moveList) in potentialMoves.ToList())
            {
                foreach (Move move in moveList.ToList())
                {
                    GameState clonedState = this.Clone();
                    clonedState.ApplyMoveToBoardState(move);
                    bool selfChecked = clonedState.KingThreatenedAtGameState(Utility.GetOpponentColor(this.playerTurn));
                    if (selfChecked)
                    {
                        moveList.Remove(move);
                    }
                }
            }
        }

        //Copy result to possible moves
        foreach (var pair in potentialMoves)
        {
            this.possibleMoves[pair.Key] = pair.Value;
        }
    }

    [Server]
    private void UpdateCheckState()
    {
        bool opponentChecked = this.KingThreatenedAtGameState(this.playerTurn);
        this.playerCheckStates[Utility.GetOpponentColor(this.playerTurn)] = opponentChecked;

        //this should be false during normal operation since self checking moves are illegal
        bool selfChecked = this.KingThreatenedAtGameState(Utility.GetOpponentColor(this.playerTurn));
        this.playerCheckStates[this.playerTurn] = selfChecked;

    }

    #endregion

    #region Utility
    //Makes clone, calculates its possible moves assuming given player is playing
    //return true if any possible move would eat opponents king
    [Server]
    private bool KingThreatenedAtGameState(PlayerColor threatPlayer)
    {
        //Calculate possible moves on clone to avoid updating actual possible moves
        GameState clonedState = this.Clone();
        clonedState.SetPlayerTurn(threatPlayer);
        //since players are still checked event if the threatening move would cause self check, we allow self checking when considering possible moves
        clonedState.UpdatePossibleMoves(allowSelfChecking: true, threateningMovesOnly: true);

        List<PlayerColor> checkedPlayers = new();
        foreach (List<Move> moveList in clonedState.possibleMoves.Values.ToList())
        {
            foreach (Move move in moveList)
            {
                if (!move.eats)
                    continue;

                if (!clonedState.PositionHoldsAPiece(move.eatPosition))
                {
                    Debug.Log("ERROR: Possible move eats peace at position where nothing is stored...");
                    continue;
                }

                GamePieceID eatenPiece = clonedState.GetPieceAtPosition(move.eatPosition);
                if (eatenPiece.typeID == PieceTypeID.king && eatenPiece.color != clonedState.playerTurn)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Used for testing castling
    [Server]
    public bool KingThreatenedAtPosition(PlayerColor threatPlayer, BoardPosition originalPosition, BoardPosition newPosition)
    {
        //Calculate possible moves on clone to avoid updating actual possible moves
        GameState clonedState = this.Clone();

        //move king to new position
        clonedState.ApplyMoveToBoardState(new Move(from: originalPosition, to: newPosition, eats: false));

        return clonedState.KingThreatenedAtGameState(threatPlayer);
    }

    internal List<PlayerColor> GetCheckedPlayers()
    {
        return this.playerCheckStates.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }

    internal List<PlayerColor> GetCheckMatedPlayers()
    {
        return this.playerCheckMateStates.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }

    public bool GetDraw()
    {
        return this.draw;
    }

    internal bool IsPossibleMove(Move move)
    {
        if (move.from.Equals(move.to))
        {
            Debug.Log("Invalid move : start == end");
            return false;
        }

        if (!this.possibleMoves.ContainsKey(move.from))
        {
            Debug.Log("Invalid move : start position is not set as possible move");
            return false;
        }

        if (!this.possibleMoves[move.from].Contains(move))
        {
            Debug.Log("Invalid move : move is not set as possible move from start");
            return false;
        }

        return true;
    }

    public bool HasPieceMoved(GamePieceID pieceID)
    {
        if (!this.pieceHasMoved.ContainsKey(pieceID))
        {
            Debug.LogFormat("Couldn't determine if piece {0} has moved, it wasn't in dictionary", pieceID);
            return false;
        }
        return this.pieceHasMoved[pieceID];
    }
    #endregion

    #region IGamePieceState
    public bool PositionHoldsAPiece(BoardPosition position)
    {
        return this.gamePieces.ContainsKey(position);
    }

    public GamePieceID GetPieceAtPosition(BoardPosition fromPosition)
    {
        if (!this.gamePieces.ContainsKey(fromPosition))
        {
            Debug.Log("No piece at requested position. Make sure you validate before sending request.");
            return new GamePieceID(PlayerColor.white, PieceTypeID.none, -1);
        }
        return gamePieces[fromPosition];
    }

    public bool IsOwnerOfPieceAtPosition(BoardPosition position, PlayerColor playerColor)
    {
        if (!this.gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece at given position to determine owner");
            return false;
        }
        return this.gamePieces[position].color == playerColor;
    }

    public List<Move> GetPossibleMovesFrom(BoardPosition boardPosition)
    {
        if (!this.possibleMoves.ContainsKey(boardPosition))
        {
            Debug.Log("No possible moves set for given position.");
            return new List<Move>();
        }
        return this.possibleMoves[boardPosition];
    }

    public bool AnyPossibleMove()
    {
        foreach(List<Move> moveList in this.possibleMoves.Values)
        {
            if (moveList.Count > 0)
                return true;
        }
        return false;
    }
    #endregion
}

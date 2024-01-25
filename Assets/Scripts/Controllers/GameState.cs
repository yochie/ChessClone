using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class GameState : IGamePieceState
{
    private PlayerColor playerTurn;
    public PlayerColor PlayerTurn { get => this.playerTurn; }

    private Dictionary<BoardPosition, GamePieceID> gamePieces;

    private Dictionary<BoardPosition, List<Move>> possibleMoves;

    private Dictionary<GamePieceID, bool> pawnHasMoved;

    private Dictionary<PlayerColor, bool> playerCheckStates;

    //Fresh copy constructor, used for initial setup
    public GameState(PlayerColor playerTurn, Dictionary<BoardPosition, GamePieceID> gamePieces, PieceTypeData pieceTypeData)
    {
        this.playerTurn = playerTurn;

        this.gamePieces = new();
        this.pawnHasMoved = new();
        foreach (var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
            if (gamePieceID.typeID == PieceTypeID.pawn)
                this.pawnHasMoved.Add(gamePieceID, false);
        }
        this.playerCheckStates = new();
        this.playerCheckStates.Add(PlayerColor.white, false);
        this.playerCheckStates.Add(PlayerColor.black, false);
        this.possibleMoves = new();
        this.UpdatePossibleMoves(pieceTypeData);
    }

    //Fully specified constructor, used for cloning
    public GameState(PlayerColor playerTurn,
                     Dictionary<BoardPosition, GamePieceID> gamePieces,
                     Dictionary<BoardPosition, List<Move>> possibleMoves,
                     Dictionary<GamePieceID, bool> pawnHasMoved,                                          
                     Dictionary<PlayerColor, bool> playerChecked)
    {
        this.playerTurn = playerTurn;
        this.gamePieces = gamePieces;
        this.possibleMoves = possibleMoves;
        this.pawnHasMoved = pawnHasMoved;
        this.playerCheckStates = playerChecked;
    }

    #region Clone

    private GameState Clone()
    {
        //since these dictionaries only store value types, fine to clone using constructor
        //dont have their own function since they are kept private
        Dictionary<GamePieceID, bool> pawnHasMovedClone = new(this.pawnHasMoved);
        Dictionary<PlayerColor, bool> playerCheckedClone = new(this.playerCheckStates);
        return new GameState(this.playerTurn, this.GetGamePiecesClone(), this.GetPossibleMovesClone(), pawnHasMovedClone, playerCheckedClone);
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
    public void DoMove(Move move, PieceTypeData pieceTypeData)
    {
        this.ApplyMoveToBoardState(move);

        this.UpdateCheckState(pieceTypeData);

        this.SwapTurn();

        this.UpdatePossibleMoves(pieceTypeData);
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

        if (toMove.typeID == PieceTypeID.pawn)
            this.pawnHasMoved[toMove] = true;
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
    private void UpdatePossibleMoves(PieceTypeData pieceTypeData)
    {
        this.possibleMoves.Clear();
        Dictionary<BoardPosition, List<Move>> potentialMoves = new();
        foreach (var (position, gamePieceID) in this.gamePieces)
        {
            if (gamePieceID.color != this.playerTurn)
                continue;
            IPieceType pieceType = pieceTypeData.GetPieceTypeByID(gamePieceID.typeID);
            List<Move> possibleMovesFromPosition = pieceType.GetPossibleMovesFrom(this, position);

            potentialMoves.Add(position, possibleMovesFromPosition);
        }

        //Remove any move that would result in check state for current player
        if (this.playerCheckStates[this.playerTurn])
        {
            foreach (var (position, moveList) in potentialMoves.ToList())
            {
                foreach (Move move in moveList.ToList())
                {
                    GameState clonedState = this.Clone();
                    clonedState.ApplyMoveToBoardState(move);
                    bool selfChecked = GameState.KingThreatenedAtGameState(pieceTypeData, Utility.GetOpponentColor(this.playerTurn), clonedState);
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
    private void UpdateCheckState(PieceTypeData pieceTypeData)
    {
        bool opponentChecked = GameState.KingThreatenedAtGameState(pieceTypeData, this.playerTurn, this);
        this.playerCheckStates[Utility.GetOpponentColor(this.playerTurn)] = opponentChecked;

        //this should be false during normal operation since self checking moves are illegal
        bool selfChecked = GameState.KingThreatenedAtGameState(pieceTypeData, Utility.GetOpponentColor(this.playerTurn), this);
        this.playerCheckStates[this.playerTurn] = selfChecked;

        //since any self checking move is invalid, we can safely clear our checked flag
        //this.playerCheckStates[this.playerTurn] = false;

    }

    //Makes clone, calculates its possible moves assuming given player is playing
    //return true if any possible move would eat opponents king
    [Server]
    private static bool KingThreatenedAtGameState(PieceTypeData pieceTypeData, PlayerColor threatPlayer, GameState state)
    {
        //Calculate possible moves on clone to avoid updating actual possible moves
        GameState clonedState = state.Clone();
        clonedState.SetPlayerTurn(threatPlayer);
        clonedState.UpdatePossibleMoves(pieceTypeData);

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

    internal List<PlayerColor> GetCheckedPlayers()
    {
        return this.playerCheckStates.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }
    #endregion

    #region Utility

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

    public bool HasPawnMoved(GamePieceID pawnID)
    {
        if (!this.pawnHasMoved.ContainsKey(pawnID))
        {
            Debug.LogFormat("Couldn't determine if pawn {0} has moved, it wasn't in dictionary", pawnID);
            return false;
        }
        return this.pawnHasMoved[pawnID];
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
    #endregion
}

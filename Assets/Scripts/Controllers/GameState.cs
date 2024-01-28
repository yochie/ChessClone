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

    //stores whether each pawn has just moved up 2 spaces for the sake of "En Passant" moves
    private Dictionary<GamePieceID, bool> pawnAdvancedBy2LastTurn;

    private Dictionary<PlayerColor, bool> playerChecked;

    private Dictionary<PlayerColor, bool> playerCheckMated;

    //Set to true when a move ends game on a draw
    private bool draw;

    //Scriptable object holding a type object (design pattern) for each game piece type
    //these objects define the possible moves for a piece type given a position and gamestate
    private PieceTypeData pieceTypeData;

    //Fresh copy constructor, used for initial setup
    public GameState(PlayerColor playerTurn, Dictionary<BoardPosition, GamePieceID> gamePieces, PieceTypeData pieceTypeData)
    {
        this.playerTurn = playerTurn;

        this.gamePieces = new();
        this.pieceHasMoved = new();
        this.pawnAdvancedBy2LastTurn = new();
        foreach (var (position, gamePieceID) in gamePieces)
        {
            this.gamePieces.Add(position, gamePieceID);
            this.pieceHasMoved.Add(gamePieceID, false);
            if (gamePieceID.typeID == PieceTypeID.pawn)
                this.pawnAdvancedBy2LastTurn[gamePieceID] = false;
        }
        this.playerChecked = new();
        this.playerChecked.Add(PlayerColor.white, false);
        this.playerChecked.Add(PlayerColor.black, false);
        this.playerCheckMated = new();
        this.playerCheckMated.Add(PlayerColor.white, false);
        this.playerCheckMated.Add(PlayerColor.black, false);
        this.draw = false;
        this.possibleMoves = new();
        this.pieceTypeData = pieceTypeData;
        this.UpdatePossibleMoves();
    }

    #region Cloning

    //Fully specified constructor, used when cloning
    public GameState(PlayerColor playerTurn,
                     Dictionary<BoardPosition, GamePieceID> gamePieces,
                     Dictionary<BoardPosition, List<Move>> possibleMoves,
                     Dictionary<GamePieceID, bool> pieceHasMoved,
                     Dictionary<GamePieceID, bool> pawnAdvancedBy2LastTurn,
                     Dictionary<PlayerColor, bool> playerChecked,
                     Dictionary<PlayerColor, bool> playerCheckMated,
                     bool draw,
                     PieceTypeData pieceTypeData)
    {
        this.playerTurn = playerTurn;
        this.gamePieces = gamePieces;
        this.possibleMoves = possibleMoves;
        this.pieceHasMoved = pieceHasMoved;
        this.pawnAdvancedBy2LastTurn = pawnAdvancedBy2LastTurn;
        this.playerChecked = playerChecked;
        this.playerCheckMated = playerCheckMated;
        this.draw = draw;
        this.pieceTypeData = pieceTypeData;
    }

    private GameState Clone()
    {
        //since these dictionaries only store value types, fine to clone using constructor
        //dont have their own function since they are kept private
        Dictionary<GamePieceID, bool> pieceHasMovedClone = new(this.pieceHasMoved);
        Dictionary<GamePieceID, bool> pawnAdvancedLastTurnClone = new(this.pawnAdvancedBy2LastTurn);
        Dictionary<PlayerColor, bool> playerCheckedClone = new(this.playerChecked);
        Dictionary<PlayerColor, bool> playerCheckMatedClone = new(this.playerCheckMated);        

        return new GameState(this.playerTurn,
                             this.GetGamePiecesClone(),
                             this.GetPossibleMovesClone(),
                             pieceHasMovedClone,
                             pawnAdvancedLastTurnClone,
                             playerCheckedClone,
                             playerCheckMatedClone,
                             this.draw,
                             this.pieceTypeData);
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
    //Main interface for game controller, will take care of everything that results from move
    public void DoMove(Move move, PieceTypeID promoteMoverTo)
    {
        this.ApplyMoveToBoardState(move);

        if (move.requiresPromotion && promoteMoverTo != PieceTypeID.none)
        {
            GamePieceID primaryMover = this.GetPieceAtPosition(move.to);
            this.ApplyPromotion(promotedPosition: move.to, promotedPieceOldID: primaryMover, promoteToType: promoteMoverTo);
        }

        this.UpdatePawnAdvanceState(move);

        this.UpdateOpponentCheckState(afterMoveByPlayer: this.playerTurn);

        this.SwapTurn();

        this.UpdatePossibleMoves();

        this.UpdateGameEndStates();
    }

    //sets gamePiece positions states and whether they have moved
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

    //Swaps out gamepiece ID in all state structures to the promoted version
    //since possible moves are generated anew every turn, no need to take care of these
    private void ApplyPromotion(BoardPosition promotedPosition, GamePieceID promotedPieceOldID, PieceTypeID? promoteToType)
    {
        GamePieceID promotedPieceNewID = GamePieceID.CreateGamePieceIDWithUniqueIndex(promotedPieceOldID.color, promoteToType.GetValueOrDefault());
        this.gamePieces[promotedPosition] = promotedPieceNewID;

        //no need to replace here since piece is no longer a pawn
        this.pawnAdvancedBy2LastTurn.Remove(promotedPieceOldID);

        this.pieceHasMoved.Remove(promotedPieceOldID);
        this.pieceHasMoved.Add(promotedPieceNewID, true);
    }

    //clears any previous state and sets advance state for any pawn that moved 2 tile vertically
    private void UpdatePawnAdvanceState(Move move)
    {
        GamePieceID moverID = this.gamePieces[move.to];
        bool isTwoStepAdvance = Mathf.Abs(move.to.yPosition - move.from.yPosition) == 2;
        foreach (GamePieceID piece in this.pawnAdvancedBy2LastTurn.Keys.ToList())
        {
            if (piece.Equals(moverID) && isTwoStepAdvance)
            {
                this.pawnAdvancedBy2LastTurn[piece] = true;
            }
            else
                this.pawnAdvancedBy2LastTurn[piece] = false;
        }
    }

    private void DeletePieceAt(BoardPosition position)
    {
        if (!gamePieces.ContainsKey(position))
        {
            Debug.Log("Couldn't find piece to remove");
        }
        this.gamePieces.Remove(position);
    }

    private void SwapTurn()
    {
        if (this.playerTurn == PlayerColor.white)
            this.playerTurn = PlayerColor.black;
        else
            this.playerTurn = PlayerColor.white;
    }

    //Should only be used on simulated game states (e.g. for checking if move will cause self-check, etc)
    //DoMove() should already take care of swapping turns during normal execution
    private void SetPlayerTurn(PlayerColor playerTurn)
    {
        this.playerTurn = playerTurn;
    }

    //"forKingThreats" is used when deciding whether the king would be threatened at some game state
    //in those cases, we consider self checking moves legal by opponent and ignore possible moves that can't pose threats
    private void UpdatePossibleMoves(bool forKingThreats = false)
    {
        Dictionary<BoardPosition, List<Move>> potentialMoves = new();
        foreach (var (position, gamePieceID) in this.gamePieces)
        {
            if (gamePieceID.color != this.playerTurn)
                continue;
            IPieceType pieceType = pieceTypeData.GetPieceTypeByID(gamePieceID.typeID);
            List<Move> possibleMovesFromPosition = pieceType.GetPossibleMovesFrom(this, position, threateningMovesOnly: forKingThreats);

            potentialMoves.Add(position, possibleMovesFromPosition);
        }

        //Remove any move that would result in check state for current player (either by causing self-check or leaving check)
        if (!forKingThreats)
        {
            foreach (var (position, moveList) in potentialMoves.ToList())
            {
                foreach (Move move in moveList.ToList())
                {
                    GameState clonedState = this.Clone();
                    clonedState.ApplyMoveToBoardState(move);
                    bool selfChecked = clonedState.KingThreatenedAtGameState(threatPlayer: Utility.GetOpponentColor(this.playerTurn));
                    if (selfChecked)
                    {
                        moveList.Remove(move);
                    }
                }
            }
        }

        //Copy result to possible moves, generating whole dict anew
        this.possibleMoves.Clear();
        foreach (var pair in potentialMoves)
        {
            this.possibleMoves[pair.Key] = pair.Value;
        }
    }

    private void UpdateOpponentCheckState(PlayerColor afterMoveByPlayer)
    {
        bool opponentChecked = this.KingThreatenedAtGameState(threatPlayer: afterMoveByPlayer);
        this.playerChecked[Utility.GetOpponentColor(afterMoveByPlayer)] = opponentChecked;
        //since a player can never be left checked after his own move, clear his flag
        this.playerChecked[afterMoveByPlayer] = false;
    }

    //TODO: check for other draw conditions, e.g. insufficient materials, 50 move rule, 3 repetition rule
    private void UpdateGameEndStates()
    {
        bool noPossibleMoves = !this.AnyPossibleMove();
        if (noPossibleMoves && this.GetCheckedPlayers().Contains(this.playerTurn))
        {
            this.playerCheckMated[this.playerTurn] = true;
        }
        else if (noPossibleMoves)
        {
            //stalemate condition
            this.draw = true;
        }
    }

    #endregion

    #region Utility
    //Makes clone, calculates its possible moves assuming given player is playing
    //return true if any possible move would eat opponents king
    private bool KingThreatenedAtGameState(PlayerColor threatPlayer)
    {
        //Calculate possible moves on clone to avoid updating actual possible moves
        GameState clonedState = this.Clone();
        clonedState.SetPlayerTurn(threatPlayer);
        //since players are still checked event if the threatening move would cause self check, we allow self checking when considering possible moves
        clonedState.UpdatePossibleMoves(forKingThreats: true);

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
        return this.playerChecked.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
    }

    internal List<PlayerColor> GetCheckMatedPlayers()
    {
        return this.playerCheckMated.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
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

    public bool DidPawnJustAdvanceBy2(GamePieceID pawnID)
    {
        if (!this.pawnAdvancedBy2LastTurn.ContainsKey(pawnID))
        {
            Debug.Log("Couldn't validate if pawn advanced by 2 last turn, not found in dictionary.");
            return false;
        }
        return this.pawnAdvancedBy2LastTurn[pawnID];
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

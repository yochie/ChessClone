using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameController : NetworkBehaviour
{
    #region Editor
    [SerializeField]
    private BoardView boardView;
    #endregion

    #region Server only
    private List<PlayerController> players = new();
    private GameState gameState;
    public GameState GameState { get => this.gameState; set => this.gameState = value; }
    #endregion

    #region Runtime
    private PlayerController localPlayer;
    public PlayerController LocalPlayer { get => this.localPlayer; set => this.localPlayer = value; }

    public static GameController Singleton { get; private set; }

    #endregion

    #region init
    private void Awake()
    {
        //wipe any previous gamecontrollers
        if(GameController.Singleton != null)
        {
            Debug.Log("Destroying previous gamecontroller");
            Destroy(GameController.Singleton.gameObject);
        }
        GameController.Singleton = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        this.gameState = this.InitializeGameStateFromBoardView();
    }

    [Server]
    private GameState InitializeGameStateFromBoardView()
    {
        Dictionary<BoardPosition, GamePiece> pieces = this.boardView.GetBoardViewPieces();
        return new GameState(pieces, PlayerColor.white);
    }

    [Server]
    private void StartGame()
    {
        Debug.Log("Both players connected. Starting game.");
        int startingPlayerIndex = UnityEngine.Random.Range(0,2);
        int index = 0;
        foreach(PlayerController player in this.players)
        {
            if (index == startingPlayerIndex)
                player.PlayerColor = PlayerColor.white;
            else
                player.PlayerColor = PlayerColor.black;
            index++;
        }
    }

    [Server]
    internal void AddPlayer(PlayerController playerController)
    {
        this.players.Add(playerController);
        if(this.players.Count == 2)
        {
            this.StartGame();
        }
    }

    #endregion

    #region Commands
    [Command(requiresAuthority = false)]
    public void CmdTryMove(BoardPosition from, BoardPosition to)
    {
        //validate on server, even though it will have been done clientsice, just to be sure
        if (IsValidMove(from, to)) {
            //Update game state
            this.GameState = this.GameState.Move(from, to);
            this.GameState = this.GameState.ChangeTurn();
            Debug.Log(GameState.playerTurn);
            //Perform clientside ui updates
            this.RpcUpdateBoardViewForMove(from, to);
        }
    }
    #endregion

    #region Rpcs
    [ClientRpc]
    private void RpcUpdateBoardViewForMove(BoardPosition from, BoardPosition to)
    {
        this.boardView.MovePieceSpriteToTile(from, to);
        this.boardView.UpdatePiecePosition(from, to);
    }
    #endregion

    #region GameState utility
    internal bool TileHoldsPiece(BoardPosition boardPosition)
    {
        return this.gameState.TileHoldsPiece(boardPosition);
    }

    internal bool ItsMyTurn()
    {
        PlayerColor myPlayerColor = this.localPlayer.PlayerColor;
        return this.gameState.playerTurn == myPlayerColor;
    }

    internal bool IOwnPieceAtTile(BoardPosition boardPosition)
    {
        PlayerColor myPlayerColor = this.localPlayer.PlayerColor;
        return this.gameState.IsTilePieceOwner(boardPosition, myPlayerColor);
    }

    internal static bool IsValidMove(BoardPosition startTile, BoardPosition endTile)
    {
        //todo : validate move
        return true;
    }
    #endregion

}

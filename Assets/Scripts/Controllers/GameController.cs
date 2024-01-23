using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameController : NetworkBehaviour
{
    #region Editor vars
    [SerializeField]
    private BoardView boardView;

    [SerializeField]
    private BoardInputHandler boardInputHandler;

    [SerializeField]
    private GameState gameState;
    #endregion

    #region Server only vars
    private List<PlayerController> players = new();
    private int connectedClientsCount = 0;
    
    #endregion

    #region Runtime vars
    public PlayerController LocalPlayer { get; set; }

    public static GameController Singleton { get; private set; }

    #endregion

    #region Init
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

        this.InitializeGameStateFromBoardView();
    }

    [Server]
    private void InitializeGameStateFromBoardView()
    {
        Dictionary<BoardPosition, GamePieceID> boardViewState = this.boardView.GetBoardViewState();
        this.gameState.Init(boardViewState, PlayerColor.white);
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
                this.RpcAssignColor(player, PlayerColor.white);
            else
                this.RpcAssignColor(player, PlayerColor.black);
            index++;
        }

        this.boardInputHandler.RpcSetInputAllowed();
    }


    [Server]
    internal void AddPlayer(PlayerController playerController)
    {
        this.players.Add(playerController);
    }

    #endregion

    #region Commands
    [Command(requiresAuthority = false)]
    public void CmdCountConnectedClient()
    {
        this.connectedClientsCount++;
        if (this.connectedClientsCount == 2)
        {
            this.StartGame();
        }
    }


    [Command(requiresAuthority = false)]
    public void CmdTryMove(BoardPosition from, BoardPosition to)
    {
        //validate on server, even though it will have been done clientsice, just to be sure
        if (this.gameState.IsValidMove(from, to))
        {
            //Update game state
            this.gameState.MovePiece(from, to);
            this.gameState.ChangeTurn();
            Debug.Log(this.gameState.PlayerTurn);
            //Perform clientside ui updates
            this.RpcUpdateBoardViewForMove(from, to);
        }
    }
    #endregion

    #region Rpcs
    [ClientRpc]
    private void RpcUpdateBoardViewForMove(BoardPosition from, BoardPosition to)
    {
        this.boardView.MovePieceSpriteToBoardPosition(from, to);
        this.boardView.UpdatePiecePosition(from, to);
    }

    [ClientRpc]
    private void RpcAssignColor(PlayerController player, PlayerColor color)
    {
        player.AssignColor(color);
    }
    #endregion

}

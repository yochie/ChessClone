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

    [SerializeField]
    private MainUI ui;

    [SerializeField]
    private AudioClip moveSound;
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
                this.RpcPlayerAssignColor(player, PlayerColor.white);
            else
                this.RpcPlayerAssignColor(player, PlayerColor.black);
            index++;
        }

        this.InitializeGameStateFromBoardView();

        this.boardInputHandler.RpcSetInputAllowed();
    }


    [Server]
    internal void RegisterPlayerOnServer(PlayerController playerController)
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
    public void CmdTryMove(Move move)
    {
        //validate on server, even though it will have been done clientsice, just to be sure
        if (this.gameState.IsValidMove(move))
        {
            Debug.LogFormat("{0} moved from {1} to {2}", this.gameState.GetPieceAtPosition(move.from), move.from, move.to);
            //Update game state
            this.gameState.MovePiece(move);
            //Perform clientside ui updates
            foreach(PlayerController player in this.players)
            {
                this.TargetRpcPostMoveClientUpdates(player.connectionToClient, move, this.gameState.PlayerTurn == player.PlayerColor);
            }            
        }
        else
        {
            Debug.LogFormat("Failed to move {0} from {1} to {2}", this.gameState.GetPieceAtPosition(move.from), move.from, move.to);
        }
    }
    #endregion

    #region Rpcs
    [TargetRpc]
    private void TargetRpcPostMoveClientUpdates(NetworkConnectionToClient target, Move move, bool yourTurn)
    {
        this.boardView.PostMoveUpdates(move);
        AudioManager.Singleton.PlaySoundEffect(this.moveSound);
        this.ui.TriggerTurnPopup(yourTurn);
    }

    [ClientRpc]
    private void RpcPlayerAssignColor(PlayerController player, PlayerColor color)
    {
        player.AssignColor(color);
    }
    #endregion

}

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

    //only holds state that is needed client side
    //updated whenever server game state is modified
    [SerializeField]
    private SyncedGameState syncedGameState;

    [SerializeField]
    private PieceTypeData pieceTypeData;

    [SerializeField]
    private MainUI ui;

    [SerializeField]
    private AudioClip moveSound;

    [SerializeField]
    private EscapeMenuController escapeMenu;
    #endregion

    #region Server only vars
    private GameState serverGameState;
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
        this.ui.DisplayWaitingMessage();
    }

    //Called once 2 clients connected, their player objects started on clients and counted to server
    [Server]
    private void StartGame()
    {
        Debug.Log("Both players connected. Starting game.");
        int startingPlayerIndex = UnityEngine.Random.Range(0,2);
        int index = 0;

        foreach (PlayerController player in this.players)
        {
            if (index == startingPlayerIndex)
                this.RpcPlayerAssignColor(player, PlayerColor.white);
            else
                this.RpcPlayerAssignColor(player, PlayerColor.black);
            index++;
        }
        this.boardInputHandler.RpcSetInputAllowed();
        this.RpcEnableConcedeButton();
    }


    [Server]
    private void InitializeGameStateFromBoardView()
    {
        Dictionary<BoardPosition, GamePieceID> boardViewState = this.boardView.GetBoardViewState();
        this.serverGameState = new(PlayerColor.white, boardViewState, this.pieceTypeData);
        this.syncedGameState.Init(this.serverGameState);
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
    public void CmdTryMove(Move move, PieceTypeID promoteMoverTo)
    {
        if(move.requiresPromotion && promoteMoverTo == PieceTypeID.none)
        {
            Debug.Log("Move requires promotion but no selected option was provided");
            return;
        } else if (!move.requiresPromotion && promoteMoverTo != PieceTypeID.none)
        {
            Debug.Log("Move doesn't require promotion but a selection was provided. Will continue ignoring promotion.");
        }
        //validate on server, even though it will have been done clientsice, just to be sure
        if (this.serverGameState.IsPossibleMove(move))
        {
            Debug.LogFormat("{0} moved from {1} to {2}", this.serverGameState.GetPieceAtPosition(move.from), move.from, move.to);
            //Update game state
            this.serverGameState.DoMove(move, promoteMoverTo);
            this.syncedGameState.UpdateState(this.serverGameState);
            List<PlayerColor> checkMatedPlayers = this.serverGameState.GetCheckMatedPlayers();
            bool gameDrawn = this.serverGameState.GetDraw();
            bool gameEnded = checkMatedPlayers.Count > 0 || gameDrawn;
            if (gameEnded)
            {
                Debug.Log("game ended");
            }
            GamePieceID promotedPieceID = new GamePieceID(PlayerColor.white, PieceTypeID.none, -1);
            if (move.requiresPromotion && promoteMoverTo != PieceTypeID.none)
            {
                promotedPieceID = this.serverGameState.GetPieceAtPosition(move.to);
            }
            //Perform clientside ui updates
            foreach(PlayerController player in this.players)
            {

                this.TargetRpcPostMoveClientUpdates(player.connectionToClient,
                                                    move,
                                                    promotedPieceID,
                                                    this.serverGameState.PlayerTurn == player.PlayerColor,
                                                    this.serverGameState.GetCheckedPlayers(),
                                                    gameEnded,
                                                    checkMatedPlayers);
            }            
        }
        else
        {
            Debug.LogFormat("Failed to move {0} from {1} to {2}", this.serverGameState.GetPieceAtPosition(move.from), move.from, move.to);
        }
    }

    [Command(requiresAuthority = false)]
    internal void CmdConcedeGame(PlayerColor concederColor)
    {
        Debug.Log("game conceded");
        //Perform clientside ui updates
        foreach (PlayerController player in this.players)
        {
            this.RpcConcessionClientUpdate(concederColor);
        }
    }

    #endregion

    #region Rpcs
    [TargetRpc]
    private void TargetRpcPostMoveClientUpdates(NetworkConnectionToClient target,
                                                Move move,
                                                GamePieceID promotedPieceID,
                                                bool yourTurn,
                                                List<PlayerColor> checkedPlayers,
                                                bool gameEnded,
                                                List<PlayerColor> checkMatedPlayers)
    {
        this.boardView.PostMoveUpdates(move, checkedPlayers, promotedPieceID);
        AudioManager.Singleton.PlaySoundEffect(this.moveSound);
        if (!gameEnded)
            this.ui.TriggerTurnPopup(yourTurn, checkedPlayers.Count > 0);
        else
        {
            bool isDraw;
            PlayerColor? winner;
            if (checkMatedPlayers.Count != 1) {
                isDraw = true;
                winner = null;
            }
            else
            {
                isDraw = false;
                winner = Utility.GetOpponentColor(checkMatedPlayers[0]);
            }                

            this.ui.TriggerEndGamePopup(isDraw, isConcession : false, winner);
        }
    }

    [ClientRpc]

    private void RpcConcessionClientUpdate(PlayerColor concederColor)
    {
        this.ui.TriggerEndGamePopup(isDraw: false, isConcession: true, winner: Utility.GetOpponentColor(concederColor));
    }

    [ClientRpc]
    private void RpcPlayerAssignColor(PlayerController player, PlayerColor color)
    {        
        player.AssignColor(color);
        if(player.isLocalPlayer)
        {
            this.ui.SetupBoardForPlayer(player);
        }
    }

    [ClientRpc]
    private void RpcEnableConcedeButton()
    {
        this.escapeMenu.EnableConcedeButton();
    }
    #endregion

}

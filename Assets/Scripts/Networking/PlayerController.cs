using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerController : NetworkBehaviour
{
    [SyncVar]
    private PlayerColor playerColor;
    public PlayerColor PlayerColor { get => this.playerColor; }

    public void AssignColor(PlayerColor color)
    {
        //Debug.LogFormat("Color assigned : {0}", color);
        this.playerColor = color;
    } 

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (GameController.Singleton != null)
            GameController.Singleton.LocalPlayer = this;
        else
            //shouldn't occur, but just in case
            Debug.Log("Couldn't set local player on gamecontroller, gamecontroller singleton not ready yet.");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        if (GameController.Singleton != null)
            GameController.Singleton.RegisterPlayerOnServer(this);
        else
            //shouldn't occur, but just in case 
            Debug.Log("Couldn't set local player on gamecontroller, gamecontroller singleton not ready yet.");
    }

    //Counts players connected to server
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!isLocalPlayer)
            return;

        if (GameController.Singleton != null)
            GameController.Singleton.CmdCountConnectedClient();
        else
            //shouldn't occur, but just in case 
            Debug.Log("Couldn't count player connections on gamecontroller, gamecontroller singleton not ready yet.");
    }
}

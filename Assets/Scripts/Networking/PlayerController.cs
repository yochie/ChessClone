using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerController : NetworkBehaviour
{
    public PlayerColor PlayerColor { get; set; }

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
            GameController.Singleton.AddPlayer(this);
        else
            //shouldn't occur, but just in case 
            Debug.Log("Couldn't set local player on gamecontroller, gamecontroller singleton not ready yet.");
    }
}

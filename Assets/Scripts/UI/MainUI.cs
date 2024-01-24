using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField]
    private TurnPopup turnPopup;

    [SerializeField]
    private BoardView boardView;

    [SerializeField]
    private Camera mainCamera;

    public void TriggerTurnPopup(bool yourTurn, bool afterCheckingMove)
    {
        this.turnPopup.TriggerPopup(yourTurn, afterCheckingMove);
    }

    internal void SetupBoardForPlayer(PlayerController player)
    {
        bool youAreWhite = player.PlayerColor == PlayerColor.white;
        if (!youAreWhite)
        {
            this.mainCamera.transform.Rotate(new Vector3(0, 0, 180));
            this.boardView.Rotate();         
        }

        this.TriggerTurnPopup(youAreWhite, afterCheckingMove: false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField]
    private TurnPopup turnPopup;

    public void TriggerTurnPopup(bool yourTurn)
    {
        this.turnPopup.TriggerPopup(yourTurn);
    }
}

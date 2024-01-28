using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour
{
    [SerializeField]
    private TurnPopup turnPopup;

    [SerializeField]
    private WaitingPopup waitingPopup;

    [SerializeField]
    private EndGamePopup endGamePopup;

    [SerializeField]
    private BoardView boardView;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private PromotionSelector promotionSelector;

    public void DisplayWaitingMessage()
    {
        this.waitingPopup.Display();
    }

    internal void DisplayPromotionOptions(Move move)
    {
        this.promotionSelector.DisplayFor(move);
    }

    public void TriggerTurnPopup(bool yourTurn, bool afterCheckingMove)
    {
        this.turnPopup.TriggerPopup(yourTurn, afterCheckingMove);
    }

    internal void TriggerEndGamePopup(bool isDraw, bool isConcession, PlayerColor? winner)
    {
        this.endGamePopup.gameObject.SetActive(true);

        this.endGamePopup.TriggerPopup(isDraw, isConcession, winner);
    }

    internal void SetupBoardForPlayer(PlayerController player)
    {
        bool youAreWhite = player.PlayerColor == PlayerColor.white;
        if (!youAreWhite)
        {
            this.mainCamera.transform.Rotate(new Vector3(0, 0, 180));
            this.boardView.Rotate();         
        }
        StartCoroutine(this.GameStartAnimationsCoroutine(youAreWhite));
    }

    private IEnumerator GameStartAnimationsCoroutine(bool youAreWhite)
    {
        //fadeout is triggered even for remote client where popup is not displayed
        //popup still takes care of blocking input until first turn popup is triggered
        this.waitingPopup.FadeOut();
        yield return new WaitForSeconds(this.waitingPopup.GetFadeOutDuration());
        this.TriggerTurnPopup(youAreWhite, afterCheckingMove: false);
    }

}

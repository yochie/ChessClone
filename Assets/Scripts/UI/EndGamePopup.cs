using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class EndGamePopup : MonoBehaviour

{
    [SerializeField]
    GameObject popup;

    [SerializeField]
    TextMeshProUGUI winnerLabel;

    internal void TriggerPopup(bool draw, bool isConcession, PlayerColor? winner)
    {
        if (draw) {
            this.winnerLabel.text = "Game over\nDraw!";
        } else
        {
            this.winnerLabel.text = string.Format("{0}\nWinner : {1}", isConcession ? "Game conceded" : "Checkmate", winner.GetValueOrDefault());
        }
        this.popup.SetActive(true);
    }
}
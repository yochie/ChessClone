using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class PromotionSelector : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    private Move forMove;

    public void DisplayFor(Move move)
    {
        this.forMove = move;
        this.content.SetActive(true);
    }

    private void SendMoveCommand(PieceTypeID chosenPieceType)
    {
        GameController.Singleton.CmdTryMove(this.forMove, promoteMoverTo: chosenPieceType);
        this.content.SetActive(false);

    }

    public void OnQueenButtonClicked()
    {
        this.SendMoveCommand(PieceTypeID.queen);
    }

    public void OnRookButtonClicked()
    {
        this.SendMoveCommand(PieceTypeID.rook);
    }

    public void OnBishopButtonClicked()
    {
        this.SendMoveCommand(PieceTypeID.bishop);
    }

    public void OnKnightButtonClicked()
    {
        this.SendMoveCommand(PieceTypeID.knight);
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    [SerializeField]
    private PlayerColor ownerColor;

    [SerializeField]
    private PieceTypeID pieceTypeID;

    [SerializeField]
    private int pieceIndex;

    [SerializeField]
    private Color checkedColor;

    [SerializeField]
    private SpriteRenderer sprite;

    public PlayerColor GetOwnerColor()
    {
        return this.ownerColor;
    }

    public PieceTypeID GetPieceTypeID()
    {

        return this.pieceTypeID;
    }

    public int GetIndex()
    {

        return this.pieceIndex;
    }

    internal void SetChecked(bool isChecked)
    {
        this.sprite.color = isChecked ? this.checkedColor : Color.white;
    }

    internal void SetIndex(int index)
    {
        this.pieceIndex = index;
    }
}

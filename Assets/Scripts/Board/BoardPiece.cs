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

    public PlayerColor GetOwnerID()
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
}

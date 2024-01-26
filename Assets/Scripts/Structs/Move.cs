using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Move
{
    public readonly BoardPosition from;
    public readonly BoardPosition to;
    public readonly bool eats;
    public readonly BoardPosition eatPosition;

    //used for castling
    public readonly bool includesSecondaryMove;
    public readonly BoardPosition from2;
    public readonly BoardPosition to2;

    public Move(BoardPosition from, BoardPosition to, bool eats, BoardPosition? eatPosition = null, bool includesSecondaryMove = false, BoardPosition? from2 = null, BoardPosition? to2 = null)
    {
        this.from = from;
        this.to = to;
        this.eats = eats;
        this.eatPosition = eatPosition == null ? BoardPosition.None() : eatPosition.GetValueOrDefault();
        this.includesSecondaryMove = includesSecondaryMove;
        this.from2 = from2 == null ? BoardPosition.None() : from2.GetValueOrDefault();
        this.to2 = to2 == null ? BoardPosition.None() : to2.GetValueOrDefault();
    }

    public override string ToString()
    {
        if(!includesSecondaryMove)
            return string.Format("from : {0}\nto: {1}\neats{2}", this.from, this.to, this.eats);
        else
            return string.Format("from : {0}\nto: {1}\neats{2}\n+ secondary move...", this.from, this.to, this.eats);
    }
}

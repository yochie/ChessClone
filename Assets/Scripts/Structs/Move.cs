using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Move
{
    public readonly BoardPosition from;
    public readonly BoardPosition to;
    public readonly bool eats;
    public readonly BoardPosition eatPosition;

    public Move(BoardPosition from, BoardPosition to, bool eats, BoardPosition? eatPosition = null)
    {
        this.from = from;
        this.to = to;
        this.eats = eats;
        this.eatPosition = eatPosition == null ? BoardPosition.None() : eatPosition.GetValueOrDefault();
    }

    public override string ToString()
    {
        return string.Format("from : {0}\nto: {1}\neats{2}", this.from, this.to, this.eats);
    }
}

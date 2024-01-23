using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct BoardPosition
{
    //left to right, bottom to top, 0 indexed
    public readonly short xPosition;
    public readonly short yPosition;

    public static readonly short maxX = 7;
    public static readonly short maxY = 7;

    public BoardPosition(int x, int y)
    {
        this.xPosition = (short) x;
        this.yPosition = (short) y;
    }

    public BoardPosition Add(Vector2Int vector)
    {
        return new BoardPosition(this.xPosition + vector.x, this.yPosition + vector.y);
    }

    public override bool Equals(object obj)
    {
        return obj is BoardPosition position &&
               this.xPosition == position.xPosition &&
               this.yPosition == position.yPosition;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.xPosition, this.yPosition);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", this.xPosition, this.yPosition);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//TODO : convert to use flyweight pattern
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

    public static BoardPosition None()
    {
        return new BoardPosition(-1, -1);
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

    internal bool IsOnBoard()
    {
        bool xInRange = Enumerable.Range(0, BoardPosition.maxX + 1).Contains(this.xPosition);
        bool yInRange = Enumerable.Range(0, BoardPosition.maxY + 1).Contains(this.yPosition);
        return xInRange && yInRange;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPositionCalculator
{

    //Excluding origin
    //public static List<BoardPosition> GetPositionsOnHorizontalLine(BoardPosition origin)
    //{
    //    List<BoardPosition> line = new();
    //    for(int x = 0; x < BoardPosition.maxX; x++)
    //    {
    //        if(x != origin.xPosition)
    //        line.Add(new BoardPosition(x, origin.yPosition));
    //    }
    //    return line;
    //}

    ////Excluding origin
    //public static List<BoardPosition> GetPositionsOnVerticalLine(BoardPosition origin)
    //{
    //    List<BoardPosition> line = new();
    //    for (int y = 0; y < BoardPosition.maxY; y++)
    //    {
    //        if (y != origin.yPosition)
    //            line.Add(new BoardPosition(origin.xPosition, y));
    //    }
    //    return line;
    //}

    ////Excluding origin
    ////forward as in forward slash    
    //public static List<BoardPosition> GetPositionsOnForwardDiagonal(BoardPosition origin)
    //{
    //    List<BoardPosition> line = new();
    //    //Go back
    //    int x = origin.xPosition, y = origin.yPosition;
    //    while (x > 0 && y > 0)
    //    {
    //        line.Add(new BoardPosition(--x, --y));
    //    }

    //    //Go forward
    //    x = origin.xPosition;
    //    y = origin.yPosition;
    //    while (x < BoardPosition.maxX && y < BoardPosition.maxY)
    //    {
    //        line.Add(new BoardPosition(++x, ++y));
    //    }
    //    return line;
    //}

    ////Excluding origin
    ////Back as in back slash    
    //public static List<BoardPosition> GetPositionsOnBackDiagonal(BoardPosition origin)
    //{
    //    List<BoardPosition> line = new();
    //    //Go back
    //    int x = origin.xPosition, y = origin.yPosition;
    //    while (x > 0 && y < BoardPosition.maxY)
    //    {
    //        line.Add(new BoardPosition(--x, ++y));
    //    }

    //    //Go forward
    //    x = origin.xPosition;
    //    y = origin.yPosition;
    //    while (x < BoardPosition.maxX && y > 0)
    //    {
    //        line.Add(new BoardPosition(++x, --y));
    //    }
    //    return line;
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField]    
    private int xPosition;


    [SerializeField]
    private int yPosition;


    public BoardPosition GetPosition() {
        return new BoardPosition(this.xPosition, this.yPosition);
    }
}

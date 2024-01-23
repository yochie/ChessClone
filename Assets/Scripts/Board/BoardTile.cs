using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardTile : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]    
    private int xPosition;

    [SerializeField]
    private int yPosition;

    [SerializeField]
    private Image highlightImage;

    [SerializeField]
    private Color baseColor;

    [SerializeField]
    private BoardInputHandler inputHandler;

    [SerializeField]

    public BoardPosition GetBoardPosition() {
        return new BoardPosition((short) this.xPosition, (short) this.yPosition);
    }

    public void Highlight(Color color)
    {
        this.highlightImage.color = color;
    }

    internal void UnHighlight()
    {
        this.highlightImage.color = this.baseColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.inputHandler.OnTileBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.inputHandler.OnTileDrag(this, eventData.pointerCurrentRaycast.worldPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.inputHandler.OnTileEndDrag(this);
    }

    public void OnMouseEnter()
    {
        this.inputHandler.HoveredTile = this;
    }

    public void OnMouseExit()
    {
        this.inputHandler.HoveredTile = null;
    }


}

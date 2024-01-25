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

    public BoardPosition GetBoardPosition() {
        return new BoardPosition(this.xPosition, this.yPosition);
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
        //Not sure why, but raycast was return 0 when not hovering a canvas, using raw position and setting z to 0 instead
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 0));
        worldPosition.z = 0;
        this.inputHandler.OnTileDrag(this, worldPosition);
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

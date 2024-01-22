using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardTile : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]    
    private int xPosition;

    [SerializeField]
    private int yPosition;

    [SerializeField]
    private SpriteRenderer highlightSprite;

    [SerializeField]
    private Color baseColor;

    [SerializeField]
    private BoardInputHandler inputHandler;


    public BoardPosition GetPosition() {
        return new BoardPosition(this.xPosition, this.yPosition);
    }

    public void Highlight(bool highlighted)
    {
        this.highlightSprite.color = highlighted ? Color.green : this.baseColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.inputHandler.OnTileBeginDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.inputHandler.OnTileEndDrag(this);
    }
}
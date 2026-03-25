using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveGizmo : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private float startPointX;
    private float startPointY;
    public float speedX;
    public float speedY;
    private float maxDragDistance = 200f;
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPointX = eventData.position.x;
        startPointY = eventData.position.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float currentPointX = eventData.position.x;
        float currentPointY = eventData.position.y;
        float dragDistanceX = currentPointX - startPointX;
        float dragDistanceY = currentPointY - startPointY;

        speedX = Math.Clamp(dragDistanceX / maxDragDistance, -1f, 1f);
        speedY = Math.Clamp(dragDistanceY / maxDragDistance, -1f, 1f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        speedX = 0;
        speedY = 0;
    }
}
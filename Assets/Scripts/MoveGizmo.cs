using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveGizmo : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    private float startPointX;
    private float startPointY;
    private Vector2 startPoint;
    public float speedX;
    public float speedY;
    private float maxDragDistance = 100f;
    [SerializeField] private GameObject tapAndSlideHint;
    [SerializeField] private float joystickRange = 50f;
    private RectTransform rectTransform;
    private bool dragHintDismissed;
    string hintKey = "MoveGizmoHint";

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        dragHintDismissed = PlayerPrefs.GetInt(hintKey, 0) == 1;
        tapAndSlideHint.SetActive(!dragHintDismissed);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        startPointX = eventData.position.x;
        startPointY = eventData.position.y;
        startPoint = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float currentPointX = eventData.position.x;
        float currentPointY = eventData.position.y;
        float dragDistanceX = currentPointX - startPointX;
        float dragDistanceY = currentPointY - startPointY;

        speedX = Math.Clamp(dragDistanceX / maxDragDistance, -1f, 1f);
        speedY = Math.Clamp(dragDistanceY / maxDragDistance, -1f, 1f);

        MoveTheJoystick();

        if (!dragHintDismissed)
        {
            float dragDistance = Vector2.Distance(startPoint, eventData.position);

            if (dragDistance > 120f)
            {
                dragHintDismissed = true;
                PlayerPrefs.SetInt(hintKey, 1);
                PlayerPrefs.Save();
                tapAndSlideHint.SetActive(false);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        speedX = 0;
        speedY = 0;
        MoveTheJoystick();
    }

    private void MoveTheJoystick()
    {
        if (rectTransform == null) return;

        float x = Mathf.Clamp(speedX * joystickRange, -joystickRange, joystickRange);
        float y = Mathf.Clamp(speedY * joystickRange, -joystickRange, joystickRange);

        rectTransform.anchoredPosition = new Vector2(x, y);
    }
}
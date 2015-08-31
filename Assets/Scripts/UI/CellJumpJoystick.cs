using System;
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 六边形跳跃摇杆
/// </summary>
public class CellJumpJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum StateEnum
    {
        Idle,
        InvalidDragging,
        ValidDragging,
    }

    public StateEnum State = StateEnum.Idle;

    public CellJumper CellJumper;

    public RectTransform TouchCircle;
    public RectTransform TouchSpot;
    public RectTransform DragDrop;
    public GameObject OriginalDrop;

    public float DragThreshold = 10;
    public Vector2 PressPosition;
    public Vector2 CurrentPosition;


    void Awake()
    {
        Init();
    }

    public void Init()
    {
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        OriginalDrop.SetActive(false);
    }

    void Update()
    {
        if (State == StateEnum.InvalidDragging || State == StateEnum.ValidDragging)
        {
            var dragDisplacement = CurrentPosition - PressPosition;
            var dragMagnitude = dragDisplacement.magnitude;
            if (dragMagnitude > DragThreshold)//有效拖动
            {
                if (State == StateEnum.InvalidDragging)
                {
                    TouchCircle.gameObject.SetActive(true);
                    TouchSpot.gameObject.SetActive(true);
                    DragDrop.gameObject.SetActive(true);
                }

                var validDragDisplacement = CalcValidDragDisplacement(dragDisplacement);

                TouchSpot.localPosition = validDragDisplacement * (1800f / Screen.width);
                DragDrop.parent.right = validDragDisplacement;
                DragDrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, validDragDisplacement.magnitude + 71 + DragThreshold + 20);

                CellJumper.JumpToward(DirectionToHexagonDirectionID(validDragDisplacement));
            }
            else
            {
                if (State == StateEnum.ValidDragging)
                {
                    State = StateEnum.InvalidDragging;
                    TouchCircle.gameObject.SetActive(false);
                    TouchSpot.gameObject.SetActive(false);
                    DragDrop.gameObject.SetActive(false);
                }

            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        State = StateEnum.InvalidDragging;
        PressPosition = eventData.pressPosition;
        CurrentPosition = eventData.position;

        var pos = PressPosition * 1800f/Screen.width;
        TouchCircle.localPosition = pos;
        TouchSpot.localPosition = Vector3.zero;
        OriginalDrop.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;

        State = StateEnum.Idle;
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        OriginalDrop.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
    }

    Vector3 CalcValidDragDisplacement(Vector2 dragDisplacement)
    {
        var dragMagnitude = dragDisplacement.magnitude;
        var validDragMagnitude = dragMagnitude - DragThreshold;
        var validDragDisplacement = dragDisplacement.normalized * validDragMagnitude;
        return validDragDisplacement;
    }

    int DirectionToHexagonDirectionID(Vector2 dir)
    {
        const float rad60 = Mathf.PI/3f;
        return Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(dir.y, dir.x), Mathf.PI * 2) / rad60) % 6;
    }
}
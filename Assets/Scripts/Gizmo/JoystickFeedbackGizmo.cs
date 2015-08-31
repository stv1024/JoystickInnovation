using System;
using Fairwood.Math;
using UnityEngine;

/// <summary>
/// 摇杆反馈Gizmo
/// </summary>
public class JoystickFeedbackGizmo : MonoBehaviour
{
    public Joystick Joystick;
    public Transform CenterObject;
    public GameObject MinCircle;
    public GameObject FallSpot;
    public GameObject DragSpot;

    private Joystick.StateEnum _lastState;

    void Awake()
    {
        Hide();
        MinCircle.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 2.5f * Joystick.WorldAimingDisplacementThreshold);
        MinCircle.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2.5f * Joystick.WorldAimingDisplacementThreshold);
    }

    void Update()
    {
        if (!CenterObject)
        {
            var unit = Joystick.Walker;
            if (unit) CenterObject = unit.transform;
            if (!CenterObject) return;
        }
        transform.position = CenterObject.position.SetV3Y(0.01f);
        if (Joystick.State != _lastState)
        {
            StateTransition(_lastState, Joystick.State);
            _lastState = Joystick.State;
        }
        if (Joystick.State != Joystick.StateEnum.Idle)
        {
            FallSpot.SetActive(Joystick.IsValidDrag);
            FallSpot.transform.position = transform.position + Joystick.WorldActualDisplacement;
            DragSpot.transform.position = transform.position + Joystick.WorldAimingDisplacement;

        }
    }

    void StateTransition(Joystick.StateEnum lastState, Joystick.StateEnum curState)
    {
        switch (curState)
        {
            case Joystick.StateEnum.Idle:
                Hide();
                break;
            case Joystick.StateEnum.InvalidDragging:
            case Joystick.StateEnum.ValidDragging:
                MinCircle.SetActive(true);
                FallSpot.SetActive(true);
                DragSpot.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Hide()
    {
        MinCircle.SetActive(false);
        FallSpot.SetActive(false);
        DragSpot.SetActive(false);
    }
}
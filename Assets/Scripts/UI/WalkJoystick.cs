using System.Collections.Generic;
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 覆盖屏幕的走位控制器
/// </summary>
public class WalkJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum StateEnum
    {
        Idle,
        InvalidDragging,
        ValidDragging,
    }

    public StateEnum State = StateEnum.Idle;

    public bool UsePathfinding;
    public Transform Walker;
    DirectionWalker _directionWalker;
    PathfindingWalker _pathfindingWalker;

    public float DragThreshold = 40;
    public Transform MainCameraTra;
    public Transform AssistPlane;
    public RectTransform TouchCircle;
    public RectTransform TouchSpot;
    public RectTransform DragDrop;

    public RectTransform JoystickAssistCircle;
    public RectTransform JoystickAssistSpot;

    public Vector2 PressPosition;
    public Vector2 CurrentPosition;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        _directionWalker = Walker.GetComponent<DirectionWalker>();
        _pathfindingWalker = Walker.GetComponent<PathfindingWalker>();

        State = StateEnum.Idle;
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        if (JoystickAssistCircle) JoystickAssistCircle.gameObject.SetActive(false);
        if (JoystickAssistSpot) JoystickAssistSpot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (State == StateEnum.InvalidDragging || State == StateEnum.ValidDragging)
        {
            ResetAssistPlaneRotation();
            var dragDisplacement = CurrentPosition - PressPosition;
            var dragMagnitude = dragDisplacement.magnitude;
            if (dragMagnitude > DragThreshold)//有效拖动
            {
                if (State == StateEnum.InvalidDragging)
                {
                    State = StateEnum.ValidDragging;
                    TouchCircle.gameObject.SetActive(true);
                    TouchSpot.gameObject.SetActive(true);
                    DragDrop.gameObject.SetActive(true);
                    if (JoystickAssistCircle) JoystickAssistCircle.gameObject.SetActive(true);
                    if (JoystickAssistSpot) JoystickAssistSpot.gameObject.SetActive(true);
                }

                var validDragDisplacement = CalcValidDragDisplacement(dragDisplacement);

                TouchSpot.localPosition = validDragDisplacement * (1800f / Screen.width);
                DragDrop.parent.right = validDragDisplacement;
                DragDrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, validDragDisplacement.magnitude + 71 + DragThreshold + 20);

                var geodesicDisplacement = DragDisplacementToGeodesicDisplacement(validDragDisplacement);

                //var arrow = MainController.Instance.Arrow;
                //var eA = arrow.localEulerAngles;
                //eA.z = Quaternion.FromToRotation(Vector3.right, geodesicDisplacement).eulerAngles.y;
                //arrow.localEulerAngles = -eA;

                if (JoystickAssistCircle) JoystickAssistCircle.position = Walker.transform.position.SetV3Y(0.01f);
                if (JoystickAssistSpot)
                {
                    var spotPos = Vector3.ClampMagnitude(geodesicDisplacement * 0.03f, 9003.88f);
                    spotPos = new Vector3(spotPos.x, spotPos.z, -0.01f);
                    JoystickAssistSpot.localPosition = spotPos;
                }

                if (!UsePathfinding) _directionWalker.WalkTowards(geodesicDisplacement);
                else _pathfindingWalker.WalkTo(Walker.transform.position + geodesicDisplacement);
            }
            else
            {
                if (State == StateEnum.ValidDragging)
                {
                    State = StateEnum.InvalidDragging;
                    TouchCircle.gameObject.SetActive(false);
                    TouchSpot.gameObject.SetActive(false);
                    DragDrop.gameObject.SetActive(false);
                    if (JoystickAssistCircle) JoystickAssistCircle.gameObject.SetActive(false);
                    if (JoystickAssistSpot) JoystickAssistSpot.gameObject.SetActive(false);

                    if (!UsePathfinding) _directionWalker.Stop();
                    else _pathfindingWalker.Stop();
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
        ResetAssistPlaneRotation();

        State = StateEnum.Idle;
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        if (JoystickAssistCircle) JoystickAssistCircle.gameObject.SetActive(false);
        if (JoystickAssistSpot) JoystickAssistSpot.gameObject.SetActive(false);
        if (!UsePathfinding) _directionWalker.Stop();
        else _pathfindingWalker.Stop();
    }

    public void OnDrag(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
    }

    void ResetAssistPlaneRotation()
    {
        AssistPlane.right = MainCameraTra.right;
    }

    Vector3 DragDisplacementToGeodesicDisplacement(Vector2 dragDisplacement)
    {
        //方案2
        var cam = MainCameraTra.GetComponent<Camera>();
        var screenStartPos = cam.WorldToScreenPoint(Walker.position.SetV3Y(0));

        var ray0 = cam.ScreenPointToRay(screenStartPos);
        var startPos = ray0.GetPoint(-ray0.origin.y/ray0.direction.y);
        //Debug.DrawRay(startPos, Vector3.right, Color.blue, 1);
        //Debug.DrawRay(startPos, Vector3.forward, Color.blue, 1);
        var ray1 = cam.ScreenPointToRay(screenStartPos.ToVector2() + dragDisplacement);
        var endPos = ray1.GetPoint(-ray1.origin.y / ray1.direction.y);
        //Debug.DrawRay(endPos, Vector3.right, Color.red, 1);
        //Debug.DrawRay(endPos, Vector3.forward, Color.red, 1);
        var geodesicDisplacement = (endPos - startPos).normalized*dragDisplacement.magnitude;
        //Debug.DrawRay(startPos, geodesicDisplacement, Color.black, 1);

        //return AssistPlane.TransformDirection(new Vector3(dragDisplacement.x, 0, dragDisplacement.y)) *
        //                MainController.Instance.MoveMode * MainController.Instance.DragToDisplacementRatio; //方案1
        //var geodesicDisplacement = MainCameraTra.TransformDirection(dragDisplacement).SetV3Y(0).normalized*
        //                           dragDisplacement.magnitude*MainController.Instance.MoveMode*
        //                           MainController.Instance.DragToDisplacementRatio;//方案3
        return geodesicDisplacement;
    }

    Vector3 CalcValidDragDisplacement(Vector2 dragDisplacement)
    {
        var dragMagnitude = dragDisplacement.magnitude;
        var validDragMagnitude = dragMagnitude - DragThreshold;
        var validDragDisplacement = dragDisplacement.normalized * validDragMagnitude;
        return validDragDisplacement;
    }
}
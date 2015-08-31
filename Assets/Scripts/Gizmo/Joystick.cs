using Fairwood.Math;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 武器摇杆，基于Gizmo
/// </summary>
public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum StateEnum
    {
        Idle,
        InvalidDragging,
        ValidDragging,
    }

    public StateEnum State = StateEnum.Idle;

    public Transform MainCameraTra;
    public Transform AssistPlane;
    public RectTransform TouchCircle;
    public RectTransform TouchSpot;
    public RectTransform DragDrop;

    public float WorldAimingDisplacementThreshold;
    public float ViewportDragToWorldRatio = 10f;

    public Vector2 PressPosition;//屏幕坐标
    public Vector2 CurrentPosition;//屏幕坐标
    public Vector3 WorldDragDisplacement;//世界拖拽位移（与屏幕拖拽距离1:1）
    public Vector3 WorldAimingDisplacement;//世界瞄准位移（拖拽位移*ratio，不考虑上下限）
    public Vector3 WorldActualDisplacement;//世界实际位移（瞄准位移，考虑上限，无下限）
    public bool IsValidDrag;
    private Unit _assistAimingTarget;
    public float AssistAimingWidth = 1;

    public Transform Walker;
    DirectionWalker _directionWalker;
    PathfindingWalker _pathfindingWalker;

    void Awake()
    {
        State = StateEnum.Idle;
        HideUI();
        enabled = true;

        _directionWalker = Walker.GetComponent<DirectionWalker>();
        _pathfindingWalker = Walker.GetComponent<PathfindingWalker>();
    }

    public void SwitchOnOff()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    void Update()
    {
        if (State == StateEnum.InvalidDragging || State == StateEnum.ValidDragging)
        {
            ResetAssistPlaneRotation();//坐标转换时使用的辅助平面，更新状态
            var dragDisplacement = CurrentPosition - PressPosition;//屏幕坐标
            WorldDragDisplacement = ScreenDragDisplacementToWorldDragDisplacement(dragDisplacement);//战场坐标
            WorldAimingDisplacement = WorldDragDisplacementToWorldAiming(WorldDragDisplacement);//战场坐标
            WorldActualDisplacement = WorldAimingDisplacementToWorldActual(WorldAimingDisplacement);
            var worldAimingMagnitude = WorldAimingDisplacement.magnitude;
            IsValidDrag = worldAimingMagnitude > WorldAimingDisplacementThreshold;
            if (IsValidDrag)//有效拖动
            {
                if (State == StateEnum.InvalidDragging)//状态切换
                {
                    State = StateEnum.ValidDragging;
                }
                RefreshJoystickUI(dragDisplacement);

                Walker.transform.forward = WorldAimingDisplacement;
                if (_directionWalker) _directionWalker.WalkTowards(WorldActualDisplacement);
                else _pathfindingWalker.WalkTo(Walker.transform.position + WorldActualDisplacement);
            }
            else
            {
                if (State == StateEnum.ValidDragging)//状态切换
                {
                    State = StateEnum.InvalidDragging;
                    HideUI();
                    if (_directionWalker) _directionWalker.Stop();
                    if (_pathfindingWalker) _pathfindingWalker.Stop();
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        State = StateEnum.InvalidDragging;
        PressPosition = eventData.pressPosition;
        CurrentPosition = eventData.position;
        Debug.LogFormat("CurrentPosition=" + CurrentPosition);
        var pos = PressPosition * 1800f / Screen.width;
        TouchCircle.localPosition = pos;
        RefreshJoystickUI(Vector2.zero);

        ShowUI();
    }

    void RefreshJoystickUI(Vector2 dragDisplacement)
    {
        TouchSpot.localPosition = dragDisplacement * (1800f / Screen.width);
        DragDrop.parent.right = dragDisplacement;
        DragDrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dragDisplacement.magnitude + 71 + 20);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
        ResetAssistPlaneRotation();

        if (_directionWalker) _directionWalker.Stop();
        if (_pathfindingWalker) _pathfindingWalker.Stop();

        State = StateEnum.Idle;
        HideUI();
    }

    public void OnDrag(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
    }

    void ResetAssistPlaneRotation()
    {
        AssistPlane.right = MainCameraTra.right;
    }

    Vector3 ScreenDragDisplacementToWorldDragDisplacement(Vector2 dragDisplacement)
    {
        //方案2
        var cam = MainCameraTra.GetComponent<Camera>();
        var screenStartPos = cam.WorldToScreenPoint(Walker.position.SetV3Y(0));

        var ray0 = cam.ScreenPointToRay(screenStartPos);
        var startPos = ray0.GetPoint(-ray0.origin.y / ray0.direction.y);
        var ray1 = cam.ScreenPointToRay(screenStartPos.ToVector2() + dragDisplacement);
        var endPos = ray1.GetPoint(-ray1.origin.y / ray1.direction.y);
        var worldDragDisplacement = (endPos - startPos).normalized * dragDisplacement.magnitude ;
        return worldDragDisplacement;
    }

    Vector3 WorldDragDisplacementToWorldAiming(Vector3 worldDragDisplacement)
    {
        return worldDragDisplacement/Screen.height*ViewportDragToWorldRatio;
    }
    Vector3 WorldAimingDisplacementToWorldActual(Vector3 worldAimingDisplacement)
    {
        return Vector3.ClampMagnitude(worldAimingDisplacement, 99999);
    }

    void ShowUI()
    {
        TouchCircle.gameObject.SetActive(true);
        TouchSpot.gameObject.SetActive(true);
        DragDrop.gameObject.SetActive(true);
    }
    void HideUI()
    {
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
    }
}
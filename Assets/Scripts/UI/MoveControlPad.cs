using System.Collections.Generic;
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 覆盖屏幕的走位控制器
/// </summary>
public class MoveControlPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum StateEnum
    {
        Idle,
        InvalidDragging,
        ValidDragging,
    }

    public StateEnum State = StateEnum.Idle;
    //public Vector2 StartPosition;
    public MainUI MainUI;

    public Transform MainCameraTra;
    public Transform AssistPlane;
    public RectTransform TouchCircle;
    public RectTransform TouchSpot;
    public RectTransform DragDrop;
    public GameObject OriginalDrop;

    public Vector2 PressPosition;
    public Vector2 CurrentPosition;

    private Unit _assistAimingTarget;
    public float AssistAimingWidth = 1;

    public int CurrentSkillID
    {
        get
        {
            var unitCurSkillID = MainController.Instance.FocusedUnit.Data.CurrentSkillID;
            return unitCurSkillID;
        }
    }
    //private const float UnitColliderDiameter = 1f;
    //private const int AssistAimingTestRayCount = 3;
    private readonly float[] AssistAimingTestRayOffsetList = {-0.5f, 0, 0.5f};

    void Awake()
    {
        enabled = false;
    }

    public void Init(Unit playerUnit)
    {
        State = StateEnum.Idle;
        MainController.Instance.Arrow.gameObject.SetActive(false);
        MainController.Instance.ThinArrow.gameObject.SetActive(false);
        MainController.Instance.FocusedUnit.UnitInfoCanvas.SprCostMP.gameObject.SetActive(false);
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        OriginalDrop.SetActive(true);
        enabled = true;
    }

    void Update()
    {
        if (!MainController.Instance.FocusedUnit || !MainController.Instance.FocusedUnit.Data.IsAlive) return;
        if (State == StateEnum.InvalidDragging || State == StateEnum.ValidDragging)
        {
            ResetAssistPlaneRotation();
            var dragDisplacement = CurrentPosition - PressPosition;
            var dragMagnitude = dragDisplacement.magnitude;
            if (dragMagnitude > MainController.Instance.DragThreshold)//有效拖动
            {
                var unit = MainController.Instance.FocusedUnit;
                if (State == StateEnum.InvalidDragging)
                {
                    State = StateEnum.ValidDragging;
                    MainController.Instance.Arrow.gameObject.SetActive(true);
                    MainController.Instance.Arrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                    MainController.Instance.FocusedUnit.UnitInfoCanvas.SprCostMP.gameObject.SetActive(true);
                    TouchCircle.gameObject.SetActive(true);
                    TouchSpot.gameObject.SetActive(true);
                    DragDrop.gameObject.SetActive(true);
                }

                var validDragDisplacement = CalcValidDragDisplacement(unit, dragDisplacement);

                TouchSpot.localPosition = validDragDisplacement * (1800f / Screen.width);
                DragDrop.parent.right = validDragDisplacement;
                DragDrop.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, validDragDisplacement.magnitude + 71 + MainController.Instance.DragThreshold + 20);

                var geodesicDisplacement = DragDisplacementToGeodesicDisplacement(validDragDisplacement);

                unit.UnitInfoCanvas.SprCostMP.anchorMin =
                    unit.UnitInfoCanvas.SprCostMP.anchorMin.SetV2X(1 - unit.GetSkillCostMP(CurrentSkillID, geodesicDisplacement.magnitude) / (unit.Data.mp + float.Epsilon));

                //辅助瞄准
                _assistAimingTarget = null;
                if (MainController.Instance.SkillAssistAimingList[CurrentSkillID])
                {
                    var mostMiddleDistance = float.MaxValue;
                    var right = Vector3.Cross(geodesicDisplacement, Vector3.up).normalized;
                    for (int i = 0; i < AssistAimingTestRayOffsetList.Length; i++)
                    {
                        var offset = AssistAimingTestRayOffsetList[i];
                        var castedHits = Physics.RaycastAll(unit.Position.SetV3Y(1) + offset * right, geodesicDisplacement,
                            geodesicDisplacement.magnitude,
                            LayerManager.Mask.Unit);
                        foreach (var hit in castedHits)
                        {
                            var castedUnit = hit.collider.GetComponent<Unit>();
                            castedUnit.MustNotBeNull();
                            castedUnit.MustNotBeEqual(unit);
                            var me2targetVector = castedUnit.Position - unit.Position;
                            var toMiddleLineDistance = Mathf.Abs(Vector3.Dot(me2targetVector, right));
                            if (toMiddleLineDistance < mostMiddleDistance)
                            {
                                _assistAimingTarget = castedUnit;
                                mostMiddleDistance = toMiddleLineDistance;
                            }
                        }
                    }
                }

                var arrow = MainController.Instance.Arrow;
                var img = arrow.GetComponent<Image>();
                img.color = img.color.SetAlpha(_assistAimingTarget ? 1f : 1);
                var eA = arrow.localEulerAngles;
                eA.z = Quaternion.FromToRotation(Vector3.right, geodesicDisplacement).eulerAngles.y;
                arrow.localEulerAngles = -eA;
                arrow.position = unit.transform.position.SetV3Y(MainController.Instance.SkillArrowHeightList[CurrentSkillID]);
                arrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    CurrentSkillID != 1
                        ? geodesicDisplacement.magnitude
                        : Mathf.Min(arrow.rect.width + 60*Time.deltaTime, geodesicDisplacement.magnitude));
                arrow.GetComponent<Image>().color = unit.GetSkillTotalCDRemaining(CurrentSkillID) <= 0 && unit.GetSkillCountEnough(CurrentSkillID) ? Color.blue : Color.grey;

                var thinArrow = MainController.Instance.ThinArrow;
                //thinArrow.gameObject.SetActive(_assistAimingTarget);
                if (_assistAimingTarget) //找到辅助瞄准的目标了
                {
                    var me2mostMiddleUnitVector = _assistAimingTarget.Position - unit.Position;
                    eA = thinArrow.localEulerAngles;
                    eA.z = Quaternion.FromToRotation(Vector3.right, me2mostMiddleUnitVector).eulerAngles.y;
                    thinArrow.localEulerAngles = -eA;
                    thinArrow.position =
                        unit.transform.position.SetV3Y(MainController.Instance.SkillArrowHeightList[CurrentSkillID]);
                    thinArrow.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, geodesicDisplacement.magnitude);
                }

                MainController.Instance.FocusedUnit.transform.forward = geodesicDisplacement;
            }
            else
            {
                if (State == StateEnum.ValidDragging)
                {
                    State = StateEnum.InvalidDragging;
                    MainController.Instance.Arrow.gameObject.SetActive(false);
                    MainController.Instance.ThinArrow.gameObject.SetActive(false);
                    MainController.Instance.FocusedUnit.UnitInfoCanvas.SprCostMP.gameObject.SetActive(false);
                    TouchCircle.gameObject.SetActive(false);
                    TouchSpot.gameObject.SetActive(false);
                    DragDrop.gameObject.SetActive(false);
                }

            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!MainController.Instance.FocusedUnit || !MainController.Instance.FocusedUnit.Data.IsAlive) return;
        State = StateEnum.InvalidDragging;
        PressPosition = eventData.pressPosition;
        CurrentPosition = eventData.position;
        if (MainController.Instance.ForceDragMode)
        {
            var unitOnScreenPos = MainCameraTra.GetComponent<Camera>()
                .WorldToScreenPoint(MainController.Instance.FocusedUnit.transform.position).ToVector2();
            PressPosition = CurrentPosition + (unitOnScreenPos - CurrentPosition).normalized*70;
        }

        var pos = PressPosition * 1800f/Screen.width;
        TouchCircle.localPosition = pos;
        TouchSpot.localPosition = Vector3.zero;
        OriginalDrop.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!MainController.Instance.FocusedUnit || !MainController.Instance.FocusedUnit.Data.IsAlive) return;
        CurrentPosition = eventData.position;
        ResetAssistPlaneRotation();
        if (State == StateEnum.ValidDragging)
        {
            var dragDisplacement = CurrentPosition - PressPosition;
            var dragMagnitude = dragDisplacement.magnitude;
            if (dragMagnitude > MainController.Instance.DragThreshold) //有效拖动
            {
                var unit = MainController.Instance.FocusedUnit;
                var validDragDisplacement = CalcValidDragDisplacement(unit, dragDisplacement);
                var geodesicDisplacement = DragDisplacementToGeodesicDisplacement(validDragDisplacement);
                Vector3 castSkillDisplacement;
                if (MainController.Instance.SkillAssistAimingList[CurrentSkillID] && _assistAimingTarget)
                {
                    castSkillDisplacement = (_assistAimingTarget.Position - unit.Position).normalized*
                                            geodesicDisplacement.magnitude;
                }
                else
                {
                    castSkillDisplacement = geodesicDisplacement;
                }
                MainController.Instance.FocusedUnit.CastSkill(castSkillDisplacement);
            }
        }

        State = StateEnum.Idle;
        MainController.Instance.Arrow.gameObject.SetActive(false);
        MainController.Instance.ThinArrow.gameObject.SetActive(false);
        MainController.Instance.FocusedUnit.UnitInfoCanvas.SprCostMP.gameObject.SetActive(false);
        TouchCircle.gameObject.SetActive(false);
        TouchSpot.gameObject.SetActive(false);
        DragDrop.gameObject.SetActive(false);
        OriginalDrop.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!MainController.Instance.FocusedUnit || !MainController.Instance.FocusedUnit.Data.IsAlive) return;
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
        var screenStartPos = cam.WorldToScreenPoint(MainController.Instance.FocusedUnit.Position.SetV3Y(0));

        var ray0 = cam.ScreenPointToRay(screenStartPos);
        var startPos = ray0.GetPoint(-ray0.origin.y/ray0.direction.y);
        //Debug.DrawRay(startPos, Vector3.right, Color.blue, 1);
        //Debug.DrawRay(startPos, Vector3.forward, Color.blue, 1);
        var ray1 = cam.ScreenPointToRay(screenStartPos.ToVector2() + dragDisplacement);
        var endPos = ray1.GetPoint(-ray1.origin.y / ray1.direction.y);
        //Debug.DrawRay(endPos, Vector3.right, Color.red, 1);
        //Debug.DrawRay(endPos, Vector3.forward, Color.red, 1);
        var geodesicDisplacement = (endPos - startPos).normalized*dragDisplacement.magnitude*
                                   MainController.Instance.MoveMode*
                                   MainController.Instance.FocusedUnit.SkillDragToDisplacementRatioList[CurrentSkillID];
        //Debug.DrawRay(startPos, geodesicDisplacement, Color.black, 1);

        //return AssistPlane.TransformDirection(new Vector3(dragDisplacement.x, 0, dragDisplacement.y)) *
        //                MainController.Instance.MoveMode * MainController.Instance.DragToDisplacementRatio; //方案1
        //var geodesicDisplacement = MainCameraTra.TransformDirection(dragDisplacement).SetV3Y(0).normalized*
        //                           dragDisplacement.magnitude*MainController.Instance.MoveMode*
        //                           MainController.Instance.DragToDisplacementRatio;//方案3
        return geodesicDisplacement;
    }

    Vector3 CalcValidDragDisplacement(Unit unit, Vector2 dragDisplacement)
    {
        var dragMagnitude = dragDisplacement.magnitude;
        var validDragMagnitude = dragMagnitude - MainController.Instance.DragThreshold;
        validDragMagnitude = Mathf.Clamp(validDragMagnitude, 0,
            MainController.Instance.SkillDragMaxLimitList[CurrentSkillID]);
        validDragMagnitude = Mathf.Clamp(validDragMagnitude, 0, unit.GetCurrentSkillMaxGeodesicDistance() / MainController.Instance.FocusedUnit.SkillDragToDisplacementRatioList[CurrentSkillID]);
        var validDragDisplacement = dragDisplacement.normalized * validDragMagnitude;
        return validDragDisplacement;
    }
}
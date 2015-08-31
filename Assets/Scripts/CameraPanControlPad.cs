using Fairwood.Math;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 覆盖屏幕的走位控制器
/// </summary>
public class CameraPanControlPad : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public MainUI MainUI;
    public int Mode = -1;
    public PathfindingWalker PlayerWalker;
    public Transform MainCameraTra;
    public Transform AssistPlane;
    public Transform Cursor;
    public Transform InSceneCursor;

    public Vector2 LastPosition;
    public Vector2 CurrentPosition;

    public void Init(Unit playerUnit)
    {
        PlayerWalker = playerUnit.Walker;
    }

    private void Update()
    {
        if (CurrentPosition == LastPosition) return;

        ResetAssistPlaneRotation();
        var dragDisplacement = CurrentPosition - LastPosition;
        var cameraDisplacement = Mode*dragDisplacement * 0.03f;
        MainCameraTra.Translate(cameraDisplacement.x, 0, cameraDisplacement.y, AssistPlane);

        
        var cam = MainCameraTra.GetComponent<Camera>();
        var ray0 = cam.ScreenPointToRay(Cursor.position);
        var destination = ray0.GetPoint(-ray0.origin.y/ray0.direction.y);
        if (InSceneCursor) InSceneCursor.position = destination.SetV3Y(0.1f);
        if (!PlayerWalker)
            PlayerWalker = MainController.Instance.FocusedUnit
                ? MainController.Instance.FocusedUnit.GetComponent<PathfindingWalker>()
                : null;
        if (PlayerWalker) PlayerWalker.WalkTo(destination);

        LastPosition = CurrentPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
        LastPosition = CurrentPosition;
    }
    public void OnDrag(PointerEventData eventData)
    {
        CurrentPosition = eventData.position;
    }

    void ResetAssistPlaneRotation()
    {
        AssistPlane.right = MainCameraTra.right;
    }

    public void SwitchMode()
    {
        Mode *= -1;
    }
}
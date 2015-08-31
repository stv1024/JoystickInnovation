using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 闪避按钮
/// </summary>
public class DodgeButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        MainController.Instance.FocusedUnit.Dodge();
    }
}
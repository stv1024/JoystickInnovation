using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Summary
/// </summary>
public class CellJumpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsPressed;


    [FormerlySerializedAs("onPress")]
    [SerializeField]
    private ButtonPressedEvent m_OnPress = new CellJumpButton.ButtonPressedEvent();

    public ButtonPressedEvent onPress
    {
        get
        {
            return this.m_OnPress;
        }
        set
        {
            this.m_OnPress = value;
        }
    }
    [Serializable]
    public class ButtonPressedEvent : UnityEvent
    {
    }

    void Update()
    {
        if (IsPressed)
        {
            m_OnPress.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsPressed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsPressed = false;
    }
}
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 主控制器
/// </summary>
public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    public MoveControlPad MoveControlPad;
    public AttackControlPad AttackControlPad;
    public CameraPanControlPad CameraPanControlPad;
    public Transform MainCameraTra;

    public static bool IsHost = false;

    public int MoveMode = 1;
    public bool MPRWhenDragging = true;
    /// <summary>
    /// TouchDown屏幕后，在另一位置设为PressPosition
    /// </summary>
    public bool ForceDragMode = false;

    public float DragThreshold = 60;
    //public float DragMaxLimit = 200f;
    //public float DragToDisplacementRatio = 0.2f;

    public float JumpingHeight = 1;
    public float JumpingTime = 0.5f;
    public static float JumpingGravity;

    public float CostMPForJumpingDistance = 5f;
    public float CostMPForSkill1 = 100f;
    public float CostMPForSkill2 = 100f;

    public float[] SkillArrowHeightList;
    public float[] SkillDragMaxLimitList;
    public bool[] SkillAssistAimingList;

    public float BombXZSpeed = 200;
    public float BombGravity = 20;

    public AnimationCurve OnDamagedRedFlashCurve;
    public float OnDamagedRedFlashDuraion = 0.5f;

    public Button[] BtnSkills;

    readonly Color _selectedColor = Color.green.SetAlpha(0.5f);
    readonly Color _unselectedColor = Color.white.SetAlpha(0.5f);

    void Awake()
    {
        Instance = this;
        JumpingGravity = -8*JumpingHeight/(JumpingTime*JumpingTime);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        IsHost = false;
    }

    public RectTransform Arrow;
    public RectTransform ThinArrow;
    public RectTransform Arrow2;

    public Unit FocusedUnit;


    private float _lastMoveRealTime = float.NegativeInfinity;
    public void SwitchSkill(int skillID)
    {
        if (FocusedUnit.Data.CurrentSkillID != skillID)
        {
            if (MoveControlPad.State != MoveControlPad.StateEnum.Idle)
            {
                MoveControlPad.PressPosition =
                    MoveControlPad.CurrentPosition;
            }
            FocusedUnit.CmdSwitchSkill(skillID);
        }
        for (int i = 0; i < BtnSkills.Length; i++)
        {
            var btn = BtnSkills[i];
            var colors = btn.colors;
            colors.normalColor = i == skillID ? _selectedColor : _unselectedColor;
            colors.highlightedColor = i == skillID ? _selectedColor : _unselectedColor;
            btn.colors = colors;
        }
        if (Time.realtimeSinceStartup - _lastMoveRealTime < 0.3f)
        {
            FocusedUnit.Dodge();
        }
        _lastMoveRealTime = Time.realtimeSinceStartup;
    }

    public void OnUnitDie(Unit unit, Unit killer)
    {
        if (killer == FocusedUnit && killer != unit) _mpBottleCount += 1;
        if (killer && killer != unit) killer.Data.KillCount += 1;
    }

    private int _mpBottleCount;
    public void GainMP()
    {
        if (_mpBottleCount <= 0) return;
        _mpBottleCount -= 1;
        FocusedUnit.Setmp(FocusedUnit.Data.MP);
    }
    public void OnAvatarClick()
    {
        Application.LoadLevel(0);
    }
}
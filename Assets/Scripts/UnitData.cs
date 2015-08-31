using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 单位属性
/// </summary>
public class UnitData : NetworkBehaviour
{

    [SyncVar]
    public int PlayerID = 0;
    [SyncVar]
    public int Camp = 0;
    [SyncVar]
    public bool IsPlayer;
    [SyncVar]
    public float hp = 1000;
    [SyncVar]
    public float HP = 1000;
    [SyncVar]
    public float HPR = 1f;
    [SyncVar]
    public float mp = 1000;
    [SyncVar]
    public float MP = 1000;
    [SyncVar]
    public float MPR = 200;
    [SyncVar]
    public float MPRbyMP = 0.2f;
    [SyncVar]
    public float ARM;
    [SyncVar]
    public bool Rebirthable;

    [SyncVar]
    public float Skill1TileSpeed = 100;

    [SyncVar]
    public int CurrentSkillID;

    [SyncVar]
    public int KillCount;

    [SyncVar]
    public float GlobalCD = 1;

    [SyncVar]
    public float _globalCDRemaining = 1;

    [SyncVar]
    public bool IsAlive = true;

    [SyncVar]
    public float JumpTime;


    private SyncListInt _skillAvailableCountList = new SyncListInt();

    public SyncListInt SkillAvailableCountList
    {
        get { return _skillAvailableCountList; }
    }

    private SyncListFloat _skillCDRemainingList = new SyncListFloat();
    public SyncListFloat SkillCDRemainingList { get { return _skillCDRemainingList; } }
}
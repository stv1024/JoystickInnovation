using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 加在玩家Unit Prefab上
/// </summary>
public class PlayerUnit : MonoBehaviour
{
    public Unit Owner;

    void Awake()
    {
        Debug.LogFormat("b:" + GetComponent<NetworkIdentity>().hasAuthority);
        if (!GetComponent<NetworkIdentity>().hasAuthority)
        {
            Destroy(this);
        }
    }
    void OnEnable()
    {
        Owner = GetComponent<Unit>();
        MainController.Instance.FocusedUnit = Owner;
        MainController.Instance.SwitchSkill(0);
        MainController.Instance.MoveControlPad.Init(Owner);
    }

    void OnStartClient()
    {
        Debug.LogFormat("OnStartClient");
    }
}
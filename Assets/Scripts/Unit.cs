using UnityEngine;
using System.Collections;
using Fairwood.Math;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class Unit : NetworkBehaviour
{
    public enum StateEnum
    {
        Static,
        Jumping,
    }

    public PathfindingWalker Walker;
    public UnitData Data;

    public UnitInfoCanvas UnitInfoCanvas;
    public Animator Animator;

    public Transform LaunchPoint;
    public GameObject ArrowPrefab;
    public GameObject BombPrefab;

    public StateEnum State = StateEnum.Static;
    public Vector3 DestinationPosition;


    public Vector3 Position
    {
        get { return transform.position.SetV3Y(0); }
    }

    public float[] SkillDragToDisplacementRatioList;


    public int[] SkillOriginalAvailableCountList = { 3, int.MaxValue, 1 };

    public float[] SkillCDList = {0f, 3f, 3f};

    private float _redFlashLeftTime;

    private Vector3 _rebirthPosition;

    public float RecoverJumpCountInterval = 3;
    private float _recoverJumpCountDown = 3;

    public override void OnStartServer()
    {
        Debug.LogFormat("OnStartServer {0}", Position);
        base.OnStartServer();
        MainController.IsHost = true;
        for (int i = 0; i < SkillCDList.Length; i++)
        {
            Data.SkillAvailableCountList.Add(SkillOriginalAvailableCountList[i]);
            Data.SkillCDRemainingList.Add(0);
        }
    }
    public override void OnStartClient()
    {
        Debug.LogFormat("OnStartClient {0}", Position);
        if (Data.Camp == 0 && !MainController.IsHost)
        {
            Destroy(GetComponent<AI>());
            enabled = false;
        }
    }

    void Awake()
    {
        var traUnitInfoCanvas = transform.FindChild("Canvas-Unit");
        if (traUnitInfoCanvas)
        {
            UnitInfoCanvas = traUnitInfoCanvas.GetComponent<UnitInfoCanvas>();
            if (UnitInfoCanvas != null) UnitInfoCanvas.Owner = this;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.LogFormat("Unit.OnStartLocalPlayer {0};{1} {2}", GetComponent<NetworkIdentity>().hasAuthority, GetComponent<NetworkIdentity>().isLocalPlayer, netId);
        if (isLocalPlayer)
        {
            MainController.Instance.FocusedUnit = this;
            MainController.Instance.SwitchSkill(0);
            MainController.Instance.MoveControlPad.Init(this);
            MainController.Instance.AttackControlPad.Init(this);
            MainController.Instance.CameraPanControlPad.Init(this);
            CmdRequestCampInfo();
        }

    }

    [Command]
    void CmdRequestCampInfo()
    {

    }

    [Server]
    public void ResetCampInfo(int playerID, int camp, Vector2 position)
    {
        Debug.LogFormat("Unit{2}.SResetCampInfo({0},{1})", camp, position, playerID);
        Data.PlayerID = playerID;
        Data.Camp = camp;
        RpcResetCampInfo(playerID, camp, position);
    }
    [ClientRpc]
    public void RpcResetCampInfo(int playerID, int camp, Vector2 position)
    {
        Debug.LogFormat("Unit{2}.RpcResetCampInfo({0},{1})", camp, position, playerID);
        name = "Player Unit " + playerID;
        if (isLocalPlayer)
        {
            transform.position = position.ToVector3(1.1f);
            _rebirthPosition = Position;
        }
    }

    [Server]
    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        if (Data.IsPlayer)
        {

        }
    }

    [Command]
    public void CmdSwitchSkill(int skillID)
    {
        Data.CurrentSkillID = skillID;
    }

    public void CastSkill(Vector3 displacement)
    {
        CastSkill(Data.CurrentSkillID, displacement);
    }
    public void CastSkill(int skillID, Vector3 displacement)
    {
        var costMP = GetSkillCostMP(skillID, displacement.magnitude);
        if (GetSkillTotalCDRemaining(skillID) > 0 || !GetSkillCountEnough(skillID))
        {
            return;
        }

        switch (skillID)
        {
            case 0://跳跃
                if (State == StateEnum.Static && Data.mp + 0.1f >= costMP)
                {
                    State = StateEnum.Jumping;
                    var jumpingTime = MainController.Instance.JumpingTime;
                    DestinationPosition = transform.localPosition + displacement;
                    Data.JumpTime = 0;
                    GetComponent<Rigidbody>().velocity = displacement/jumpingTime -
                                                         Vector3.up*0.5f*MainController.JumpingGravity*jumpingTime;
                    if (Animator)
                    {
                        Animator.SetTrigger("Jump");
                    }
                }
                break;
        }
        CmdCastAtkSkill(skillID, displacement);
    }
    [Command]
    public void CmdCastAtkSkill(int skillID, Vector3 displacement)
    {
        var costMP = GetSkillCostMP(skillID, displacement.magnitude);
        if (GetSkillTotalCDRemaining(skillID) > 0 || !GetSkillCountEnough(skillID))
        {
            return;
        }
        Data._globalCDRemaining = Data.GlobalCD;
        Data.SkillCDRemainingList[skillID] = SkillCDList[skillID];
        Data.SkillAvailableCountList[skillID]--;
        switch (skillID)
        {
            case 0://跳跃
                Setmp(Data.mp - costMP);
                break;
            case 1://弓箭
                if (Data.mp + float.Epsilon >= costMP)
                {
                    Setmp(Data.mp - costMP);
                    CmdCreateProjectile1(displacement);
                    if (Animator)
                    {
                        Animator.SetTrigger("CastSkill1");
                    }
                }
                break;
            case 2://炸弹
                if (Data.mp + float.Epsilon >= costMP)
                {
                    Setmp(Data.mp - costMP);
                    CmdCreateProjectile2(displacement);
                    if (Animator)
                    {
                        Animator.SetTrigger("CastSkill1");
                    }
                }
                break;
        }
    }

    public void WalkTowards(Vector2 direction)
    {
        
    }

    public void Dodge()
    {
        const float dodgeDistance = 3;
        var needDodge = false;
        var displacement = Vector3.zero;
        var cldrs = Physics.OverlapSphere(Position.SetV3Y(1), 16, LayerManager.Mask.Projectile);
        foreach (var cldr in cldrs)
        {
            var projectile = cldr.GetComponent<Projectile>();
            if (projectile)
            {
                var rgd = projectile.GetComponent<Rigidbody>();
                //RaycastHit hit;
                //var canHit = GetComponent<Collider>()
                //    .Raycast(new Ray(projectile.transform.position, rgd.velocity), out hit, 16);
                //if (canHit)
                //{
                var right = Vector3.Cross(Vector3.up, rgd.velocity.SetV3Y(0)).normalized;
                var projectile2UnitOnRight = Vector3.Dot((transform.position - projectile.transform.position).SetV3Y(0), right);

                displacement = projectile2UnitOnRight >= 0 ? right * dodgeDistance : -right * dodgeDistance;
                needDodge = true;
                break;
                //}
            }
        }
        if (!needDodge)
        {
            var ranV = Random.insideUnitCircle;
            displacement = new Vector3(ranV.x, 0, ranV.y).normalized*dodgeDistance;
        }
        Debug.DrawRay(transform.position.SetV3Y(1), displacement, Color.blue, 5);
        var oriSkillID = Data.CurrentSkillID;
        CmdSwitchSkill(0);
        Data._globalCDRemaining = 0;
        CastSkill(displacement);
        CmdSwitchSkill(oriSkillID);
    }

    public float GetSkillTotalCDRemaining(int skillID)
    {
        return Mathf.Max(Data.SkillCDRemainingList[skillID], Data._globalCDRemaining);
    }

    void Update()
    {
        if (Data.IsAlive)
        {
            Data._globalCDRemaining -= Time.deltaTime;
            for (int i = 0; i < Data.SkillCDRemainingList.Count; i++)
            {
                Data.SkillCDRemainingList[i] -= Time.deltaTime;
            }
            switch (State)
            {
                case StateEnum.Jumping:
                    Data.JumpTime += Time.deltaTime;
                    var destinationVector = DestinationPosition - transform.localPosition;
                    //Velocity = destinationVector.normalized * Speed;
                    //var stepVector = Velocity * Time.deltaTime;
                    if (Data.JumpTime >= MainController.Instance.JumpingTime)
                    {
                        //Velocity = Vector2.zero;
                        //transform.localPosition = DestinationPosition.SetV3Y(1.001f);
                        State = StateEnum.Static;
                    }
                    else
                    {
                        //transform.localPosition += stepVector;
                        //transform.localPosition =
                        //    transform.localPosition.SetV3Y(1.001f + MainController.Instance.JumpingHeight + 
                        //                                   0.5f*MainController.JumpingGravity*
                        //                                   Mathf.Pow(JumpTime - MainController.Instance.JumpingTime/2f, 2));
                    }
                    break;
                case StateEnum.Static:
                    break;
            }


            Sethp(Data.hp + Data.HPR * Time.deltaTime);
            if (MainController.Instance.MPRWhenDragging ||
                MainController.Instance.MoveControlPad.State != MoveControlPad.StateEnum.ValidDragging)
            {
                Setmp(Data.mp + (Data.MPR + Data.MPRbyMP * Data.mp) * Time.deltaTime);
            }

            if (Data.SkillAvailableCountList[0] >= 3)
            {
                _recoverJumpCountDown = RecoverJumpCountInterval;
            }
            else
            {
                _recoverJumpCountDown -= Time.deltaTime;
                if (_recoverJumpCountDown <= 0 && Data.SkillAvailableCountList[0] < 3)
                {
                    Data.SkillAvailableCountList[0] += 1;
                    _recoverJumpCountDown = RecoverJumpCountInterval;
                }
            }

            if (_redFlashLeftTime > 0)
            {
                _redFlashLeftTime -= Time.deltaTime;

                var color = (Color.white*
                             MainController.Instance.OnDamagedRedFlashCurve.Evaluate(1 -
                                                                                     _redFlashLeftTime/
                                                                                     MainController.Instance
                                                                                         .OnDamagedRedFlashDuraion));
                color.r = 1;
                GetComponent<Renderer>().material.color = color;
            }
        }

        if (UnitInfoCanvas)
        {
            UnitInfoCanvas.transform.rotation = MainController.Instance.MainCameraTra.rotation;
        }
    }

    public void PushBack(float force, Vector3 explodePosition, float explodeRadius)
    {
        var rgd = GetComponent<Rigidbody>();
        rgd.AddExplosionForce(force, explodePosition, explodeRadius);
        rgd.AddForce(Vector3.up/((explodePosition - transform.position).magnitude + 1) * 2000);
    }
    public float TakeDamage(Unit caster, float power)
    {
        var dmg = power / (1 + 0.05f * Data.ARM);
        Sethp(Data.hp - dmg);
        if (Data.hp <= 0)
        {
            Die(caster);
            return dmg;
        }
        _redFlashLeftTime = MainController.Instance.OnDamagedRedFlashDuraion;

        return dmg;
    }

    public void Sethp(float newhp)
    {
        Data.hp = Mathf.Clamp(newhp, 0, Data.HP);
    }
    public void Setmp(float newmp)
    {
        Data.mp = Mathf.Clamp(newmp, 0, Data.MP);
    }

    [Server]
    public void Die(Unit killer)
    {
        if (!Data.IsAlive) return;
        Data.IsAlive = false;

        var rgd = GetComponent<Rigidbody>();
        if (rgd)
        {
            rgd.constraints = RigidbodyConstraints.None;
        }

        if (Data.Rebirthable)
        {
            UnitInfoCanvas.gameObject.SetActive(false);
            CoroutineManager.StartCoroutine(new CoroutineManager.Coroutine(3, Rebirth));
        }
        else
        {
            Destroy(UnitInfoCanvas.gameObject);
            Destroy(gameObject, 3);
        }

        MainController.Instance.OnUnitDie(this, killer);
    }

    [Command]
    public void CmdCreateProjectile1(Vector3 displacement)
    {
        var ratio = 0.9f;

        //var go = Network.Instantiate(ArrowPrefab, Vector3.zero, Quaternion.identity, 0);
        var go = PrefabHelper.InstantiateAndReset(ArrowPrefab, null);
        go.transform.position = LaunchPoint.position;
        go.transform.forward = displacement;
        var rigid = go.GetComponent<Rigidbody>();
        var projectile = go.GetComponent<Projectile>();
        var actualDisplacement = displacement.normalized*
                                 Mathf.Max(0, displacement.magnitude - LaunchPoint.localPosition.z);
        rigid.velocity = actualDisplacement / projectile.Lifespan * ratio;
        projectile.Launcher = this;

        NetworkServer.Spawn(go);
    }

    [Command]
    public void CmdCreateProjectile2(Vector3 displacement)
    {
        var go = PrefabHelper.InstantiateAndReset(BombPrefab, null);
        go.transform.position = transform.TransformPoint(new Vector3(0, 1.1f, 0));
        go.transform.forward = displacement;
        var rigid = go.GetComponent<Rigidbody>();
        var ratio = 0.9f;
        var xzSpeed = MainController.Instance.BombXZSpeed;
        var t = displacement.magnitude/xzSpeed;
        var vy = MainController.Instance.BombGravity*0.5f*t;
        rigid.velocity = (displacement.normalized*xzSpeed + Vector3.up*vy)*ratio;
        var projectile = go.GetComponent<Projectile>();
        projectile.Launcher = this;
        //projectile.Lifespan = t;
        go.GetComponent<ConstantForce>().force = Vector3.down*rigid.mass*MainController.Instance.BombGravity;

        NetworkServer.Spawn(go);
    }

    public float GetSkillCostMPInfo(int skillID)
    {
        switch (skillID)
        {
            case 0:
                return MainController.Instance.CostMPForJumpingDistance;
            case 1:
                return MainController.Instance.CostMPForSkill1;
            case 2:
                return MainController.Instance.CostMPForSkill2;
        }
        return 0;
    }
    public float GetSkillCostMP(int skillID, float displacement)
    {
        if (skillID == 0) return displacement * GetSkillCostMPInfo(skillID);
        return GetSkillCostMPInfo(skillID);
    }

    public float GetCurrentSkillMaxGeodesicDistance()
    {
        return GetSkillMaxGeodesicDistance(Data.CurrentSkillID);
    }
    public float GetSkillMaxGeodesicDistance(int skillID)
    {
        if (skillID == 0) return Data.mp / GetSkillCostMPInfo(0);
        return Mathf.Infinity;
    }

    public bool GetSkillCountEnough(int skillID)
    {
        return Data.SkillAvailableCountList[skillID] > 0;
    }

    [Server]
    public void Rebirth()
    {
        Data.IsAlive = true;
        Sethp(Data.HP);
        Setmp(Data.MP);
        UnitInfoCanvas.gameObject.SetActive(true);
        RpcRebirth();

        CmdSwitchSkill(0);
    }

    [ClientRpc]
    public void RpcRebirth()
    {
        transform.position = _rebirthPosition.AddV3Y(1.01f);
        transform.rotation = Quaternion.identity;
        var rgd = GetComponent<Rigidbody>();
        if (rgd)
        {
            rgd.velocity = Vector3.zero;
            rgd.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}

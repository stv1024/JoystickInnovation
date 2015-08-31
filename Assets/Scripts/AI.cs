using System.Collections.Generic;
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// AI
/// </summary>
public class AI : MonoBehaviour
{
    public Unit Owner;

    private float _maxDisplacement;
    public float RandomJumpRatio = 0.1f;
    public float GlobalCD = 3;

    public float _globalCD = 3;

    void Start()
    {
        _maxDisplacement = MainController.Instance.SkillDragMaxLimitList[0] * Owner.SkillDragToDisplacementRatioList[0];
    }

    private void Update()
    {
        if (!Owner) return;
        if (!Owner.Data.IsAlive) return;
        if (!Owner.isServer) return;

        _globalCD -= Time.deltaTime;

        var cldrs = Physics.OverlapSphere(Owner.Position, _maxDisplacement*1.7f);
        Unit target = null;
        var minDistance = float.PositiveInfinity;
        var toTargetVector = Vector3.zero;
        foreach (var cldr in cldrs)
        {
            var unit = cldr.GetComponent<Unit>();
            if (unit && unit.Data.Camp != Owner.Data.Camp)
            {
                var toCurUnitVector = (unit.Position - Owner.Position).SetV3Y(0);
                var hasBlock = Physics.Raycast(Owner.Position.SetV3Y(1.3f), toCurUnitVector, toCurUnitVector.magnitude, LayerManager.Mask.Ground);
                if (!hasBlock && toCurUnitVector.magnitude < minDistance)
                {
                    target = unit;
                    minDistance = toCurUnitVector.magnitude;
                    toTargetVector = toCurUnitVector;
                }
            }
        }

        //Debug.DrawRay(Owner.Position.SetV3Y(1.3f), toTargetVector, Color.blue);
        if (null == target)
        {
            if (Random.value < 0.02f && Owner.Data.mp > Owner.Data.MP * 0.7f)
            {
                Owner.CmdSwitchSkill(0);
                var displacement = (Random.onUnitSphere * _maxDisplacement * RandomJumpRatio).SetV3Y(0);
                displacement = displacement.normalized * Mathf.Clamp(displacement.magnitude, 0, _maxDisplacement);
                Owner.CastSkill(displacement);
            }
        }
        else if (toTargetVector.magnitude > _maxDisplacement)
        {
            if (Owner.Data.mp > Owner.Data.MP * 0.4f)
            {
                Owner.CmdSwitchSkill(0);
                var displacement = target.Position + (Random.onUnitSphere*_maxDisplacement*0.6f).SetV3Y(0) -
                                   Owner.Position;
                displacement = displacement.normalized*Mathf.Clamp(displacement.magnitude, 0, _maxDisplacement);
                Owner.CastSkill(displacement);
            }
        }
        else
        {
            if (_globalCD <= 0)
            {
                _globalCD = GlobalCD;
                var displacement = target.Position + (Random.onUnitSphere*0.3f).SetV3Y(0) - Owner.Position;
                var skillID = Random.value < 0.95f ? 1 : 2;
                Owner.CmdSwitchSkill(skillID);
                if (skillID == 1) displacement = displacement.normalized*_maxDisplacement;
                Owner.CastSkill(displacement);
            }
        }
    }
}
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 6向离散跳跃模块
/// </summary>
public class CellJumper : MonoBehaviour
{
    public IntVector2 IJ;
    public IntVector2 StartIJ;
    public IntVector2 DestinationIJ;

    public enum StateEnum
    {
        Idle,
        Jumping
    }

    public StateEnum State;
    public float JumpingDuration = 0.6f;
    const float BasicHeight = 1f;
    const float JumpingHeight = 0.5f;
    public AnimationCurve JumpingCurve;

    public Animator Animator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directionID">0-Right;1-ForwardRight;...</param>
    public void JumpToward(int directionID)
    {
        if (State != StateEnum.Idle) return;

        var deltaIJ = CellularMap.DirectionIDToDeltaIJ(directionID);
        var destinationIJ = IJ + deltaIJ;
        if (CellularMap.Instance[destinationIJ])
        {
            State = StateEnum.Jumping;
            _jumpingTime = 0f;
            StartIJ = IJ;
            DestinationIJ = destinationIJ;
            transform.forward = CellularMap.DeltaIJToWorldDirection(deltaIJ);

            if (Animator)
            {
                Animator.SetTrigger("Jump");
                Animator.SetFloat("JumpingSpeed", 1.333f*(27f/40f)/JumpingDuration);
            }
        }
    }

    private float _jumpingTime;
    void Update()
    {
        switch (State)
        {
            case StateEnum.Idle:
                break;
            case StateEnum.Jumping:
                _jumpingTime += Time.deltaTime;
                var f = Mathf.Clamp01(_jumpingTime/JumpingDuration);
                var h = JumpingCurve.Evaluate(f)*JumpingHeight;
                transform.position = Vector3.Lerp(CellularMap.Instance[StartIJ].transform.position,
                    CellularMap.Instance[DestinationIJ].transform.position, f).SetV3Y(BasicHeight + JumpingHeight*h);
                if (f >= 0.5f) IJ = DestinationIJ;
                if (f >= 1f)
                {
                    State = StateEnum.Idle;
                }
                break;
        }
    }

}
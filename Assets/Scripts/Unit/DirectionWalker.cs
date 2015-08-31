using System;
using UnityEngine;

/// <summary>
/// 摇杆方向走路者
/// </summary>
public class DirectionWalker : MonoBehaviour
{
    public enum StateEnum
    {
        Idle,
        Running
    }

    public StateEnum State;
    private Animator _animator;
    private Rigidbody _rigidbody;

    public float Speed = 7;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    public void WalkTowards(Vector3 direction)
    {
        var velocity = direction.normalized * Speed;
        _rigidbody.velocity = velocity;
        transform.forward = velocity;
        if (State != StateEnum.Running)
        {
            State = StateEnum.Running;
            _animator.SetBool("Running", true);
        }
    }

    public void Stop()
    {
        _rigidbody.velocity = Vector3.zero;
        if (State != StateEnum.Idle)
        {
            State = StateEnum.Idle;
            _animator.SetBool("Running", false);
        }
    }
}
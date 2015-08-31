using UnityEngine;

/// <summary>
/// 准星跟随走路模块
/// </summary>
public class PathfindingWalker : MonoBehaviour
{
    public enum StateEnum
    {
        Idle,
        Running
    }
    public StateEnum State;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void WalkTo(Vector3 position)
    {
        _navMeshAgent.Resume();
        _navMeshAgent.SetDestination(position);
        if (State != StateEnum.Running)
        {
            State = StateEnum.Running;
            _animator.SetBool("Running", true);
        }
    }
    public void Stop()
    {
        _navMeshAgent.Stop();
        if (State != StateEnum.Idle)
        {
            State = StateEnum.Idle;
            _animator.SetBool("Running", false);
        }
    }
}
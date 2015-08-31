using UnityEngine;

/// <summary>
/// 准星跟随走路模块
/// </summary>
public class PathfindingWalker : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void WalkTo(Vector3 position)
    {
        _navMeshAgent.Resume();
        _navMeshAgent.SetDestination(position);
    }
    public void Stop()
    {
       _navMeshAgent.Stop();
    }
}
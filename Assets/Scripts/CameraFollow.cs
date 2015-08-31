using Fairwood.Math;
using UnityEngine;

/// <summary>
/// Summary
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Vector3 Offset;

    public float FollowSpeed = 0.1f;

    public Transform Target;

    void Awake()
    {
        Offset = transform.position;
    }

    void Update()
    {
        if (Target != null)
            transform.position = Vector3.Lerp(transform.position, Target.position.SetV3Y(0) + Offset, FollowSpeed);
    }
}
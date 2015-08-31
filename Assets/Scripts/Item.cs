using UnityEngine;

/// <summary>
/// 道具
/// </summary>
public class Item : MonoBehaviour
{
    public ItemRespawner Respawner;

    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime*180, Space.World);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerManager.ID.Unit)
        {
            Destroy(gameObject);
            Respawner.OnItemDestroy();
        }
    }
}
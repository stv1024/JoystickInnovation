using Fairwood.Math;
using UnityEngine;

/// <summary>
/// 道具生成器
/// </summary>
public class ItemRespawner : MonoBehaviour
{
    public enum MapModeEnum
    {
        Continuous,
        Hexagon
    }

    public MapModeEnum MapMode;
    public GameObject ItemPrefab;
    public int MaintainAmount = 5;
    public Rect RangeToRespawn;
    int _currentAmount;

    public void OnItemDestroy()
    {
        _currentAmount--;
    }

    void Update()
    {
        while (_currentAmount < MaintainAmount)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        _currentAmount++;

        var x = Random.Range(RangeToRespawn.xMin, RangeToRespawn.xMax);
        var z = Random.Range(RangeToRespawn.yMin, RangeToRespawn.yMax);

        var item = PrefabHelper.InstantiateAndReset(ItemPrefab, null);
        item.GetComponent<Item>().Respawner = this;
        item.transform.position = new Vector3(x, 1, z);
    }
}
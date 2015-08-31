using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UnitInfo的Canvas
/// </summary>
public class UnitInfoCanvas : MonoBehaviour
{
    public Slider HPSlider;
    public Slider MPSlider;
    public RectTransform SprCostMP;
    public Unit Owner;

    void Update()
    {
        if (Owner)
        {
            if (HPSlider) HPSlider.value = Owner.Data.hp / Owner.Data.HP;
            if (MPSlider) MPSlider.value = Owner.Data.mp / Owner.Data.MP;
        }
    }
}
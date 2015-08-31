using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI父物体
/// </summary>
public class MainUI : MonoBehaviour
{
    public static MainUI Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public Slider HPSlider;
    public Slider MPSlider;

    public Text TxtDKA;
    public Text[] TxtSkillAvailableCountList;

    public Image[] ImgSkillCDList;

    void Update()
    {
        var focusedUnit = MainController.Instance.FocusedUnit;
        if (focusedUnit)
        {
            HPSlider.value = focusedUnit.Data.hp / focusedUnit.Data.HP;
            MPSlider.value = focusedUnit.Data.mp / focusedUnit.Data.MP;
            TxtDKA.text = string.Format("{0}/0/0", focusedUnit.Data.KillCount);

            for (int i = 0; i < ImgSkillCDList.Length; i++)
            {
                if (TxtSkillAvailableCountList[i])
                    TxtSkillAvailableCountList[i].text = string.Format("{0}", focusedUnit.Data.SkillAvailableCountList[i]);
                var image = ImgSkillCDList[i];
                image.fillAmount = Mathf.Max(focusedUnit.Data._globalCDRemaining, focusedUnit.Data.SkillCDRemainingList[i]) /
                                   Mathf.Max(focusedUnit.Data.GlobalCD, focusedUnit.SkillCDList[i]);
            }
        }
    }
}
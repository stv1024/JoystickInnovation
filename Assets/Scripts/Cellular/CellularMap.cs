using System.Collections.Generic;
using Fairwood.Math;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 蜂窝地图
/// </summary>
public class CellularMap : MonoBehaviour
{
    public static CellularMap Instance;

    public const int CellSize = 32;
    private const float _Offset = 2.55f;
    public static Vector2 VI = Vector3.right * _Offset;
    public static Vector2 VJ = new Vector3(_Offset * Mathf.Cos(Mathf.PI / 3f), _Offset * Mathf.Sin(Mathf.PI / 3f));
    public GameObject HexagonTemplate;
    //public Transform CenterCell;
    //public Transform ContigeousCell;

    public GameObject[][] Cells = new GameObject[CellSize][];

    public GameObject this[IntVector2 ij]
    {
        get
        {
            if (ij.i >= 0 && Cells.Length > ij.i && ij.i >= 0 && Cells[ij.i].Length > ij.j)
            {
                return Cells[ij.i][ij.j];
            }
            return null;
        }
        set { Cells[ij.i][ij.j] = value; }
    }
    void Awake()
    {
        Instance = this;

        for (int i = 0; i < CellSize; i++)
        {
            Cells[i] = new GameObject[CellSize];
            for (int j = 0; j < CellSize; j++)
            {
                var cell = PrefabHelper.InstantiateAndReset(HexagonTemplate, transform);
                cell.transform.localPosition = VI*(i - CellSize/2) + VJ*(j - CellSize/2);
                cell.name = "Cell " + i + "," + j;
                Cells[i][j] = cell;
            }
        }

        HexagonTemplate.SetActive(false);
    }
    public static IntVector2 DirectionIDToDeltaIJ(int directionID)
    {
        switch (directionID)
        {
            case 0:
                return new IntVector2(1, 0);
            case 1:
                return new IntVector2(0, 1);
            case 2:
                return new IntVector2(-1, 1);
            case 3:
                return new IntVector2(-1, 0);
            case 4:
                return new IntVector2(0, -1);
            case 5:
                return new IntVector2(1, -1);
            default:
                Assert.IsTrue(false);
                return IntVector2.zero;
        }
    }

    public static Vector3 DeltaIJToWorldDirection(IntVector2 deltaIJ)
    {
        var v2 = VI*deltaIJ.i + VJ*deltaIJ.j;
        return new Vector3(v2.x, 0, v2.y);
    }
}
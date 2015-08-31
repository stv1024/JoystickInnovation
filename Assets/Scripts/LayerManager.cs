using UnityEngine;

/// <summary>
/// Layer管理器
/// </summary>
public static class LayerManager
{
    public static class ID
    {
        public const int Projectile = 8;
        public const int Unit = 9;
        public const int Ground = 10;
    }
    public static class Mask
    {
        public const int Projectile = 1 << ID.Projectile;
        public const int Unit = 1<<ID.Unit;
        public const int Ground = 1 << ID.Ground;
    }
}
using UnityEngine;

namespace Rimbara.World
{
    // Original RIMBARA river layout. Shared by authoring and route diagnostics;
    // it contains no vendor-demo placements and never mutates scene objects.
    public static class RecoveryRiverLayout
    {
        public const int TrashCount = 10;
        public const float StartZ = 18f;
        public const float EndZ = 45f;
        public const float HalfWidth = 1.65f;
        public const float WaterY = 0.04f;
        public const float BridgeZ = 43f;
        public static float CenterX(float z) => 0.35f + Mathf.Sin((z - StartZ) / (EndZ - StartZ) * Mathf.PI * 2f) * 1.05f;
        public static Vector3 BankPoint(float z, int side, float distance = 3.5f) => new Vector3(CenterX(z) + side * distance, 0f, z);
        public static Vector3 TrashPosition(int index)
        {
            int row = index < 5 ? index : 9 - index;
            return BankPoint(20f + row * 5f, index < 5 ? -1 : 1, 3.15f);
        }
    }
}

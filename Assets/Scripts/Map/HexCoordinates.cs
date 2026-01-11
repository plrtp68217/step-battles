using Assets.Scripts.Map.Metric;
using UnityEngine;

namespace Assets.Scripts.Map
{
    [System.Serializable]
    public struct HexCoordinates
    {
        [field: SerializeField]
        public int X { get; private set; }
        [field: SerializeField]
        public int Z { get; private set; }
        public readonly int Y => -X - Z;

        public HexCoordinates(int x, int y)
        {
            X = x;
            Z = y;
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new(x - z / 2, z);
        }

        public static HexCoordinates FromPosition(Vector3 pos)
        {
            // Я тут не вникал
            float x = pos.x / (HexMetrics.INNER_RADIUS * 2f);
            float y = -x;
            float offset = pos.z / (HexMetrics.INNER_RADIUS * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new(iX, iZ);
        }

        public override readonly string ToString() => $"({X},{Y},{Z})";
        public readonly string ToStringOnSeparateLines() => $"{X}\n{Y}\n{Z}";


    }
}

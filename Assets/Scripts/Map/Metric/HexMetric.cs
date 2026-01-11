using UnityEngine;

namespace Assets.Scripts.Map.Metric
{
    public static class HexMetrics
    {
        // Чанки
        // 5 на 5 клеток
        public const int CHUNK_SIZE_X = 5;
        public const int CHUNK_SIZE_Z = 5;

        // Треугольники
        public const float SOLID_FACTOR = 0.8f;
        public const float BLEND_FACTOR = 1f - SOLID_FACTOR;

        public const float OUTER_RADIUS = 10f;
        public const float INNER_RADIUS = OUTER_RADIUS * 0.866025404f;

        public static Vector3[] Corners = {
            new(0f, 0f, OUTER_RADIUS),
            new(INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
            new(INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
            new(0f, 0f, -OUTER_RADIUS),
            new(-INNER_RADIUS, 0f, -0.5f * OUTER_RADIUS),
            new(-INNER_RADIUS, 0f, 0.5f * OUTER_RADIUS),
            // Повторяем первый для соединения последнего треугольника
            new(0f, 0f, OUTER_RADIUS),
        };


        // Шум
        public const float CELL_PERTURB_STRENGTH = 4f;
        public const float ELEVATION_PERTURB_STRENGTH = 1.5f;
        public const float NOISE_SCALE = 0.003f;

        public static Texture2D NoiseSource;
        public static Vector4 SampleNoise(Vector3 position)
        {
            if (NoiseSource== null)
                return Vector4.zero;

            return NoiseSource.GetPixelBilinear(
                position.x * NOISE_SCALE,
                position.z * NOISE_SCALE
            );
        }


        // Высоты
        public const float ELEVATION_STEP = 3f;
        public const int TERRACES_PER_SLOPE = 2;
        public const int TERRACE_STEP = TERRACES_PER_SLOPE * 2 + 1;
        public const float HORRIZONTAL_TERRACE_STEP_SIZE = 1f / TERRACE_STEP;
        public const float VERTICAL_TERRACE_STEP_SIZE = 1f / (TERRACES_PER_SLOPE + 1);

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * HORRIZONTAL_TERRACE_STEP_SIZE;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            float v = ((step + 1) / 2) * VERTICAL_TERRACE_STEP_SIZE;
            a.y += (b.y - a.y) * v;
            return a;
        }
        public static Color TerraceLerp(Color a, Color b, int step)
            => Color.Lerp(a, b, step * HORRIZONTAL_TERRACE_STEP_SIZE);

    
        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            if (elevation1 == elevation2)
            {
                return HexEdgeType.Flat;
            }
            int delta = elevation2 - elevation1;
            if (delta == 1 || delta == -1)
            {
                return HexEdgeType.Slope;
            }
            return HexEdgeType.Cliff;
        }


        public static Vector3 GetFirstSolidCorner(HexDirection direction)
             => Corners[(int)direction] * SOLID_FACTOR;

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
            => Corners[(int)direction + 1] * SOLID_FACTOR;

        public static Vector3 GetBridge(HexDirection direction)
            => (Corners[(int)direction] + Corners[(int)direction + 1]) * BLEND_FACTOR;


        public static Vector3 GetFirstCorner(HexDirection direction)
            => Corners[(int)direction];

        public static Vector3 GetSecondCorner(HexDirection direction)
            => Corners[(int)direction + 1];

        public static HexDirection Previous(this HexDirection direction)
            => direction == HexDirection.NE ? HexDirection.NW : (direction - 1);

        public static HexDirection Next(this HexDirection direction)
            => direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}

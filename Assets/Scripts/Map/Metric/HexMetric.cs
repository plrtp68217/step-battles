using UnityEngine;

namespace Assets.Scripts.Map.Metric
{
    public static class HexMetrics
    {
        public const float OuterRadius = 10f;

        public const float InnerRadius = OuterRadius * 0.866025404f;

        public static Vector3[] Corners = {
            new(0f, 0f, OuterRadius),
            new(InnerRadius, 0f, 0.5f * OuterRadius),
            new(InnerRadius, 0f, -0.5f * OuterRadius),
            new(0f, 0f, -OuterRadius),
            new(-InnerRadius, 0f, -0.5f * OuterRadius),
            new(-InnerRadius, 0f, 0.5f * OuterRadius),
            // Повторяем первый для соединения последнего треугольника
            new(0f, 0f, OuterRadius),
        };
    }
}

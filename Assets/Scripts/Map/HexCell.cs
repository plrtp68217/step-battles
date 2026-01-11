using Assets.Scripts.Map.Metric;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public sealed class HexCell : MonoBehaviour
    {

        [field: SerializeField]
        public HexCoordinates Coordinates { get; set; }

        private int _elevation;
        public int Elevation
        {
            get => _elevation;
            set
            {
                _elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.ELEVATION_STEP;

                // Добавляем шум к высоте ячейки
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                              HexMetrics.ELEVATION_PERTURB_STRENGTH;

                transform.localPosition = position;

                // Проверяем, что UI Rect уже назначен
                if (UIRect != null)
                {
                    Vector3 uiPosition = UIRect.localPosition;
                    uiPosition.z = -position.y;
                    UIRect.localPosition = uiPosition;
                }
            }
        }

        public Vector3 Position { get => transform.localPosition; }
        public Color Color { get; set; }
        public RectTransform UIRect { get; set; }

        [SerializeField]
        private HexCell[] _neighbors;
        public HexCell GetNeighbor(HexDirection direction)
            => _neighbors[(int)direction];

        public HexEdgeType GetEdgeType(HexDirection direction)
            => HexMetrics.GetEdgeType(_elevation, _neighbors[(int)direction]._elevation);
        public HexEdgeType GetEdgeType(HexCell otherCell)
            => HexMetrics.GetEdgeType(_elevation, otherCell._elevation);
        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            _neighbors[(int)direction] = cell;
            cell._neighbors[(int)direction.Opposite()] = this;
        }
    }
}


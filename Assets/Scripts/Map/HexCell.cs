using Assets.Scripts.Map.Metric;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Map
{
    public sealed class HexCell : MonoBehaviour
    {
        [field: SerializeField]
        public HexCoordinates Coordinates { get; set; }

        public HexGridChunk Chunk { get; set; } 

        private int _elevation = int.MinValue;
        public int Elevation
        {
            get => _elevation;
            set
            {
                if (_elevation == value)
                    return;

                _elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.ELEVATION_STEP;

                // Добавляем шум к высоте ячейки
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                              HexMetrics.ELEVATION_PERTURB_STRENGTH;

                transform.localPosition = position;

                if (UIRect != null)
                {
                    Vector3 uiPosition = UIRect.localPosition;
                    uiPosition.z = -position.y;
                    UIRect.localPosition = uiPosition;
                }

                Refresh();
            }
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value) // Проверяем, изменился ли цвет
                    return;

                _color = value;
                Refresh(); // Вызываем обновление
            }
        }

        public Vector3 Position { get => transform.localPosition; }
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

        private void Refresh()
        {
            if (Chunk != null)
            {
                Chunk.Refresh(); // Обновляем текущий чанк

                // Обновляем соседние чанки, если нужно
                for (int i = 0; i < _neighbors.Length; i++)
                {
                    HexCell neighbor = _neighbors[i];
                    if (neighbor != null && neighbor.Chunk != Chunk)
                    {
                        neighbor.Chunk.Refresh();
                    }
                }
            }
        }
    }
}
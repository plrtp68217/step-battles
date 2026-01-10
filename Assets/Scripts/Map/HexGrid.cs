using Assets.Scripts.Map.Metric;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.Scripts.Map
{
    public sealed class HexGrid : MonoBehaviour
    {
        [field: SerializeField]
        public Text LabelPrefab { get; private set; }



        [field: SerializeField]
        public int Width { get; private set; } = 6;

        [field: SerializeField]
        public int Height { get; private set; } = 6;

        [field: SerializeField]
        public HexCell CellPrefab { get; private set; }

        private HexCell[] _cells;
        private HexMesh _hexMesh;
        private Canvas _gridCanvas;

        private Color _defaultColor = Color.white;
        private Color _touchedColor = Color.green;

        private void Awake()
        {
            _gridCanvas = GetComponentInChildren<Canvas>();
            _hexMesh = GetComponentInChildren<HexMesh>();

            _cells = new HexCell[Width * Height];
            for (int z = 0, i = 0; z < Height; z++)
                for (int x = 0; x < Width; x++)
                    CreateCell(x, z, i++);
        }

        private void Start()
        {
            _hexMesh.Triangulate(_cells);
        }

        public HexCell GetCellAt(Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
            int index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;
            return _cells[index];
        }
        public void Refresh() 
        {
            _hexMesh.Triangulate(_cells);
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 pos;
            // z / 2 дает смещение каждой 2 строки
            pos.x = (x + z * 0.5f - z / 2) * Metric.HexMetrics.InnerRadius * 2f;
            pos.y = 0;
            pos.z = z * Metric.HexMetrics.OuterRadius * 1.5f;

            HexCell cell = Instantiate(CellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = pos;

            cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            cell.Color = _defaultColor;

            // Заполнение соседей
            if (x > 0)
                cell.SetNeighbor(HexDirection.W, _cells[i - 1]);

            if (z > 0)
            {
                if (z % 2 == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - Width]);
                    if (x > 0)
                        cell.SetNeighbor(HexDirection.SW, _cells[i - Width - 1]);

                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - Width]);
                    if (x < Width - 1)
                        cell.SetNeighbor(HexDirection.SE, _cells[i - Width + 1]);

                }
            }


            Text label = Instantiate(LabelPrefab);
            // rectTranform для UI элементов
            label.rectTransform.SetParent(_gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new(pos.x, pos.z);
            label.text = cell.Coordinates.ToStringOnSeparateLines();
            cell.UIRect = label.rectTransform;
            _cells[i] = cell;
        }
    }
}

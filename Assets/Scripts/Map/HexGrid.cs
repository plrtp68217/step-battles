using UnityEngine;
using UnityEngine.UI;

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

        //private void Update()
        //{
        //    if (Input.GetMouseButton(0))
        //        HandleInput();
        //}

        //private void HandleInput()
        //{
        //    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(inputRay, out RaycastHit hit))
        //    {
        //        SetCellColorAt(hit, _touchedColor);
        //    }

        //}

        public void SetCellColorAt(Vector3 point,Color color)
        {
            var pos = transform.InverseTransformPoint(point);
            HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
            int index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;
            HexCell cell = _cells[index];
            cell.Color = color;
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

            Text label = Instantiate(LabelPrefab);
            // rectTranform для UI элементов
            label.rectTransform.SetParent(_gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new(pos.x, pos.z);
            label.text = cell.Coordinates.ToStringOnSeparateLines();

            _cells[i] = cell;
        }
    }
}

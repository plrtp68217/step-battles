using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Map.MapEditor
{
    public class HexMapEditor : MonoBehaviour
    {

        [field: SerializeField]
        public Color[] Colors { get; private set; }


        [field: SerializeField]
        public HexGrid HexGrid { get; private set; }
        [field: SerializeField]
        public int BrushSize { get; private set; } = 2;

        private Color? _activeColor;
        private int _activeElevation;

        private void Awake()
        {
            SelectColor(0);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0)
                && !EventSystem.current.IsPointerOverGameObject())
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                EditCells(HexGrid.GetCellAt(hit.point));
            }
        }
        //private void HandleInput()
        //{
        //    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //    Plane mapPlane = new(Vector3.up, HexGrid.transform.position);

        //    if (mapPlane.Raycast(inputRay, out float distance))
        //    {
        //        Vector3 hitPoint = inputRay.GetPoint(distance);
        //        HexCell cell = HexGrid.GetCellAt(hitPoint);
        //        if (cell != null)
        //            EditCells(cell);
        //    }
        //}

        private void EditCells(HexCell center)
        {
            int centerX = center.Coordinates.X;
            int centerZ = center.Coordinates.Z;

            for (int r = 0, z = centerZ - BrushSize; z <= centerZ; z++, r++)
                for (int x = centerX - r; x <= centerX + BrushSize; x++)
                    EditCell(HexGrid.GetCellAt(new HexCoordinates(x, z)));


            for (int r = 0, z = centerZ + BrushSize; z > centerZ; z--, r++)
                for (int x = centerX - BrushSize; x <= centerX + r; x++)
                    EditCell(HexGrid.GetCellAt(new HexCoordinates(x, z)));


        }

        private void EditCell(HexCell cell)
        {
            if (cell == null) return;
            if (_activeColor.HasValue)
                cell.Color = _activeColor.Value;

            cell.Elevation = _activeElevation;
        }
        public void SetElevation(float elevation)
        {
            _activeElevation = (int)elevation;
        }
        public void SelectColor(int index)
        {
            if (index < 0) 
            {
                _activeColor = null;
                return;
            }
            _activeColor = Colors[index];
        }
    }

}

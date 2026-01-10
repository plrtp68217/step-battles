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

        private Color _activeColor;
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
            if (Physics.Raycast(inputRay, out var hit)) 
            {
                var cell = HexGrid.GetCellAt(hit.point);
                EditCell(cell);
            }

        }

        private void EditCell(HexCell cell) 
        {
            cell.Color = _activeColor;
            cell.Elevation = _activeElevation;
            HexGrid.Refresh();
        }
        public void SetElevation(float elevation)
        {
            _activeElevation = (int)elevation;
        }
        public void SelectColor(int index)
        {
            _activeColor = Colors[index];
        }
    }

}

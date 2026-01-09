using UnityEngine;

namespace Assets.Scripts.Map.MapEditor 
{
    public class HexMapEditor : MonoBehaviour
    {

        [field: SerializeField]
        public Color[] Colors { get; private set; }


        [field: SerializeField]
        public HexGrid HexGrid { get; private set; }

        private Color _activeColor;

        private void Awake()
        {
            SelectColor(0);
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                HandleInput();
            }
        }

        void HandleInput()
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out var hit))
                HexGrid.SetCellColorAt(hit.point, _activeColor);

        }

        public void SelectColor(int index)
        {
            _activeColor = Colors[index];
        }
    }

}

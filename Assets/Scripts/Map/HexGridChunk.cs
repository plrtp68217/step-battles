using Assets.Scripts.Map.Metric;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public sealed class HexGridChunk : MonoBehaviour
    {
        private HexCell[] _cells;
        private HexMesh _hexMesh;
        private Canvas _gridCanvas;

        private void Awake()
        {
            _gridCanvas = GetComponentInChildren<Canvas>();
            _hexMesh = GetComponentInChildren<HexMesh>();

            _cells = new HexCell[HexMetrics.CHUNK_SIZE_X * HexMetrics.CHUNK_SIZE_Z];
        }

        public void AddCell(int index, HexCell cell)
        {
            _cells[index] = cell;
            cell.Chunk = this; // Добавляем эту строку
            cell.transform.SetParent(transform, false);
            cell.UIRect.SetParent(_gridCanvas.transform, false);
        }

        public void Refresh()
        {
            enabled = true; // Включаем компонент для обновления в LateUpdate
        }

        private void LateUpdate()
        {
            _hexMesh.Triangulate(_cells);
            enabled = false; // Отключаем после обновления
        }
    }
}
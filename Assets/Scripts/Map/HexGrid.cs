using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Map.Metric;

namespace Assets.Scripts.Map
{
    public sealed class HexGrid : MonoBehaviour
    {
        [field: SerializeField] public HexCell CellPrefab { get; private set; }
        [field: SerializeField] public Text LabelPrefab { get; private set; }
        [field: SerializeField] public HexGridChunk ChunkPrefab { get; private set; }
        [field: SerializeField] public Texture2D NoiseSource { get; set; }

        [SerializeField] public int ChunkCountX = 4;
        [SerializeField] public int ChunkCountZ = 3;

        private int _cellCountX, _cellCountZ;
        private HexCell[] _cells;
        private HexGridChunk[] _chunks;

        private Color _defaultColor = Color.white;

        private void Awake()
        {
            HexMetrics.NoiseSource = NoiseSource;

            _cellCountX = ChunkCountX * HexMetrics.CHUNK_SIZE_X;
            _cellCountZ = ChunkCountZ * HexMetrics.CHUNK_SIZE_Z;

            CreateChunks();
            CreateCells();
        }

        private void CreateChunks()
        {
            _chunks = new HexGridChunk[ChunkCountX * ChunkCountZ];

            for (int z = 0, i = 0; z < ChunkCountZ; z++)
            {
                for (int x = 0; x < ChunkCountX; x++)
                {
                    HexGridChunk chunk = Instantiate(ChunkPrefab);
                    chunk.transform.SetParent(transform, false);
                    _chunks[i++] = chunk;
                }
            }
        }

        private void CreateCells()
        {
            _cells = new HexCell[_cellCountX * _cellCountZ];

            for (int z = 0, i = 0; z < _cellCountZ; z++)
            {
                for (int x = 0; x < _cellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        public HexCell GetCellAt(Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            HexCoordinates coordinates = HexCoordinates.FromPosition(pos);
            int index = coordinates.X + coordinates.Z * _cellCountX + coordinates.Z / 2;
            return _cells[index];
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 pos;
            pos.x = (x + z * 0.5f - z / 2) * HexMetrics.InnerRadius * 2f;
            pos.y = 0;
            pos.z = z * HexMetrics.OuterRadius * 1.5f;

            HexCell cell = _cells[i] = Instantiate(CellPrefab);
            cell.transform.localPosition = pos;
            cell.Coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.Color = _defaultColor;

            if (x > 0)
                cell.SetNeighbor(HexDirection.W, _cells[i - 1]);

            if (z > 0)
            {
                if (z % 2 == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, _cells[i - _cellCountX]);
                    if (x > 0)
                        cell.SetNeighbor(HexDirection.SW, _cells[i - _cellCountX - 1]);
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, _cells[i - _cellCountX]);
                    if (x < _cellCountX - 1)
                        cell.SetNeighbor(HexDirection.SE, _cells[i - _cellCountX + 1]);
                }
            }

            Text label = Instantiate(LabelPrefab);
            label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
            label.text = cell.Coordinates.ToStringOnSeparateLines();
            cell.UIRect = label.rectTransform;
            cell.Elevation = 0;

            AddCellToChunk(x, z, cell);
        }

        private void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.CHUNK_SIZE_X;
            int chunkZ = z / HexMetrics.CHUNK_SIZE_Z;
            HexGridChunk chunk = _chunks[chunkX + chunkZ * ChunkCountX];

            int localX = x - chunkX * HexMetrics.CHUNK_SIZE_X;
            int localZ = z - chunkZ * HexMetrics.CHUNK_SIZE_Z;
            chunk.AddCell(localX + localZ * HexMetrics.CHUNK_SIZE_X, cell);
        }
    }
}
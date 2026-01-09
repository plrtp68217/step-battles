using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Map
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class HexMesh : MonoBehaviour
    {
        private Mesh _hexMesh;
        private List<Vector3> _vertices;
        private List<int> _triangles;

        private List<Color> _colors;

        private MeshCollider _meshCollider;

        private void Awake()
        {
            _hexMesh = new Mesh
            {
                name = "Hex Mesh",
            };

            _vertices = new();
            _triangles = new();
            _colors = new();

            GetComponent<MeshFilter>().mesh = _hexMesh;

            _meshCollider = gameObject.AddComponent<MeshCollider>();

        }


        public void Triangulate(HexCell[] cells)
        {
            _hexMesh.Clear();
            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            for (int i = 0; i < cells.Length; i++)
                Triangulate(cells[i]);

            _hexMesh.vertices = _vertices.ToArray();
            _hexMesh.triangles = _triangles.ToArray();
            _hexMesh.colors = _colors.ToArray();
            _hexMesh.RecalculateNormals();

            _meshCollider.sharedMesh = _hexMesh;
        }

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _triangles.Add(vertexIndex);
            _triangles.Add(++vertexIndex);
            _triangles.Add(++vertexIndex);
        }

        private void Triangulate(HexCell hexCell)
        {
            Vector3 hexCenter = hexCell.transform.localPosition;
            for (int i = 0; i < Metric.HexMetrics.Corners.Length - 1; i++)
            {
                AddTriangle(
                    hexCenter,
                    hexCenter + Metric.HexMetrics.Corners[i],
                    hexCenter + Metric.HexMetrics.Corners[i + 1]
                );
                // Добвляем цвет каждой вершине
                for (var j = 0; j < 3; j++)
                    _colors.Add(hexCell.Color);
            }
        }
    }
}

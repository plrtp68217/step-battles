using Assets.Scripts.Map.Metric;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

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

        private Vector3 Perturb(Vector3 position)
        {
            Vector4 sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.CELL_PERTURB_STRENGTH;
            position.z += (sample.z * 2f - 1f) * HexMetrics.CELL_PERTURB_STRENGTH;
            return position;
        }



        private void Triangulate(HexCell hexCell)
        {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
                Triangulate(d, hexCell);
        }
        private void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.Color);

            TriangulateConnection(direction, cell, v1, v2);
        }

        private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(v1);
            _vertices.Add(v2);
            _vertices.Add(v3);
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(Perturb(v1));
            _vertices.Add(Perturb(v2));
            _vertices.Add(Perturb(v3));
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }





        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
        {
            HexCell neighbor = cell.GetNeighbor(direction);

            if (neighbor == null)
                return;

            Vector3 bridge = HexMetrics.GetBridge(direction);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;

            v3.y = neighbor.Elevation * HexMetrics.ELEVATION_STEP;
            v4.y = neighbor.Elevation * HexMetrics.ELEVATION_STEP;

            // Триангулируем ребро в зависимости от типа (ровное, склон, обрыв)
            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            }
            else
            {
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(cell.Color, neighbor.Color);
            }


            // Заполняем угол между текущей, соседней и следующей ячейкой
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Elevation * HexMetrics.ELEVATION_STEP;

                // Определяем, какая ячейка находится ниже, чтобы правильно ориентировать угол
                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                        TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                    else
                        TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);

                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
        }
        // Триангуляция уступов между ребрами
        private void TriangulateEdgeTerraces(
            EdgeVertices begin, HexCell beginCell,
            EdgeVertices end, HexCell endCell
        )
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

            for (int i = 2; i < HexMetrics.TERRACE_STEP; i++)
            {
                EdgeVertices e1 = e2;
                Color c1 = c2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
                TriangulateEdgeStrip(e1, c1, e2, c2);
            }

            TriangulateEdgeStrip(e2, c2, end, endCell.Color);
        }

        // Заполняет угол тремя ячейками, где два ребра — склоны (формирует "лестницу" уступов)
        private void TriangulateCornerTerraces(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.Color, c3, c4);

            for (int i = 2; i < HexMetrics.TERRACE_STEP; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        }

        // Заполняет угол, где левое ребро — склон, а правое — обрыв
        private void TriangulateCornerTerracesCliff(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell
        )
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        // Заполняет угол, где левое ребро — обрыв, а правое — склон (зеркально предыдущему)
        private void TriangulateCornerCliffTerraces(
            Vector3 begin, HexCell beginCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell
        )
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
            Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        }

        // Создает треугольник, где одна сторона — граница (например, край обрыва), а противоположная — склон с уступами
        private void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor
    )
        {
            Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            AddTriangleUnperturbed(Perturb(begin), v2, boundary);
            AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.TERRACE_STEP; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                AddTriangleUnperturbed(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangleUnperturbed(v2, Perturb(left), boundary);
            AddTriangleColor(c2, leftCell.Color, boundaryColor);
        }

        private void TriangulateCorner(
            Vector3 bottom, HexCell bottomCell,
            Vector3 left, HexCell leftCell,
            Vector3 right, HexCell rightCell
        )
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                else if (rightEdgeType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                else
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                else
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                else
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
            }
        }
        private void TriangulateEdgeTerraces(
            Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
            Vector3 endLeft, Vector3 endRight, HexCell endCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColor(beginCell.Color, c2);
            for (int i = 2; i < HexMetrics.TERRACE_STEP; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;
                v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }
            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.Color);

        }

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
        {
            AddTriangle(center, edge.v1, edge.v2);
            AddTriangleColor(color);
            AddTriangle(center, edge.v2, edge.v3);
            AddTriangleColor(color);
            AddTriangle(center, edge.v3, edge.v4);
            AddTriangleColor(color);
        }

        private void TriangulateEdgeStrip(
                EdgeVertices e1, Color c1,
                EdgeVertices e2, Color c2
        )
        {
            AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            AddQuadColor(c1, c2);
            AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            AddQuadColor(c1, c2);
            AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            AddQuadColor(c1, c2);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(Perturb(v1));
            _vertices.Add(Perturb(v2));
            _vertices.Add(Perturb(v3));
            _vertices.Add(Perturb(v4));
            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);
        }

        private void AddQuadColor(Color c1, Color c2)
            => AddQuadColor(c1, c1, c2, c2);

        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
        {
            _colors.Add(c1);
            _colors.Add(c2);
            _colors.Add(c3);
            _colors.Add(c4);
        }
        private void AddTriangleColor(Color c1) => AddTriangleColor(c1, c1, c1);
        private void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            _colors.Add(c1);
            _colors.Add(c2);
            _colors.Add(c3);
        }

    }
}

using Assets.Scripts.Map;
using Assets.Scripts.Map.Metric;
using System;
using UnityEngine;

namespace Assets.Scripts.MapCamera
{
    public sealed class MapCamera : MonoBehaviour
    {

        [field: SerializeField] public HexGrid Grid { get; private set; }
        [field: SerializeField] public Transform Stick { get; private set; }
        [field: SerializeField] public Transform Swivel { get; private set; }
        [field: SerializeField] public float StickMinZoom { get; private set; } = -250;
        [field: SerializeField] public float StickMaxZoom { get; private set; } = -45;
        [field: SerializeField] public float SwivelMinZoom { get; private set; } = -10;
        [field: SerializeField] public float SwivelMaxZoom { get; private set; } = 45;
        [field: SerializeField] public int MoveSpeed { get; private set; } = 100;
        [field: SerializeField] public int RotationSpeed { get; private set; } = 180;

        private float _currentZoom = 1f;
        private float _currentRotationAngle = 1f;

        private void Awake()
        {
            Swivel = transform.GetChild(0);
            Stick = Swivel.GetChild(0);
        }

        private void Update()
        {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                AdjustZoom(zoomDelta);
            }

            float rotationDelta = Input.GetAxis("Rotation");
            if (rotationDelta != 0f)
            {
                AdjustRotation(rotationDelta);
            }

            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            //_speedMultiplier = Input.GetKeyDown(KeyCode.LeftShift);
            if (xDelta != 0f || zDelta != 0f)
            {
                AdjustPosition(xDelta, zDelta);
            }
        }

        private void AdjustRotation(float rotationDelta)
        {
            _currentRotationAngle += rotationDelta * RotationSpeed * Time.deltaTime;
            if (_currentRotationAngle < 0f)
            {
                _currentRotationAngle += 360f;
            }
            else if (_currentRotationAngle >= 360f)
            {
                _currentRotationAngle -= 360f;
            }
            transform.localRotation = Quaternion.Euler(0f, _currentRotationAngle, 0f);
        }

        private void AdjustPosition(float xDelta, float zDelta)
        {
            Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance = MoveSpeed * damping * Time.deltaTime;

            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }

        Vector3 ClampPosition(Vector3 position)
        {
            float xMax = Grid.ChunkCountX * HexMetrics.CHUNK_SIZE_X * (2f * HexMetrics.INNER_RADIUS);
            position.x = Mathf.Clamp(position.x, 0f, xMax);

            float zMax = Grid.ChunkCountZ * HexMetrics.CHUNK_SIZE_Z * (1.5f * HexMetrics.OUTER_RADIUS);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }

        private void AdjustZoom(float zoomDelta)
        {
            // При оттдалении переходим в вид сверху (как 2д)
            _currentZoom = Mathf.Clamp01(_currentZoom + zoomDelta);

            float distance = Mathf.Lerp(StickMinZoom, StickMaxZoom, _currentZoom);
            Stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(SwivelMinZoom, SwivelMaxZoom, _currentZoom);
            Stick.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }
}

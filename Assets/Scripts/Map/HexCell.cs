using UnityEngine;

namespace Assets.Scripts.Map
{
    public sealed class HexCell : MonoBehaviour
    {
        [field: SerializeField]
        public HexCoordinates Coordinates { get; set; }

        public Color Color { get; set; }
    }
}


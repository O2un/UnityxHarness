using UnityEngine;

namespace O2un.Camera
{
    public interface ICameraBasisProvider
    {
        Vector3 PlanarForward { get; }
        Vector3 PlanarRight { get; }
    }
}

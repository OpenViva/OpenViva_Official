using UnityEngine;

namespace Viva.Interaction
{
    public interface IPointerSource
    {
        Ray GetRay();
        bool IsActive { get; }
    }
}
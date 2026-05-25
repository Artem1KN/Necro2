using UnityEngine;

public interface IDeflectable
{
    bool TryDeflect(Vector3 newDirection, Transform newOwner, float damageMultiplier);
}

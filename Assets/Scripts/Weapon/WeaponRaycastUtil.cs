using UnityEngine;

/// Helpers for hitscan weapons that start the ray from inside the player
/// capsule. Standard Physics.Raycast often grabs the player's own collider
/// first, so we step through the hits in distance order and skip anything
/// belonging to the player (matched by tag "Player").
public static class WeaponRaycastUtil
{
    private static readonly RaycastHit[] HitBuffer = new RaycastHit[16];

    public static bool RaycastSkippingPlayer(
        Vector3 origin,
        Vector3 direction,
        float maxRange,
        LayerMask layerMask,
        out RaycastHit firstValidHit)
    {
        int count = Physics.RaycastNonAlloc(origin, direction, HitBuffer, maxRange, layerMask, QueryTriggerInteraction.Ignore);
        if (count == 0)
        {
            firstValidHit = default;
            return false;
        }

        System.Array.Sort(HitBuffer, 0, count, RaycastDistanceComparer.Instance);

        for (int i = 0; i < count; i++)
        {
            var h = HitBuffer[i];
            if (h.collider == null) continue;
            if (h.collider.transform.root.CompareTag("Player")) continue;
            firstValidHit = h;
            return true;
        }

        firstValidHit = default;
        return false;
    }

    private sealed class RaycastDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
    {
        public static readonly RaycastDistanceComparer Instance = new();
        public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
    }
}

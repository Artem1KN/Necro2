using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject coolingOrbPrefab;
    [SerializeField] private GameObject healingOrbPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnCoolingOrb(Vector3 position, Vector3 targetPosition)
    {
        SpawnOrb(coolingOrbPrefab, position, targetPosition, Color.blue);
    }

    public void SpawnHealingOrb(Vector3 position, Vector3 targetPosition)
    {
        SpawnOrb(healingOrbPrefab, position, targetPosition, Color.green);
    }

    private void SpawnOrb(GameObject prefab, Vector3 position, Vector3 target, Color color)
    {
        if (prefab == null) return;

        GameObject orb = Instantiate(prefab, position, Quaternion.identity);
        // In a real implementation, an Orbit script would handle the movement towards 'target'
        // For now, we just provide the structure.
        Debug.Log($"Orb spawned at {position} heading to {target}");
    }
}
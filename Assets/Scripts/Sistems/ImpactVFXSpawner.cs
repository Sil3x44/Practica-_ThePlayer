using UnityEngine;

public class ImpactVFXSpawner : MonoBehaviour
{
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject hitVFXPrefab;
    [SerializeField] private GameObject blockVFXPrefab;

    [Header("Lifetime")]
    [SerializeField] private float vfxLifetime;

    public void SpawnHitVFX(Vector3 position, Vector3 normal)
    {
        SpawnVFX(hitVFXPrefab, position, normal);
    }

    public void SpawnBlockVFX(Vector3 position, Vector3 normal)
    {
        SpawnVFX(blockVFXPrefab, position, normal);
    }

    private void SpawnVFX(GameObject prefab, Vector3 position, Vector3 normal)
    {
        Quaternion rotation = normal.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        GameObject instance = Instantiate(prefab, position, rotation);
        Destroy(instance, vfxLifetime);
    }
}

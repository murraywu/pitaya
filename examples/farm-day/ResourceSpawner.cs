using System.Collections;
using UnityEngine;

namespace FarmDay.Resources
{
    /// <summary>
    /// Spawns resources like Stone and Wood at specified intervals.
    /// The spawned resources will have spawn protection enabled.
    /// </summary>
    public class ResourceSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] resourcePrefabs;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float spawnRadius = 3f;
        [SerializeField] private int maxResourcesInArea = 10;
        
        [Header("Spawn Area")]
        [SerializeField] private Transform spawnCenter;
        [SerializeField] private bool useRandomOffset = true;
        
        private int currentResourceCount = 0;
        
        private void Start()
        {
            if (spawnCenter == null)
            {
                spawnCenter = transform;
            }
            
            StartCoroutine(SpawnRoutine());
        }
        
        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                
                if (currentResourceCount < maxResourcesInArea)
                {
                    SpawnResource();
                }
            }
        }
        
        private void SpawnResource()
        {
            if (resourcePrefabs == null || resourcePrefabs.Length == 0)
            {
                Debug.LogWarning("[ResourceSpawner] No resource prefabs assigned!");
                return;
            }
            
            // Pick a random resource prefab
            GameObject prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
            
            // Calculate spawn position
            Vector3 spawnPosition = spawnCenter.position;
            if (useRandomOffset)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                spawnPosition += new Vector3(randomOffset.x, randomOffset.y, 0);
            }
            
            // Spawn the resource
            GameObject resource = Instantiate(prefab, spawnPosition, Quaternion.identity);
            
            // Track resource destruction to update count
            var collectableResource = resource.GetComponent<CollectableResource>();
            if (collectableResource != null)
            {
                currentResourceCount++;
                
                // Subscribe to destruction to update count
                // Note: In production, you'd want a more robust tracking system
                StartCoroutine(TrackResourceDestruction(resource));
            }
            
            Debug.Log($"[ResourceSpawner] Spawned {prefab.name} at {spawnPosition}. Total resources: {currentResourceCount}");
        }
        
        private IEnumerator TrackResourceDestruction(GameObject resource)
        {
            while (resource != null)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            currentResourceCount--;
            Debug.Log($"[ResourceSpawner] Resource destroyed. Remaining: {currentResourceCount}");
        }
        
        /// <summary>
        /// Manually spawn a resource of a specific type.
        /// </summary>
        public void SpawnResourceOfType(int prefabIndex)
        {
            if (prefabIndex < 0 || prefabIndex >= resourcePrefabs.Length)
            {
                Debug.LogWarning("[ResourceSpawner] Invalid prefab index!");
                return;
            }
            
            Vector3 spawnPosition = spawnCenter.position;
            if (useRandomOffset)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                spawnPosition += new Vector3(randomOffset.x, randomOffset.y, 0);
            }
            
            Instantiate(resourcePrefabs[prefabIndex], spawnPosition, Quaternion.identity);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Visualize spawn area in editor
            Transform center = spawnCenter != null ? spawnCenter : transform;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center.position, spawnRadius);
        }
    }
}

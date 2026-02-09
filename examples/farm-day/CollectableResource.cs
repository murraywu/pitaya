using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmDay.Resources
{
    /// <summary>
    /// Base class for collectable resources like Stone and Wood.
    /// Implements spawn protection to prevent automatic collection when resources
    /// spawn inside the farmer's collection area.
    /// 
    /// The resource will only be collected when:
    /// 1. The farmer enters the collection area AFTER the resource spawned, OR
    /// 2. The farmer exits and re-enters the collection area
    /// </summary>
    public class CollectableResource : MonoBehaviour
    {
        [Header("Resource Settings")]
        [SerializeField] protected ResourceType resourceType;
        [SerializeField] protected int amount = 1;
        
        [Header("Collection Settings")]
        [SerializeField] protected string collectorTag = "Player";
        [SerializeField] protected bool useSpawnProtection = true;
        
        // Tracks collectors that were inside the trigger when this resource spawned
        private HashSet<int> collectorsInsideOnSpawn = new HashSet<int>();
        private bool isInitialized = false;
        
        protected virtual void Start()
        {
            if (useSpawnProtection)
            {
                StartCoroutine(InitializeSpawnProtection());
            }
            else
            {
                isInitialized = true;
            }
        }
        
        /// <summary>
        /// Detects which collectors are already overlapping when the resource spawns.
        /// These collectors will need to exit and re-enter to collect this resource.
        /// </summary>
        private IEnumerator InitializeSpawnProtection()
        {
            // Wait for physics to process the new collider
            yield return new WaitForFixedUpdate();
            
            // Get our collider
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider == null)
            {
                Debug.LogWarning($"CollectableResource on {gameObject.name} has no Collider2D!");
                isInitialized = true;
                yield break;
            }
            
            // Find all overlapping colliders
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(Physics2D.AllLayers);
            
            List<Collider2D> overlapping = new List<Collider2D>();
            myCollider.OverlapCollider(filter, overlapping);
            
            foreach (var col in overlapping)
            {
                if (col.CompareTag(collectorTag))
                {
                    // Track this collector by instance ID
                    collectorsInsideOnSpawn.Add(col.gameObject.GetInstanceID());
                    Debug.Log($"[CollectableResource] {gameObject.name}: Collector {col.name} was inside on spawn, will require exit+re-entry to collect");
                }
            }
            
            isInitialized = true;
        }
        
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized) return;
            if (!other.CompareTag(collectorTag)) return;
            
            int collectorId = other.gameObject.GetInstanceID();
            
            // If this collector was inside when we spawned, don't collect yet
            if (collectorsInsideOnSpawn.Contains(collectorId))
            {
                Debug.Log($"[CollectableResource] {gameObject.name}: Ignoring enter from {other.name} (was inside on spawn)");
                return;
            }
            
            // Collector is entering fresh - attempt collection
            TryCollect(other);
        }
        
        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(collectorTag)) return;
            
            int collectorId = other.gameObject.GetInstanceID();
            
            // When a collector exits, remove them from the spawn protection list
            // Next time they enter, they can collect
            if (collectorsInsideOnSpawn.Remove(collectorId))
            {
                Debug.Log($"[CollectableResource] {gameObject.name}: {other.name} exited, spawn protection cleared");
            }
        }
        
        /// <summary>
        /// Attempts to collect this resource. Override in derived classes for custom behavior.
        /// </summary>
        protected virtual void TryCollect(Collider2D collector)
        {
            var farmerCollector = collector.GetComponent<IResourceCollector>();
            if (farmerCollector == null)
            {
                Debug.LogWarning($"[CollectableResource] Collector {collector.name} doesn't have IResourceCollector component!");
                return;
            }
            
            if (farmerCollector.CanCollect(resourceType))
            {
                farmerCollector.Collect(resourceType, amount);
                OnCollected();
            }
        }
        
        /// <summary>
        /// Called when the resource is successfully collected.
        /// </summary>
        protected virtual void OnCollected()
        {
            // Play collection effects, sounds, etc.
            Debug.Log($"[CollectableResource] {gameObject.name} collected! Type: {resourceType}, Amount: {amount}");
            
            // Destroy the resource
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Force clears spawn protection, allowing immediate collection.
        /// Useful for special cases where you want to override the protection.
        /// </summary>
        public void ClearSpawnProtection()
        {
            collectorsInsideOnSpawn.Clear();
        }
    }
    
    /// <summary>
    /// Types of resources that can be collected.
    /// </summary>
    public enum ResourceType
    {
        Stone,
        Wood,
        // Add other resource types as needed
    }
    
    /// <summary>
    /// Interface for objects that can collect resources (e.g., Farmer).
    /// </summary>
    public interface IResourceCollector
    {
        bool CanCollect(ResourceType type);
        void Collect(ResourceType type, int amount);
    }
}

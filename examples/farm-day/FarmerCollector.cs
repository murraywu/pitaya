using System.Collections.Generic;
using UnityEngine;

namespace FarmDay.Resources
{
    /// <summary>
    /// Component attached to the Farmer that handles resource collection.
    /// </summary>
    public class FarmerCollector : MonoBehaviour, IResourceCollector
    {
        [Header("Collection Settings")]
        [SerializeField] private int maxStoneCapacity = 100;
        [SerializeField] private int maxWoodCapacity = 100;
        
        [Header("Events")]
        public System.Action<ResourceType, int> OnResourceCollected;
        public System.Action<ResourceType, int> OnResourceChanged;
        
        private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
        
        private void Awake()
        {
            // Initialize resource counts
            resources[ResourceType.Stone] = 0;
            resources[ResourceType.Wood] = 0;
        }
        
        public bool CanCollect(ResourceType type)
        {
            int current = GetResourceAmount(type);
            int max = GetMaxCapacity(type);
            
            return current < max;
        }
        
        public void Collect(ResourceType type, int amount)
        {
            if (!CanCollect(type))
            {
                Debug.Log($"[FarmerCollector] Cannot collect {type}, inventory full!");
                return;
            }
            
            int current = GetResourceAmount(type);
            int max = GetMaxCapacity(type);
            int amountToAdd = Mathf.Min(amount, max - current);
            
            resources[type] = current + amountToAdd;
            
            Debug.Log($"[FarmerCollector] Collected {amountToAdd} {type}. Total: {resources[type]}");
            
            OnResourceCollected?.Invoke(type, amountToAdd);
            OnResourceChanged?.Invoke(type, resources[type]);
        }
        
        public int GetResourceAmount(ResourceType type)
        {
            return resources.TryGetValue(type, out int amount) ? amount : 0;
        }
        
        public int GetMaxCapacity(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Stone:
                    return maxStoneCapacity;
                case ResourceType.Wood:
                    return maxWoodCapacity;
                default:
                    return 100;
            }
        }
        
        /// <summary>
        /// Removes resources from inventory (e.g., when using them for crafting).
        /// </summary>
        public bool UseResource(ResourceType type, int amount)
        {
            int current = GetResourceAmount(type);
            if (current < amount)
            {
                return false;
            }
            
            resources[type] = current - amount;
            OnResourceChanged?.Invoke(type, resources[type]);
            return true;
        }
    }
}

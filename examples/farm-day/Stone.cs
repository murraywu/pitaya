using UnityEngine;

namespace FarmDay.Resources
{
    /// <summary>
    /// Stone resource that can be collected by the farmer.
    /// Inherits spawn protection from CollectableResource.
    /// </summary>
    public class Stone : CollectableResource
    {
        [Header("Stone Settings")]
        [SerializeField] private int stoneAmount = 1;
        [SerializeField] private GameObject collectionEffectPrefab;
        [SerializeField] private AudioClip collectionSound;
        
        protected override void Start()
        {
            // Set resource type for stone
            resourceType = ResourceType.Stone;
            amount = stoneAmount;
            
            base.Start();
        }
        
        protected override void OnCollected()
        {
            // Spawn collection effect
            if (collectionEffectPrefab != null)
            {
                Instantiate(collectionEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Play collection sound
            if (collectionSound != null)
            {
                AudioSource.PlayClipAtPoint(collectionSound, transform.position);
            }
            
            base.OnCollected();
        }
    }
}

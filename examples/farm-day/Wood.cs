using UnityEngine;

namespace FarmDay.Resources
{
    /// <summary>
    /// Wood resource that can be collected by the farmer.
    /// Inherits spawn protection from CollectableResource.
    /// </summary>
    public class Wood : CollectableResource
    {
        [Header("Wood Settings")]
        [SerializeField] private int woodAmount = 1;
        [SerializeField] private GameObject collectionEffectPrefab;
        [SerializeField] private AudioClip collectionSound;
        
        protected override void Start()
        {
            // Set resource type for wood
            resourceType = ResourceType.Wood;
            amount = woodAmount;
            
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

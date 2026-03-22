using UnityEngine;

public class WaterHazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        playerHealth = other.GetComponentInParent<PlayerHealth>();
        
        if (!playerHealth.GetIsDead())
        {
            playerHealth.TakeDamage(playerHealth.GetMaxHealth());
        }
    }
}

using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    private EnemyController enemyController;
    private Transform player;
    private PlayerNoiseEmitter playerNoiseEmitter;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        player = enemyController.GetPlayer();
        playerNoiseEmitter = player.GetComponent<PlayerNoiseEmitter>();
    }

    public bool CanHearPlayer()
    {
        float noiseRadius = playerNoiseEmitter.GetCurrentNoiseRadius();

        if (noiseRadius <= 0f)
        {
            return false;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        return distanceToPlayer <= noiseRadius;
    }
}

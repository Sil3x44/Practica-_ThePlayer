using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    private EnemyController enemyController;
    private NavMeshAgent agent;
    private Transform player;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
        player = enemyController.GetPlayer();
    }

    public void UpdateChase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }
}

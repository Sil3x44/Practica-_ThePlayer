using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint;
    [SerializeField] private float pointReachedDistance;

    private EnemyController enemyController;
    private NavMeshAgent agent;

    private int currentPointIndex;
    private float waitTimer;
    private bool isWaiting;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void StartPatrol()
    {
        isWaiting = false;
        waitTimer = 0f;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    public void UpdatePatrol()
    {
        if (patrolPoints.Length == 0)
        {
            return;
        }

        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= pointReachedDistance)
        {
            isWaiting = true;
            waitTimer = 0f;
            agent.ResetPath();
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                GoToNextPoint();
            }

            return;
        }

        if (!agent.hasPath)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }
    
    private void GoToNextPoint()
    {
        currentPointIndex++;

        if (currentPointIndex >= patrolPoints.Length)
        {
            currentPointIndex = 0;
        }

        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }
}


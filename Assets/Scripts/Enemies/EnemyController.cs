using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visuals;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float combatApproachSpeed;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public Transform GetVisuals()
    {
        return visuals;
    }

    public Transform GetEyePoint()
    {
        return eyePoint;
    }

    public Animator GetAnimator()
    {
        return animator;
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public float GetPatrolSpeed()
    {
        return patrolSpeed;
    }

    public float GetChaseSpeed()
    {
        return chaseSpeed;
    }

    public float GetCombatApproachSpeed()
    {
        return combatApproachSpeed;
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    public void SetAgentSpeed(float newSpeed)
    {
        agent.speed = newSpeed;
    }
}

using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;

    [Header("Hit Reaction")]
    [SerializeField] private float hitReactionDuration;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform executionStartPoint;
    [SerializeField] private Transform executionHoldPoint;
    [SerializeField] private Transform executionHitPoint;

    private int currentHealth;
    private bool isDead;
    private bool isBeingExecuted;
    private bool deathRegistered;

    private EnemyCombat enemyCombat;
    private EnemyBrain enemyBrain;
    private EnemyAudio enemyAudio;
    private NavMeshAgent agent;
    private Collider[] colliders;

    private void Awake()
    {
        enemyCombat = GetComponent<EnemyCombat>();
        enemyBrain = GetComponent<EnemyBrain>();
        enemyAudio = GetComponent<EnemyAudio>();
        agent = GetComponent<NavMeshAgent>();
        colliders = GetComponentsInChildren<Collider>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public DamageResult TakeDamage(int damage)
    {
        if (isDead || isBeingExecuted)
        {
            return DamageResult.None;
        }

        if (enemyCombat.IsBlocking())
        {
            enemyAudio.PlayBlock();
            return DamageResult.Blocked;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            DieNormal();
            return DamageResult.Killed;
        }

        enemyAudio.PlayHit();

        animator.ResetTrigger("Die");
        animator.ResetTrigger("ExecuteDeath");
        animator.SetTrigger("Hit");

        enemyBrain.EnterHitState(hitReactionDuration);

        return DamageResult.Hit;
    }

    public void StartExecutionDeath()
    {
        if (isDead || isBeingExecuted)
        {
            return;
        }

        isBeingExecuted = true;

        StopEnemySystemsForDeath();

        animator.applyRootMotion = true;
        animator.ResetTrigger("Die");
        animator.SetTrigger("ExecuteDeath");
    }

    public void ExecuteKill()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHealth = 0;

        RegisterDeathOnce();
    }

    private void DieNormal()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        StopEnemySystemsForDeath();

        animator.applyRootMotion = true;
        animator.ResetTrigger("ExecuteDeath");
        animator.SetTrigger("Die");

        enemyAudio.PlayDeath();

        RegisterDeathOnce();
    }

    private void StopEnemySystemsForDeath()
    {
        
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        agent.enabled = false;
        

        enemyBrain.EnterDeadState();
        enemyBrain.enabled = false;

        enemyCombat.enabled = false;

        DisableAllColliders();
    }

    private void DisableAllColliders()
    {
        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    private void RegisterDeathOnce()
    {
        if (deathRegistered)
        {
            return;
        }

        deathRegistered = true;

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RegisterEnemyDeath();
        }
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public bool GetIsBeingExecuted()
    {
        return isBeingExecuted;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public Transform GetExecutionStartPoint()
    {
        return executionStartPoint;
    }

    public Transform GetExecutionHoldPoint()
    {
        return executionHoldPoint;
    }

    public Transform GetExecutionHitPoint()
    {
        return executionHitPoint;
    }
}
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint;

    [Header("Attack")]
    [SerializeField] private float attackRadius;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float attackCooldown;

    [Header("Execution")]
    [SerializeField] private float executionRange;
    [SerializeField] private float executionBackAngleThreshold;
    [SerializeField] private LayerMask executionLayer;

    private PlayerInputReader inputReader;
    private PlayerMovement playerMovement;
    private PlayerAudio playerAudio;
    private ImpactVFXSpawner impactVFXSpawner;
    private CharacterController characterController;

    private EnemyHealth executionTarget;

    private float attackCooldownTimer;
    private bool hasDealtDamage;
    private bool isAttacking;
    private bool combatLocked;
    private bool isExecuting;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAudio = GetComponent<PlayerAudio>();
        impactVFXSpawner = GetComponent<ImpactVFXSpawner>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        UpdateCooldown();
        HandleAttackInput();
    }

    private void UpdateCooldown()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleAttackInput()
    {
        if (!inputReader.GetAttackPressedThisFrame())
        {
            return;
        }

        if (attackCooldownTimer > 0f || isAttacking || combatLocked)
        {
            return;
        }

        if (TryExecution())
        {
            return;
        }

        StartAttack();
    }

    private void StartAttack()
    {
        attackCooldownTimer = attackCooldown;
        hasDealtDamage = false;
        isAttacking = true;

        playerMovement.SetMovementLocked(true);

        animator.SetTrigger("Attack");
    }

    public void DealDamage()
    {
        if (hasDealtDamage)
        {
            return;
        }

        hasDealtDamage = true;

        playerAudio.PlayAttack();

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth == null)
            {
                continue;
            }

            DamageResult result = enemyHealth.TakeDamage(attackDamage);

            if (impactVFXSpawner != null)
            {
                Vector3 hitPoint = hit.ClosestPoint(attackPoint.position);
                Vector3 hitNormal = (hitPoint - attackPoint.position).normalized;

                if (hitNormal.sqrMagnitude < 0.001f)
                {
                    hitNormal = transform.forward;
                }

                switch (result)
                {
                    case DamageResult.Blocked:
                        impactVFXSpawner.SpawnBlockVFX(hitPoint, hitNormal);
                        break;

                    case DamageResult.Hit:
                    case DamageResult.Killed:
                        impactVFXSpawner.SpawnHitVFX(hitPoint, hitNormal);
                        break;
                }
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (!combatLocked)
        {
            playerMovement.SetMovementLocked(false);
        }
    }

    public void SetCombatLocked(bool isLocked)
    {
        combatLocked = isLocked;

        if (combatLocked)
        {
            isAttacking = false;
            playerMovement.SetMovementLocked(true);
        }
        else
        {
            playerMovement.SetMovementLocked(false);
        }
    }

    private bool TryExecution()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            executionRange,
            executionLayer
        );

        EnemyHealth bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
            {
                enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth == null || enemyHealth.GetIsDead() || enemyHealth.GetIsBeingExecuted())
            {
                continue;
            }

            EnemyBrain enemyBrain = enemyHealth.GetComponent<EnemyBrain>();

            if (enemyBrain == null || !enemyBrain.CanBeExecuted())
            {
                continue;
            }

            Vector3 directionFromEnemyToPlayer = (transform.position - enemyHealth.transform.position).normalized;
            float dot = Vector3.Dot(enemyHealth.transform.forward, directionFromEnemyToPlayer);

            if (dot > executionBackAngleThreshold)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemyHealth.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = enemyHealth;
            }
        }

        if (bestTarget == null)
        {
            return false;
        }

        StartExecution(bestTarget);
        return true;
    }

    private void StartExecution(EnemyHealth target)
    {
        executionTarget = target;

        isAttacking = true;
        isExecuting = true;
        combatLocked = true;
        hasDealtDamage = true;

        playerMovement.SetMovementLocked(true);
        playerMovement.SetRotationLocked(true);

        characterController.enabled = false;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");

        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveY", 0f);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsCrouching", false);

        animator.SetTrigger("Execute");

        target.StartExecutionDeath();
    }

    public void SnapToExecutionStartPoint()
    {
        if (executionTarget == null)
        {
            return;
        }

        Transform point = executionTarget.GetExecutionStartPoint();
        if (point == null)
        {
            return;
        }

        transform.position = point.position;
        transform.rotation = point.rotation;
    }

    public void SnapToExecutionHoldPoint()
    {
        if (executionTarget == null)
        {
            return;
        }

        Transform point = executionTarget.GetExecutionHoldPoint();
        if (point == null)
        {
            return;
        }

        transform.position = point.position;
        transform.rotation = point.rotation;
    }

    public void ExecuteKillTarget()
    {
        if (!executionTarget.GetIsDead())
        {
            executionTarget.ExecuteKill();
        }
    }

    public void PlayExecutionHitVFX()
    {
        if (executionTarget == null)
        {
            return;
        }

        Vector3 hitPoint = executionTarget.transform.position + executionTarget.transform.up * 1.2f;
        Transform executionHitPoint = executionTarget.GetExecutionHitPoint();

        hitPoint = executionHitPoint.position;

        Vector3 hitNormal = transform.forward;
        impactVFXSpawner.SpawnHitVFX(hitPoint, hitNormal);

        playerAudio.PlayDaggerHit();
    }

    public void EndExecution()
    {
        isAttacking = false;
        isExecuting = false;
        combatLocked = false;

        characterController.enabled = true;

        playerMovement.SetMovementLocked(false);
        playerMovement.SetRotationLocked(false);

        executionTarget = null;
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    public bool GetIsExecuting()
    {
        return isExecuting;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, executionRange);
    }
}

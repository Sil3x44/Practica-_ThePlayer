using UnityEngine;
using UnityEngine.AI;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Distances")]
    [SerializeField] private float combatIdealRange;
    [SerializeField] private float combatActionRange;
    [SerializeField] private float combatChaseExitRange;

    [Header("Combat Idle Time")]
    [SerializeField] private float combatIdleMinTime;
    [SerializeField] private float combatIdleMaxTime;

    [Header("Rotation")]
    [SerializeField] private float faceTargetRotationSpeed;

    [Header("Action Durations")]
    [SerializeField] private float strafeDuration;
    [SerializeField] private float tauntDuration;
    [SerializeField] private float attack1Duration;
    [SerializeField] private float attack2Duration;
    [SerializeField] private float comboDuration;
    [SerializeField] private float comboSecondAttackTriggerTime;

    [Header("Strafe Settings")]
    [SerializeField] private float strafeSpeed;
    [SerializeField] private float strafeArrivalDistance;
    [SerializeField] private float strafeAngle;

    [Header("Action Weights")]
    [SerializeField] private int strafeLeftWeight;
    [SerializeField] private int strafeRightWeight;
    [SerializeField] private int attack1Weight;
    [SerializeField] private int attack2Weight;
    [SerializeField] private int comboWeight;
    [SerializeField] private int tauntWeight;

    [Header("Attack Impact")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask playerLayer;

    private EnemyController enemyController;
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private ImpactVFXSpawner impactVFXSpawner;

    private float currentCombatIdleTime;
    private float combatIdleTimer;

    private EnemyCombatAction currentAction = EnemyCombatAction.None;
    private float currentActionTimer;

    private Vector3 currentStrafeTarget;
    private bool strafeTargetSet;
    private bool comboSecondAttackTriggered;
    private bool hasDealtDamage;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
        impactVFXSpawner = GetComponent<ImpactVFXSpawner>();
    }

    private void Start()
    {
        player = enemyController.GetPlayer();
        animator = enemyController.GetAnimator();
    }

    public void StartCombatIdle()
    {
        combatIdleTimer = 0f;
        currentCombatIdleTime = Random.Range(combatIdleMinTime, combatIdleMaxTime);

        currentAction = EnemyCombatAction.None;
        currentActionTimer = 0f;
        strafeTargetSet = false;
        comboSecondAttackTriggered = false;
        hasDealtDamage = false;

        StopAgentMovement();
        ResetActionAnimatorFlags();
        ResetAttackTriggers();
    }

    public void UpdateCombatApproach()
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        if (distanceToPlayer <= combatIdealRange)
        {
            StopAgentMovement();
            FaceTarget();
            return;
        }
        
        agent.isStopped = false;
        agent.speed = enemyController.GetCombatApproachSpeed();
        agent.SetDestination(player.position);

        FaceTarget();
    }

    public void UpdateCombatIdle()
    {
        StopAgentMovement();
        FaceTarget();

        combatIdleTimer += Time.deltaTime;
    }

    public bool CombatIdleFinished()
    {
        return combatIdleTimer >= currentCombatIdleTime;
    }

    public void StartCombatAction()
    {
        currentAction = ChooseRandomAction();
        currentActionTimer = 0f;
        strafeTargetSet = false;
        comboSecondAttackTriggered = false;
        hasDealtDamage = false;

        ResetActionAnimatorFlags();
        ResetAttackTriggers();

        switch (currentAction)
        {
            case EnemyCombatAction.StrafeLeft:
                agent.speed = strafeSpeed;
                animator.SetBool("StrafingLeft", true);
                break;

            case EnemyCombatAction.StrafeRight:
                agent.speed = strafeSpeed;
                animator.SetBool("StrafingRight", true);
                break;

            case EnemyCombatAction.Taunt:
                StopAgentMovement();
                animator.SetBool("Taunting", true);
                break;

            case EnemyCombatAction.Attack1:
                StopAgentMovement();
                animator.SetTrigger("Attack1");
                break;

            case EnemyCombatAction.Attack2:
                StopAgentMovement();
                animator.SetTrigger("Attack2");
                break;

            case EnemyCombatAction.Combo:
                StopAgentMovement();
                animator.SetTrigger("Attack1");
                break;
        }
    }

    public void UpdateCombatAction()
    {
        currentActionTimer += Time.deltaTime;

        switch (currentAction)
        {
            case EnemyCombatAction.StrafeLeft:
                UpdateStrafe(-1f);
                break;

            case EnemyCombatAction.StrafeRight:
                UpdateStrafe(1f);
                break;

            case EnemyCombatAction.Taunt:
                UpdateTaunt();
                break;

            case EnemyCombatAction.Attack1:
                UpdateAttack1();
                break;

            case EnemyCombatAction.Attack2:
                UpdateAttack2();
                break;

            case EnemyCombatAction.Combo:
                UpdateCombo();
                break;
        }
    }

    public bool CombatActionFinished()
    {
        switch (currentAction)
        {
            case EnemyCombatAction.StrafeLeft:
            case EnemyCombatAction.StrafeRight:
                return HasReachedStrafeDestination() || currentActionTimer >= strafeDuration;

            case EnemyCombatAction.Taunt:
                return currentActionTimer >= tauntDuration;

            case EnemyCombatAction.Attack1:
                return currentActionTimer >= attack1Duration;

            case EnemyCombatAction.Attack2:
                return currentActionTimer >= attack2Duration;

            case EnemyCombatAction.Combo:
                return currentActionTimer >= comboDuration;
        }

        return true;
    }

    public void FinishCombatAction()
    {
        ResetActionAnimatorFlags();
        ResetAttackTriggers();

        currentAction = EnemyCombatAction.None;
        currentActionTimer = 0f;
        strafeTargetSet = false;
        comboSecondAttackTriggered = false;
        hasDealtDamage = false;

        StopAgentMovement();
    }

    public void InterruptCombat()
    {
        ResetActionAnimatorFlags();
        ResetAttackTriggers();

        currentAction = EnemyCombatAction.None;
        currentActionTimer = 0f;
        strafeTargetSet = false;
        comboSecondAttackTriggered = false;
        hasDealtDamage = false;

        StopAgentMovement();
    }

    private void UpdateStrafe(float directionSign)
    {
        agent.isStopped = false;

        if (!strafeTargetSet)
        {
            currentStrafeTarget = CalculateStrafePosition(directionSign);
            strafeTargetSet = true;
            agent.SetDestination(currentStrafeTarget);
        }

        FaceTarget();
    }

    private Vector3 CalculateStrafePosition(float directionSign)
    {
        Vector3 playerPosition = player.position;

        Vector3 offset = transform.position - playerPosition;
        offset.y = 0f;

        if (offset.sqrMagnitude < 0.001f)
        {
            offset = -transform.forward;
        }

        offset = offset.normalized * combatIdealRange;

        Quaternion rotation = Quaternion.Euler(0f, strafeAngle * directionSign, 0f);
        Vector3 newOffset = rotation * offset;

        return playerPosition + newOffset;
    }

    private bool HasReachedStrafeDestination()
    {
        if (!strafeTargetSet)
        {
            return false;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentStrafeTarget;

        currentPosition.y = 0f;
        targetPosition.y = 0f;

        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        return distanceToTarget <= strafeArrivalDistance;
    }

    private void UpdateTaunt()
    {
        StopAgentMovement();
        FaceTarget();
    }

    private void UpdateAttack1()
    {
        StopAgentMovement();
        FaceTarget();
    }

    private void UpdateAttack2()
    {
        StopAgentMovement();
        FaceTarget();
    }

    private void UpdateCombo()
    {
        StopAgentMovement();
        FaceTarget();

        if (!comboSecondAttackTriggered && currentActionTimer >= comboSecondAttackTriggerTime)
        {
            comboSecondAttackTriggered = true;
            hasDealtDamage = false;
            animator.SetTrigger("Attack2");
        }
    }

    private EnemyCombatAction ChooseRandomAction()
    {
        int totalWeight = strafeLeftWeight + strafeRightWeight + attack1Weight + attack2Weight + comboWeight + tauntWeight;
        int randomValue = Random.Range(0, totalWeight);

        if (randomValue < strafeLeftWeight) return EnemyCombatAction.StrafeLeft;
        randomValue -= strafeLeftWeight;

        if (randomValue < strafeRightWeight) return EnemyCombatAction.StrafeRight;
        randomValue -= strafeRightWeight;

        if (randomValue < attack1Weight) return EnemyCombatAction.Attack1;
        randomValue -= attack1Weight;

        if (randomValue < attack2Weight) return EnemyCombatAction.Attack2;
        randomValue -= attack2Weight;

        if (randomValue < comboWeight) return EnemyCombatAction.Combo;

        return EnemyCombatAction.Taunt;
    }

    private void ResetActionAnimatorFlags()
    {
        animator.SetBool("StrafingLeft", false);
        animator.SetBool("StrafingRight", false);
        animator.SetBool("Taunting", false);
    }

    private void ResetAttackTriggers()
    {
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
    }

    private void StopAgentMovement()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void FaceTarget()
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetDirection = player.position - transform.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceTargetRotationSpeed * Time.deltaTime);
    }

    public float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        Vector3 enemyPosition = transform.position;
        Vector3 playerPosition = player.position;

        enemyPosition.y = 0f;
        playerPosition.y = 0f;

        return Vector3.Distance(enemyPosition, playerPosition);
    }

    public bool IsInsideCombatIdealRange()
    {
        return GetDistanceToPlayer() <= combatIdealRange;
    }

    public bool IsInsideCombatActionRange()
    {
        return GetDistanceToPlayer() <= combatActionRange;
    }

    public bool IsOutsideCombatChaseExitRange()
    {
        return GetDistanceToPlayer() > combatChaseExitRange;
    }

    public bool IsOutsideCombatActionRange()
    {
        return GetDistanceToPlayer() > combatActionRange;
    }

    public bool IsBlocking()
    {
        return currentAction == EnemyCombatAction.None ||
               currentAction == EnemyCombatAction.StrafeLeft ||
               currentAction == EnemyCombatAction.StrafeRight;
    }

    public EnemyCombatAction GetCurrentAction()
    {
        return currentAction;
    }

    public void DealDamage()
    {
        if (hasDealtDamage)
        {
            return;
        }

        hasDealtDamage = true;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

            playerHealth.TakeDamage(attackDamage);

            Vector3 hitPoint = hit.ClosestPoint(attackPoint.position);
            Vector3 hitNormal = (hitPoint - attackPoint.position).normalized;

            if (hitNormal.sqrMagnitude < 0.001f)
            {
                hitNormal = transform.forward;
            }

            impactVFXSpawner.SpawnHitVFX(hitPoint, hitNormal);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
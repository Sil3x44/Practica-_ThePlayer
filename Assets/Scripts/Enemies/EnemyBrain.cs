using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothFactor;

    private EnemyController enemyController;
    private EnemyVision enemyVision;
    private EnemyHearing enemyHearing;
    private EnemyPatrol enemyPatrol;
    private EnemyChase enemyChase;
    private EnemyCombat enemyCombat;
    private EnemyAudio enemyAudio;

    private NavMeshAgent agent;
    private Animator animator;

    private float hitTimer;
    private bool isDead;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        enemyVision = GetComponent<EnemyVision>();
        enemyHearing = GetComponent<EnemyHearing>();
        enemyPatrol = GetComponent<EnemyPatrol>();
        enemyChase = GetComponent<EnemyChase>();
        enemyCombat = GetComponent<EnemyCombat>();
        enemyAudio = GetComponent<EnemyAudio>();

        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        animator = enemyController.GetAnimator();
        EnterPatrolState();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        UpdateState();
        RotateTowardsMovement();
        UpdateAnimator();
        UpdateFootsteps();
    }

    private void UpdateState()
    {
        bool canSeePlayer = enemyVision.CanSeePlayer();
        bool canHearPlayer = enemyHearing.CanHearPlayer();

        switch (currentState)
        {
            case EnemyState.Patrol:
                enemyPatrol.UpdatePatrol();

                if (canSeePlayer || canHearPlayer)
                {
                    EnterChaseState();
                }
                break;

            case EnemyState.Chase:
                enemyChase.UpdateChase();

                if (enemyCombat.IsInsideCombatActionRange())
                {
                    EnterCombatApproachState();
                }
                break;

            case EnemyState.CombatApproach:
                enemyCombat.UpdateCombatApproach();

                if (enemyCombat.IsInsideCombatIdealRange())
                {
                    EnterCombatIdleState();
                }
                else if (enemyCombat.IsOutsideCombatChaseExitRange())
                {
                    EnterChaseState();
                }
                break;

            case EnemyState.CombatIdle:
                enemyCombat.UpdateCombatIdle();

                if (enemyCombat.IsOutsideCombatChaseExitRange())
                {
                    EnterChaseState();
                }
                else if (enemyCombat.IsOutsideCombatActionRange())
                {
                    EnterCombatApproachState();
                }
                else if (enemyCombat.CombatIdleFinished())
                {
                    EnterCombatActionState();
                }
                break;

            case EnemyState.CombatAction:
                enemyCombat.UpdateCombatAction();

                if (enemyCombat.IsOutsideCombatChaseExitRange())
                {
                    enemyCombat.FinishCombatAction();
                    EnterChaseState();
                }
                else if (enemyCombat.IsOutsideCombatActionRange())
                {
                    enemyCombat.FinishCombatAction();
                    EnterCombatApproachState();
                }
                else if (enemyCombat.CombatActionFinished())
                {
                    enemyCombat.FinishCombatAction();
                    EnterCombatIdleState();
                }
                break;

            case EnemyState.Hit:
                hitTimer -= Time.deltaTime;

                if (hitTimer <= 0f)
                {
                    if (enemyCombat.IsOutsideCombatChaseExitRange())
                    {
                        EnterChaseState();
                    }
                    else if (enemyCombat.IsOutsideCombatActionRange())
                    {
                        EnterCombatApproachState();
                    }
                    else
                    {
                        EnterCombatIdleState();
                    }
                }
                break;

            case EnemyState.Dead:
                break;
        }
    }

    private void EnterPatrolState()
    {
        currentState = EnemyState.Patrol;

        agent.isStopped = false;

        enemyController.SetAgentSpeed(enemyController.GetPatrolSpeed());
        enemyPatrol.StartPatrol();
    }

    private void EnterChaseState()
    {
        bool wasAlreadyChasing = currentState == EnemyState.Chase;

        currentState = EnemyState.Chase;

        agent.isStopped = false;

        enemyController.SetAgentSpeed(enemyController.GetChaseSpeed());

        if (!wasAlreadyChasing)
        {
            enemyAudio.PlayDetect();
        }
    }

    private void EnterCombatApproachState()
    {
        currentState = EnemyState.CombatApproach;

        agent.isStopped = false;

        enemyController.SetAgentSpeed(enemyController.GetCombatApproachSpeed());
    }

    private void EnterCombatIdleState()
    {
        currentState = EnemyState.CombatIdle;
        enemyController.SetAgentSpeed(enemyController.GetCombatApproachSpeed());
        enemyCombat.StartCombatIdle();
    }

    private void EnterCombatActionState()
    {
        currentState = EnemyState.CombatAction;
        enemyCombat.StartCombatAction();
    }

    public void EnterHitState(float duration)
    {
        if (isDead)
        {
            return;
        }

        currentState = EnemyState.Hit;
        hitTimer = duration;

        enemyCombat.InterruptCombat();

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void EnterDeadState()
    {
        isDead = true;
        currentState = EnemyState.Dead;
    }

    private void RotateTowardsMovement()
    {
        if (currentState == EnemyState.CombatApproach ||
            currentState == EnemyState.CombatIdle ||
            currentState == EnemyState.CombatAction ||
            currentState == EnemyState.Hit ||
            currentState == EnemyState.Dead)
        {
            return;
        }

        Vector3 moveDirection = agent.velocity;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothFactor * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        float speed = 0f;
        speed = agent.velocity.magnitude;

        animator.SetFloat("Speed", speed);

        bool inCombat =
            currentState == EnemyState.CombatApproach ||
            currentState == EnemyState.CombatIdle ||
            currentState == EnemyState.CombatAction ||
            currentState == EnemyState.Hit;

        animator.SetBool("InCombat", inCombat);
        animator.SetBool("Blocking", inCombat && enemyCombat.IsBlocking());
    }

    private void UpdateFootsteps()
    {
        if (isDead)
        {
            enemyAudio.StopFootsteps();
            return;
        }

        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (!isMoving)
        {
            enemyAudio.StopFootsteps();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                enemyAudio.StartFootsteps(enemyAudio.GetPatrolPitch());
                break;

            case EnemyState.Chase:
                enemyAudio.StartFootsteps(enemyAudio.GetChasePitch());
                break;

            case EnemyState.CombatApproach:
                enemyAudio.StartFootsteps(enemyAudio.GetCombatApproachPitch());
                break;

            case EnemyState.CombatAction:
                if (enemyCombat.GetCurrentAction() == EnemyCombatAction.StrafeLeft ||
                    enemyCombat.GetCurrentAction() == EnemyCombatAction.StrafeRight)
                {
                    enemyAudio.StartFootsteps(enemyAudio.GetStrafePitch());
                }
                else
                {
                    enemyAudio.StopFootsteps();
                }
                break;

            default:
                enemyAudio.StopFootsteps();
                break;
        }
    }

    public bool CanBeExecuted()
    {
        return currentState == EnemyState.Patrol;
    }

    public EnemyState GetCurrentState()
    {
        return currentState;
    }
}
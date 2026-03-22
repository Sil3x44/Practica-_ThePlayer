using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Animation Damping")]
    [SerializeField] private float inputDampTime = 0.1f;

    private void Update()
    {
        if (playerCombat.GetIsExecuting())
        {
            ForceIdleLocomotion();
            return;
        }

        if (playerHealth.GetIsDead())
        {
            ForceIdleLocomotion();
            return;
        }

        Vector2 moveInput = inputReader.GetMoveInput();

        animator.SetFloat("MoveX", moveInput.x, inputDampTime, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, inputDampTime, Time.deltaTime);
        animator.SetBool("IsRunning", playerMovement.GetIsRunning());
        animator.SetBool("IsCrouching", playerMovement.GetIsCrouching());
    }

    private void ForceIdleLocomotion()
    {
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveY", 0f);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsCrouching", false);
    }
}

using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Animation Damping")]
    [SerializeField] private float inputDampTime;

    private void Update()
    {
        Debug.Log("IsCrouched: " + playerMovement.GetIsCrouching());
        Vector2 moveInput = inputReader.GetMoveInput();
        Debug.Log("MoveX: " + moveInput.x + " MoveY: " + moveInput.y);
        
        animator.SetFloat("MoveX", moveInput.x, inputDampTime, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, inputDampTime, Time.deltaTime);
        animator.SetBool("IsRunning", playerMovement.GetIsRunning());
        animator.SetBool("IsCrouching", playerMovement.GetIsCrouching());
    }
}

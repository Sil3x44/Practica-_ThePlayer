using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput input;

    private Vector2 moveInput;
    private bool attackPressedThisFrame;
    private bool throwPressedThisFrame;
    private bool pausePressedThisFrame;

    private MovementMode currentMovementMode = MovementMode.Walk;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        input.actions["Move"].performed += OnMovePerformed;
        input.actions["Move"].canceled += OnMovePerformed;

        input.actions["Run"].performed += OnRunPerformed;
        input.actions["Walk"].performed += OnWalkPerformed;
        input.actions["Crouch"].performed += OnCrouchPerformed;

        input.actions["Attack"].performed += OnAttackPerformed;
        input.actions["Pause"].performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        input.actions["Move"].performed -= OnMovePerformed;
        input.actions["Move"].canceled -= OnMovePerformed;

        input.actions["Run"].performed -= OnRunPerformed;
        input.actions["Walk"].performed -= OnWalkPerformed;
        input.actions["Crouch"].performed -= OnCrouchPerformed;

        input.actions["Attack"].performed -= OnAttackPerformed;
        input.actions["Pause"].performed -= OnPausePerformed;
    }

    private void LateUpdate()
    {
        attackPressedThisFrame = false;
        throwPressedThisFrame = false;
        pausePressedThisFrame = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        currentMovementMode = MovementMode.Run;
    }

    private void OnWalkPerformed(InputAction.CallbackContext context)
    {
        currentMovementMode = MovementMode.Walk;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        currentMovementMode = MovementMode.Crouch;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        attackPressedThisFrame = true;
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        throwPressedThisFrame = true;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        pausePressedThisFrame = true;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public MovementMode GetCurrentMovementMode()
    {
        return currentMovementMode;
    }

    public bool GetAttackPressedThisFrame()
    {
        return attackPressedThisFrame;
    }

    public bool GetPausePressedThisFrame()
    {
        return pausePressedThisFrame;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput input;

    private Vector2 moveInput;
    private bool isRunPressed;
    private bool isCrouchPressed;
    private bool attackPressedThisFrame;
    private bool throwPressedThisFrame;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        input.actions["Move"].performed += OnMovePerformed;
        input.actions["Move"].canceled += OnMovePerformed;

        input.actions["Run"].started += OnRunStarted;
        input.actions["Run"].canceled += OnRunCanceled;

        input.actions["Crouch"].started += OnCrouchStarted;
        input.actions["Crouch"].canceled += OnCrouchCanceled;

        input.actions["Attack"].performed += OnAttackPerformed;
        input.actions["Throw"].performed += OnThrowPerformed;
    }

    private void OnDisable()
    {
        input.actions["Move"].performed -= OnMovePerformed;
        input.actions["Move"].canceled -= OnMovePerformed;

        input.actions["Run"].started -= OnRunStarted;
        input.actions["Run"].canceled -= OnRunCanceled;

        input.actions["Crouch"].started -= OnCrouchStarted;
        input.actions["Crouch"].canceled -= OnCrouchCanceled;

        input.actions["Attack"].performed -= OnAttackPerformed;
        input.actions["Throw"].performed -= OnThrowPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnRunStarted(InputAction.CallbackContext context)
    {
        isRunPressed = true;
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        isRunPressed = false;
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        isCrouchPressed = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        isCrouchPressed = false;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        attackPressedThisFrame = true;
    }

    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        throwPressedThisFrame = true;
    }

    private void LateUpdate()
    {
        attackPressedThisFrame = false;
        throwPressedThisFrame = false;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public bool GetIsRunPressed()
    {
        return isRunPressed;
    }

    public bool GetIsCrouchPressed()
    {
        return isCrouchPressed;
    }

    public bool GetAttackPressedThisFrame()
    {
        return attackPressedThisFrame;
    }

    public bool GetThrowPressedThisFrame()
    {
        return throwPressedThisFrame;
    }
}
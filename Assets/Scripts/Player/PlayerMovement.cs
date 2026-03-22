using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float sidewaysAndBackwardsMultiplier;
    [SerializeField] private float movementSmoothFactor;
    [SerializeField] private float rotationSmoothFactor;

    [Header("Gravity")]
    [SerializeField] private float gravityScale;
    [SerializeField] private float groundedGravity;

    [Header("Ground Detection")]
    [SerializeField] private float detectionRadius;
    [SerializeField] private Transform feet;
    [SerializeField] private LayerMask whatIsGround;

    private CharacterController controller;
    private PlayerInputReader inputReader;
    private PlayerAudio playerAudio;

    private Vector2 inputVector;
    private Vector3 horizontalMovement;
    private Vector3 verticalMovement;
    private Vector3 totalMovement;

    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
    private float rotationVelocity;

    private bool isGrounded;
    private bool movementLocked;
    private bool rotationLocked;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    private void Update()
    {
        if (controller == null || !controller.enabled)
        {
            return;
        }

        GroundCheck();
        ApplyGravity();
        MoveAndRotate();
        UpdateFootsteps();
    }

    private void MoveAndRotate()
    {
        inputVector = inputReader.GetMoveInput();

        if (movementLocked)
        {
            targetSpeed = 0f;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, movementSmoothFactor);
            horizontalMovement = Vector3.zero;

            if (!rotationLocked)
            {
                RotateTowardsCameraForward();
            }

            totalMovement = horizontalMovement + verticalMovement;
            controller.Move(totalMovement * Time.deltaTime);
            return;
        }

        float maxSpeed = GetCurrentMaxSpeed();
        float directionMultiplier = GetDirectionSpeedMultiplier();
        targetSpeed = maxSpeed * inputVector.magnitude * directionMultiplier;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, movementSmoothFactor);

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        horizontalMovement = (cameraForward * inputVector.y + cameraRight * inputVector.x) * currentSpeed;

        if (!rotationLocked)
        {
            RotateTowardsCameraForward();
        }

        totalMovement = horizontalMovement + verticalMovement;
        controller.Move(totalMovement * Time.deltaTime);
    }

    private void RotateTowardsCameraForward()
    {
        Vector3 lookDirection = playerCamera.transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        float targetAngle = Quaternion.LookRotation(lookDirection).eulerAngles.y;
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothFactor
        );

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalMovement.y < 0f)
        {
            verticalMovement.y = groundedGravity;
        }
        else
        {
            verticalMovement.y += gravityScale * Time.deltaTime;
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(feet.position, detectionRadius, whatIsGround);
    }

    private float GetCurrentMaxSpeed()
    {
        MovementMode currentMode = inputReader.GetCurrentMovementMode();

        switch (currentMode)
        {
            case MovementMode.Crouch:
                return crouchSpeed;
            case MovementMode.Run:
                return runSpeed;
            case MovementMode.Walk:
            default:
                return walkSpeed;
        }
    }

    private float GetDirectionSpeedMultiplier()
    {
        bool isMovingSideways = Mathf.Abs(inputVector.x) > 0.01f;
        bool isMovingBackwards = inputVector.y < -0.01f;

        if (isMovingSideways || isMovingBackwards)
        {
            return sidewaysAndBackwardsMultiplier;
        }

        return 1f;
    }

    private void UpdateFootsteps()
    {
        if (playerAudio == null)
        {
            return;
        }

        bool shouldPlayFootsteps = GetIsMoving() && isGrounded && !movementLocked;

        if (!shouldPlayFootsteps)
        {
            playerAudio.StopFootsteps();
            return;
        }

        if (GetIsCrouching())
        {
            playerAudio.StartFootsteps(playerAudio.GetCrouchPitch());
        }
        else if (GetIsRunning())
        {
            playerAudio.StartFootsteps(playerAudio.GetRunPitch());
        }
        else
        {
            playerAudio.StartFootsteps(playerAudio.GetWalkPitch());
        }
    }

    public void SetMovementLocked(bool isLocked)
    {
        movementLocked = isLocked;
    }

    public void SetRotationLocked(bool isLocked)
    {
        rotationLocked = isLocked;
    }

    public bool GetIsMovementLocked()
    {
        return movementLocked;
    }

    public bool GetIsRotationLocked()
    {
        return rotationLocked;
    }

    public bool GetIsMoving()
    {
        return inputVector.sqrMagnitude > 0.001f && !movementLocked;
    }

    public bool GetIsCrouching()
    {
        return inputReader.GetCurrentMovementMode() == MovementMode.Crouch;
    }

    public bool GetIsRunning()
    {
        return inputReader.GetCurrentMovementMode() == MovementMode.Run && GetIsMoving();
    }

    public int GetCurrentMovementModeIndex()
    {
        return (int)inputReader.GetCurrentMovementMode();
    }

    private void OnDrawGizmosSelected()
    {
        if (feet == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(feet.position, detectionRadius);
    }
}
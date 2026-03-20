using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visuals;
    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float sidewaysAndBackwardsMultiplier = 0.75f;
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

    private Vector2 inputVector;
    private Vector3 horizontalMovement;
    private Vector3 verticalMovement;
    private Vector3 totalMovement;

    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
    private float rotationVelocity;

    private bool isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        GroundCheck();
        ApplyGravity();
        MoveAndRotate();
    }

    private void MoveAndRotate()
    {
        inputVector = inputReader.GetMoveInput();

        float maxSpeed = GetCurrentMaxSpeed();
        float directionMultiplier = GetDirectionSpeedMultiplier();
        targetSpeed = maxSpeed * inputVector.magnitude *directionMultiplier;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, movementSmoothFactor);

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        horizontalMovement = (cameraForward * inputVector.y + cameraRight * inputVector.x) * currentSpeed;

        RotateTowardsCameraForward();

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

        float smoothAngle = Mathf.SmoothDampAngle(visuals.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothFactor);
        visuals.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

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
        if (inputReader.GetIsCrouchPressed())
        {
            return crouchSpeed;
        }

        if (inputReader.GetIsRunPressed())
        {
            return runSpeed;
        }

        return walkSpeed;
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

    public float GetCurrentHorizontalSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(horizontalMovement.x, 0f, horizontalMovement.z);
        return horizontalVelocity.magnitude;
    }

    public bool GetIsMoving()
    {
        return inputVector.sqrMagnitude > 0.001f;
    }

    public bool GetIsCrouching()
    {
        return inputReader.GetIsCrouchPressed();
    }

    public bool GetIsRunning()
    {
        return inputReader.GetIsRunPressed() && !inputReader.GetIsCrouchPressed() && GetIsMoving();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(feet.position, detectionRadius);
    }
}
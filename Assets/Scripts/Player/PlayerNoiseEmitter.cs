using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("Noise Radii")]
    [SerializeField] private float walkNoiseRadius;
    [SerializeField] private float runNoiseRadius;
    [SerializeField] private float crouchNoiseRadius;

    private PlayerInputReader inputReader;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public float GetCurrentNoiseRadius()
    {
        if (!playerMovement.GetIsMoving())
        {
            return 0f;
        }

        switch (inputReader.GetCurrentMovementMode())
        {
            case MovementMode.Run:
                return runNoiseRadius;

            case MovementMode.Crouch:
                return crouchNoiseRadius;

            case MovementMode.Walk:
            default:
                return walkNoiseRadius;
        }
    }

    public bool IsMakingNoise()
    {
        return GetCurrentNoiseRadius() > 0f;
    }
}

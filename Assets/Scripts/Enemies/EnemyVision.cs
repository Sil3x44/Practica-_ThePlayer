using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewDistance;
    [SerializeField] private float viewAngle;

    private EnemyController enemyController;
    private Transform eyePoint;
    private Transform player;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        eyePoint = enemyController.GetEyePoint();
        player = enemyController.GetPlayer();
    }

    public bool CanSeePlayer()
    {
        if (eyePoint == null || player == null)
        {
            return false;
        }

        Vector3 directionToPlayer = player.position - eyePoint.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            return false;
        }

        Vector3 forwardFlat = transform.forward;
        forwardFlat.y = 0f;
        forwardFlat.Normalize();

        Vector3 directionFlat = directionToPlayer;
        directionFlat.y = 0f;
        directionFlat.Normalize();

        float angleToPlayer = Vector3.Angle(forwardFlat, directionFlat);
        return angleToPlayer <= viewAngle * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null)
        {
            EnemyController controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                eyePoint = controller.GetEyePoint();
            }
        }

        if (eyePoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 leftBoundary = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + rightBoundary * viewDistance);
    }
}

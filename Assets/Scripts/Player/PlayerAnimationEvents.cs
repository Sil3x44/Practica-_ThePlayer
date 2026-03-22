using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerCombat playerCombat;

    private void Awake()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
    }

    public void DealDamage()
    {
        playerCombat.DealDamage();
    }

    public void EndAttack()
    {
        playerCombat.EndAttack();
    }

    public void ExecuteKill()
    {
        playerCombat.ExecuteKillTarget();
    }

    public void PlayExecutionHitVFX()
    {
        playerCombat.PlayExecutionHitVFX();
    }

    public void EndExecution()
    {
        playerCombat.EndExecution();
    }

    public void SnapToExecutionStartPoint()
    {
        playerCombat.SnapToExecutionStartPoint();
    }

    public void SnapToExecutionHoldPoint()
    {
        playerCombat.SnapToExecutionHoldPoint();
    }
}
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyCombat enemyCombat;
    private EnemyAudio enemyAudio;

    private void Awake()
    {
        enemyCombat = GetComponentInParent<EnemyCombat>();
        enemyAudio = GetComponentInParent<EnemyAudio>();
    }

    public void DealDamage()
    {
        enemyCombat.DealDamage();
    }

    public void PlayAttack1Sound()
    {
        enemyAudio.PlayAttack1();
    }

    public void PlayAttack2Sound()
    {
        enemyAudio.PlayAttack2();
    }

    public void PlayTauntSound()
    {
         enemyAudio.PlayTaunt();
    }

    public void PlayDeathSound()
    {
        enemyAudio.PlayDeath();
    }
}

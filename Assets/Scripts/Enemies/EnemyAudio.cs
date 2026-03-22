using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource loopSource;

    [Header("One Shot Clips")]
    [SerializeField] private AudioClip attack1Clip;
    [SerializeField] private AudioClip attack2Clip;
    [SerializeField] private AudioClip tauntClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip blockClip;
    [SerializeField] private AudioClip detectClip;

    [Header("Loop Clips")]
    [SerializeField] private AudioClip footstepLoopClip;

    [Header("Footstep Pitch")]
    [SerializeField] private float patrolPitch = 0.9f;
    [SerializeField] private float chasePitch = 1.15f;
    [SerializeField] private float combatApproachPitch = 0.8f;
    [SerializeField] private float strafePitch = 0.95f;

    private void Awake()
    {
        loopSource.loop = true;
        loopSource.playOnAwake = false;
    }

    private void PlayOneShot(AudioClip clip)
    {
        oneShotSource.PlayOneShot(clip);
    }

    public void PlayAttack1() => PlayOneShot(attack1Clip);
    public void PlayAttack2() => PlayOneShot(attack2Clip);
    public void PlayTaunt() => PlayOneShot(tauntClip);
    public void PlayHit() => PlayOneShot(hitClip);
    public void PlayDeath() => PlayOneShot(deathClip);
    public void PlayBlock() => PlayOneShot(blockClip);
    public void PlayDetect() => PlayOneShot(detectClip);

    public void StartFootsteps(float pitch)
    {
        loopSource.pitch = pitch;

        if (loopSource.clip != footstepLoopClip)
        {
            loopSource.clip = footstepLoopClip;
        }

        if (!loopSource.isPlaying)
        {
            loopSource.Play();
        }
    }

    public void StopFootsteps()
    {
        if (loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    public float GetPatrolPitch() => patrolPitch;
    public float GetChasePitch() => chasePitch;
    public float GetCombatApproachPitch() => combatApproachPitch;
    public float GetStrafePitch() => strafePitch;
}

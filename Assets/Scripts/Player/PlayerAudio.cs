using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioSource loopSource;

    [Header("One Shot Clips")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip daggerHitClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Loop Clips")]
    [SerializeField] private AudioClip footstepLoopClip;

    [Header("Footstep Pitch")]
    [SerializeField] private float crouchPitch;
    [SerializeField] private float walkPitch;
    [SerializeField] private float runPitch;

    private void Awake()
    {
        loopSource.loop = true;
        loopSource.playOnAwake = false;

        oneShotSource.loop = false;
        oneShotSource.playOnAwake = false;
    }

    private void PlayOneShot(AudioClip clip)
    {
        oneShotSource.PlayOneShot(clip);
    }

    public void PlayAttack() => PlayOneShot(attackClip);
    public void PlayDaggerHit() => PlayOneShot(daggerHitClip);
    public void PlayHit() => PlayOneShot(hitClip);
    public void PlayDeath() => PlayOneShot(deathClip);

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

    public float GetCrouchPitch() => crouchPitch;
    public float GetWalkPitch() => walkPitch;
    public float GetRunPitch() => runPitch;
}
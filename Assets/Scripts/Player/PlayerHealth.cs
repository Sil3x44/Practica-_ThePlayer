using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;

    [Header("Hit Reaction")]
    [SerializeField] private float hitReactionDuration;

    [Header("References")]
    [SerializeField] private Animator animator;

    private int currentHealth;
    private bool isDead;

    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerInputReader inputReader;
    private PlayerAudio playerAudio;

    private Coroutine hitRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;

        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        inputReader = GetComponent<PlayerInputReader>();
        playerAudio = GetComponent<PlayerAudio>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (playerAudio != null)
        {
            playerAudio.PlayHit();
        }

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitReactionRoutine());
    }

    private IEnumerator HitReactionRoutine()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (playerCombat != null)
        {
            playerCombat.SetCombatLocked(true);
        }
        else if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
        }

        yield return new WaitForSeconds(hitReactionDuration);

        if (isDead)
        {
            yield break;
        }

        if (playerCombat != null)
        {
            playerCombat.SetCombatLocked(false);
        }
        else if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(false);
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
            playerMovement.SetRotationLocked(true);
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        if (inputReader != null)
        {
            inputReader.enabled = false;
        }

        if (playerAudio != null)
        {
            playerAudio.PlayDeath();
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TriggerDefeat();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool GetIsDead()
    {
        return isDead;
    }
}

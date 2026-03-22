using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerInputReader playerInputReader;

    [Header("Health UI")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Locomotion UI")]
    [SerializeField] private Image locomotionIconImage;
    [SerializeField] private TextMeshProUGUI locomotionText;

    [Header("Locomotion Sprites")]
    [SerializeField] private Sprite crouchSprite;
    [SerializeField] private Sprite walkSprite;
    [SerializeField] private Sprite runSprite;

    [Header("Enemies UI")]
    [SerializeField] private TextMeshProUGUI enemiesLeftText;

    private void Update()
    {
        if (gameManager != null)
        {
            UpdateHealth();
            UpdateEnemies();
        }

        if (playerInputReader != null)
        {
            UpdateLocomotionState();
        }
    }

    private void UpdateHealth()
    {
        int currentHealth = gameManager.GetPlayerCurrentHealth();
        int maxHealth = gameManager.GetPlayerMaxHealth();

        if (healthFillImage != null)
        {
            if (maxHealth <= 0)
            {
                healthFillImage.fillAmount = 0f;
            }
            else
            {
                healthFillImage.fillAmount = (float)currentHealth / maxHealth;
            }
        }

        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + " / " + maxHealth;
        }
    }

    private void UpdateEnemies()
    {
        if (enemiesLeftText == null)
        {
            return;
        }

        enemiesLeftText.text = $"{gameManager.GetAliveEnemies()}";
    }

    private void UpdateLocomotionState()
    {
        MovementMode mode = playerInputReader.GetCurrentMovementMode();

        if (locomotionText != null)
        {
            switch (mode)
            {
                case MovementMode.Crouch:
                    locomotionText.text = "CROUCHING";
                    break;

                case MovementMode.Run:
                    locomotionText.text = "RUNNING";
                    break;

                case MovementMode.Walk:
                default:
                    locomotionText.text = "WALKING";
                    break;
            }
        }

        if (locomotionIconImage != null)
        {
            switch (mode)
            {
                case MovementMode.Crouch:
                    locomotionIconImage.sprite = crouchSprite;
                    break;

                case MovementMode.Run:
                    locomotionIconImage.sprite = runSprite;
                    break;

                case MovementMode.Walk:
                default:
                    locomotionIconImage.sprite = walkSprite;
                    break;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameFlowManager gameFlowManager;

    private bool gameEnded;
    private int aliveEnemies;
    private int totalEnemies;

    private void Start()
    {
        CountAliveEnemiesAtStart();
    }

    private void CountAliveEnemiesAtStart()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        totalEnemies = enemies.Length;
        aliveEnemies = enemies.Length;
    }

    public void RegisterEnemyDeath()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);

        if (aliveEnemies <= 0)
        {
            gameEnded = true;
            gameFlowManager.NotifyVictory();
        }
    }

    public void TriggerDefeat()
    {
        gameEnded = true;
        gameFlowManager.NotifyDefeat();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public int GetAliveEnemies()
    {
        return aliveEnemies;
    }

    public int GetTotalEnemies()
    {
        return totalEnemies;
    }

    public bool GetGameEnded()
    {
        return gameEnded;
    }

    public int GetPlayerCurrentHealth()
    {
        return playerHealth.GetCurrentHealth();
    }

    public int GetPlayerMaxHealth()
    {
        return playerHealth.GetMaxHealth();
    }
}
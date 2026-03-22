using System.Collections;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private CursorController cursorController;
    [SerializeField] private MusicManager musicManager;

    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Timing")]
    [SerializeField] private float introDuration = 4f;
    [SerializeField] private float victoryDelay = 1.5f;
    [SerializeField] private float defeatDelay = 1.5f;

    private GameFlowState currentState = GameFlowState.Intro;
    private Coroutine introCoroutine;

    private void Start()
    {
        EnterIntroState();
    }

    private void Update()
    {
        if (playerInputReader == null)
        {
            return;
        }

        if (playerInputReader.GetPausePressedThisFrame())
        {
            if (currentState == GameFlowState.Playing)
            {
                EnterPausedState();
            }
            else if (currentState == GameFlowState.Paused)
            {
                ResumeGameplay();
            }
        }
    }

    public void NotifyVictory()
    {
        if (currentState == GameFlowState.Victory || currentState == GameFlowState.Defeat)
        {
            return;
        }

        StartCoroutine(VictoryRoutine());
    }

    public void NotifyDefeat()
    {
        if (currentState == GameFlowState.Victory || currentState == GameFlowState.Defeat)
        {
            return;
        }

        StartCoroutine(DefeatRoutine());
    }

    private void EnterIntroState()
    {
        currentState = GameFlowState.Intro;

        SetPanelState(introPanel, true);
        SetPanelState(hudRoot, false);
        SetPanelState(pausePanel, false);
        SetPanelState(victoryPanel, false);
        SetPanelState(defeatPanel, false);

        Time.timeScale = 0f;

        if (cursorController != null)
        {
            cursorController.UnlockCursor();
        }

        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
        }

        introCoroutine = StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        yield return new WaitForSecondsRealtime(introDuration);
        CloseIntro();
    }

    public void CloseIntro()
    {
        if (currentState != GameFlowState.Intro)
        {
            return;
        }

        SetPanelState(introPanel, false);
        SetPanelState(hudRoot, true);

        ResumeGameplay();
    }

    public void EnterPausedState()
    {
        if (currentState != GameFlowState.Playing)
        {
            return;
        }

        currentState = GameFlowState.Paused;
        Time.timeScale = 0f;

        SetPanelState(pausePanel, true);

        if (cursorController != null)
        {
            cursorController.UnlockCursor();
        }
    }

    public void ResumeGameplay()
    {
        currentState = GameFlowState.Playing;
        Time.timeScale = 1f;

        SetPanelState(pausePanel, false);

        if (cursorController != null)
        {
            cursorController.LockCursor();
        }
    }

    private IEnumerator VictoryRoutine()
    {
        FreezePlayer();
        currentState = GameFlowState.Victory;

        yield return new WaitForSecondsRealtime(victoryDelay);

        Time.timeScale = 0f;

        SetPanelState(victoryPanel, true);
        SetPanelState(hudRoot, false);

        if (cursorController != null)
        {
            cursorController.UnlockCursor();
        }

        if (musicManager != null)
        {
            musicManager.PlayVictoryMusicAndStinger();
        }
    }

    private IEnumerator DefeatRoutine()
    {
        FreezePlayer();
        currentState = GameFlowState.Defeat;

        yield return new WaitForSecondsRealtime(defeatDelay);

        Time.timeScale = 0f;

        SetPanelState(defeatPanel, true);
        SetPanelState(hudRoot, false);

        if (cursorController != null)
        {
            cursorController.UnlockCursor();
        }

        if (musicManager != null)
        {
            musicManager.PlayDefeatMusicAndStinger();
        }
    }

    private void FreezePlayer()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();
        PlayerInputReader inputReader = FindFirstObjectByType<PlayerInputReader>();

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
    }

    private void SetPanelState(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    public GameFlowState GetCurrentState()
    {
        return currentState;
    }

    public bool IsGameplayBlocked()
    {
        return currentState != GameFlowState.Playing;
    }
}

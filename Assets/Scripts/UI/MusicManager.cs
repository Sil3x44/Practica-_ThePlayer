using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public enum MusicState
    {
        None,
        MainMenu,
        LevelAmbient,
        Combat,
        Victory,
        Defeat
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource stingerSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip levelAmbientMusic;
    [SerializeField] private AudioClip combatMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip defeatMusic;

    [Header("Stingers")]
    [SerializeField] private AudioClip goblinVictoryLaugh;
    [SerializeField] private AudioClip humanDefeatStinger;

    [Header("Volumes")]
    [SerializeField] private float musicVolume;
    [SerializeField] private float stingerVolume;

    [Header("Fade")]
    [SerializeField] private float fadeDuration;

    [Header("Level Settings")]
    [SerializeField] private bool useCombatMusic = true;
    [SerializeField] private bool startWithAmbientMusic = true;

    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private MusicState currentState = MusicState.None;
    private Coroutine fadeCoroutine;

    private GameManager gameManager;

    private void Awake()
    {
        activeSource = musicSourceA;
        inactiveSource = musicSourceB;

        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;
        musicSourceA.volume = 0f;

        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;
        musicSourceB.volume = 0f;

        stingerSource.loop = false;
        stingerSource.playOnAwake = false;
        stingerSource.volume = stingerVolume;
    }

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameObject.scene.name == "MainMenu")
        {
            PlayMusic(MusicState.MainMenu, true);
            return;
        }

        if (startWithAmbientMusic)
        {
            PlayMusic(MusicState.LevelAmbient, true);
        }
    }

    private void UpdateCombatMusicState()
    {
        bool anyEnemyInCombatState = IsAnyEnemyAlerted();

        if (anyEnemyInCombatState)
        {
            if (currentState != MusicState.Combat)
            {
                PlayMusic(MusicState.Combat);
            }
        }
        else
        {
            if (currentState == MusicState.Combat)
            {
                PlayMusic(MusicState.LevelAmbient);
            }
        }
    }

    private bool IsAnyEnemyAlerted()
    {
        EnemyBrain[] enemies = FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None);

        foreach (EnemyBrain enemy in enemies)
        {
            if (enemy == null || !enemy.enabled)
            {
                continue;
            }

            EnemyState state = enemy.GetCurrentState();

            if (state == EnemyState.Chase ||
                state == EnemyState.CombatApproach ||
                state == EnemyState.CombatIdle ||
                state == EnemyState.CombatAction)
            {
                return true;
            }
        }

        return false;
    }

    public void PlayMusic(MusicState newState, bool instant = false)
    {
        if (currentState == newState)
        {
            return;
        }

        AudioClip nextClip = GetClipForState(newState);

        if (nextClip == null)
        {
            return;
        }

        currentState = newState;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (instant)
        {
            activeSource.Stop();
            inactiveSource.Stop();

            activeSource.clip = nextClip;
            activeSource.volume = musicVolume;
            activeSource.Play();
            return;
        }

        fadeCoroutine = StartCoroutine(CrossfadeMusic(nextClip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float timer = 0f;
        float startActiveVolume = activeSource.volume;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, t);
            inactiveSource.volume = Mathf.Lerp(0f, musicVolume, t);

            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;
        inactiveSource.volume = musicVolume;

        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;

        fadeCoroutine = null;
    }

    private AudioClip GetClipForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.MainMenu:
                return mainMenuMusic;
            case MusicState.LevelAmbient:
                return levelAmbientMusic;
            case MusicState.Combat:
                return combatMusic;
            case MusicState.Victory:
                return victoryMusic;
            case MusicState.Defeat:
                return defeatMusic;
            default:
                return null;
        }
    }

    public void PlayVictoryMusicAndStinger()
    {
        PlayMusic(MusicState.Victory);
        stingerSource.PlayOneShot(goblinVictoryLaugh, stingerVolume);
    }

    public void PlayDefeatMusicAndStinger()
    {
        PlayMusic(MusicState.Defeat);
        stingerSource.PlayOneShot(humanDefeatStinger, stingerVolume);
    }
}

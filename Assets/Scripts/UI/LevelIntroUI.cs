using System.Collections;
using UnityEngine;

public class LevelIntroUI : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private float visibleDuration;

    private void Start()
    {
        Time.timeScale = 0f;

        introPanel.SetActive(true);

        hudRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        yield return new WaitForSecondsRealtime(visibleDuration);

        CloseIntro();
    }

    public void CloseIntro()
    {
        introPanel.SetActive(false);

        hudRoot.SetActive(true);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
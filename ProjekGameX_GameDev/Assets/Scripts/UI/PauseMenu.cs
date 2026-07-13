using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [HideInInspector] public bool isPaused = false;
    [SerializeField] private GameObject pauseCanvas;
    public IntroUI introUI;
    public PlayerDead playerDead;

    public InputManager inputManager;

    public LevelLoaderScript levelLoader;

    public bool menuPaused = false;
    public bool journalPaused = false;

    public void SetActiveHud(bool state)
    {
        pauseCanvas.SetActive(state);
    }
    public void SetActivePause()
    {
        if (!introUI.introPanelIsActive && !playerDead.playerIsDead)
        {
            isPaused = !isPaused;
            menuPaused = !menuPaused;
            pauseCanvas.SetActive(isPaused);
        }
    }

    public void SetActivePauseWithEvent(Component component, object data)
    {
        bool active = (bool)data;
        if (!introUI.introPanelIsActive && !playerDead.playerIsDead)
        {
            isPaused = active;
            journalPaused = active;
        }
    }

    public void PauseGame()
    {

        if (isPaused)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // AudioListener.pause = false;
        }

    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        DOTween.Clear(true);
    }
    public void AbrirCuestionarioGoogle()
    {
        string linkForms = "https://docs.google.com/forms/d/e/1FAIpQLSdEOHPEWi7yWnL7uwmSoDVN-AtD1OnuIc_S60mMSQWtJJiLPQ/viewform?usp=publish-editor";
        Application.OpenURL(linkForms);
    }

    public void BacktoMainMenu()
    {
        SetActivePause();
        AIDirectorBlackboard.ResetSession();

        levelLoader.LoadMainMenu();
    }
    private void Update()
    {
        PauseGame();
    }

    public void setTimeScaleOne()
    {
        isPaused = false;
    }
}
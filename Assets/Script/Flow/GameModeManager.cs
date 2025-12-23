using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode
    {
        Easy,
        Medium,
        Hard,
        God
    }

    [Header("Player Stats")]
    public int lives = 3;
    public int score = 0;

    [Header("Difficulty Progression")]
    public GameMode currentMode = GameMode.Easy;
    public int minigamesCompletedInMode = 0;
    public int minigamesPerMode = 3;

    [Header("Timer")]
    public float timer;
    private bool timerRunning;

    [Header("Transition")]
    public string transitionSceneName = "TempTransition";
    public float transitionDelay = 2f;

    [Header("Game Over")]
    public string gameOverSceneName = "TempGameOver";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        // Reset specific minigame data if needed
        if (scene.name == "SliceEmAll")
        {
            MoveAlongConveyor.ResetSliceEmAll();
            FruitSlice.currentCenterFruit = null;
            BombExplode.currentCenterBomb = null;
        }
    }

    void Update()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            MinigameFailed();
        }
    }

    // External calls for minigame handling
    public float GetTimeLimitForExternalCall()
    {
        switch (currentMode)
        {
            case GameMode.Medium: return 25f;
            case GameMode.Hard:   return 20f;
            case GameMode.God:    return 10f;
            default:              return 30f;
        }
    }

    public int GetBaseScoreForExternalCall()
    {
        switch (currentMode)
        {
            case GameMode.Medium: return 50;
            case GameMode.Hard:   return 100;
            case GameMode.God:    return 200;
            default:              return 20;
        }
    }

    public void StartTimerExternally()
    {
        timerRunning = true;
    }

    // Game progression
    void AdvanceDifficulty()
    {
        minigamesCompletedInMode = 0;

        if (currentMode == GameMode.God)
        {
            Debug.Log("MAX DIFFICULTY REACHED");
            return;
        }

        currentMode++;
        Debug.Log("Difficulty increased to: " + currentMode);
    }

    public void MinigameCompleted(int timeBonus = 0)
    {
        timerRunning = false;

        int points = GetBaseScoreForExternalCall() + timeBonus;

        if (currentMode == GameMode.God)
        {
            points *= 2;
        }

        score += points;
        minigamesCompletedInMode++;

        if (minigamesCompletedInMode >= minigamesPerMode)
        {
            StartCoroutine(ModeTransitionRoutine());
        }
    }

    public void MinigameFailed()
    {
        timerRunning = false;
        lives--;

        if (lives <= 0)
        {
            TriggerGameOver();
        }
    }

    IEnumerator ModeTransitionRoutine()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(transitionSceneName);

        yield return new WaitForSecondsRealtime(transitionDelay);

        Time.timeScale = 1f;
        AdvanceDifficulty();
    }

    void TriggerGameOver()
    {
        Debug.Log("GAME OVER");

        Time.timeScale = 0f;
        SceneManager.LoadScene(gameOverSceneName);
    }
}

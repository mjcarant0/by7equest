using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Scenes")]
    public List<string> minigameScenes = new List<string>()
    {
        "SliceEmAll",
        "Karate",
        "SimonSays"
    };

    [Header("Transition")]
    public string transitionSceneName = "TempTransition";
    public float transitionDelay = 2f;

    [Header("Game Over")]
    public string gameOverSceneName = "TempGameOver";

    private int currentSceneIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ShuffleScenes();
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
    }

    void Start()
    {
        LoadNextMinigame();
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

    void ShuffleScenes()
    {
        for (int i = 0; i < minigameScenes.Count; i++)
        {
            int rand = Random.Range(i, minigameScenes.Count);
            (minigameScenes[i], minigameScenes[rand]) =
            (minigameScenes[rand], minigameScenes[i]);
        }
    }

    void LoadNextMinigame()
    {
        if (currentSceneIndex >= minigameScenes.Count)
        {
            ShuffleScenes();
            currentSceneIndex = 0;
        }

        timer = GetTimeLimit();
        timerRunning = true;

        SceneManager.LoadScene(minigameScenes[currentSceneIndex]);
        currentSceneIndex++;
    }

    float GetTimeLimit()
    {
        switch (currentMode)
        {
            case GameMode.Medium: return 25f;
            case GameMode.Hard:   return 20f;
            case GameMode.God:    return 10f;
            default:              return 30f;
        }
    }

    int GetBaseScore()
    {
        switch (currentMode)
        {
            case GameMode.Medium: return 50;
            case GameMode.Hard:   return 100;
            case GameMode.God:    return 200;
            default:              return 20;
        }
    }

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

    public void MinigameCompleted()
    {
        if (!timerRunning) return;

        timerRunning = false;

        int bonus = Mathf.FloorToInt(timer);
        int points = GetBaseScore() + bonus;

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
        else
        {
            LoadNextMinigame();
        }
    }

    public void MinigameFailed()
    {
        if (!timerRunning) return;

        timerRunning = false;
        lives--;

        if (lives <= 0)
        {
            TriggerGameOver();
            return;
        }

        LoadNextMinigame();
    }

    IEnumerator ModeTransitionRoutine()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(transitionSceneName);

        yield return new WaitForSecondsRealtime(transitionDelay);

        Time.timeScale = 1f;
        AdvanceDifficulty();
        LoadNextMinigame();
    }

    void TriggerGameOver()
    {
        Debug.Log("GAME OVER");

        Time.timeScale = 0f;
        SceneManager.LoadScene(gameOverSceneName);
    }
}

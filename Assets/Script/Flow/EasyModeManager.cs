using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EasyModeManager : MonoBehaviour
{
    public static EasyModeManager Instance;

    public int lives = 3;
    public int score = 0;

    [Header("Easy Mode Settings")]
    public float minigameTimeLimit = 30f;

    private float timer;
    private bool timerRunning;

    private List<string> easyScenes = new List<string>()
    {
        "SliceEmAll",
        "Karate",
        "SimonSays"
    };

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

    void Start()
    {
        LoadNextMinigame();
    }

    void Update()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            MinigameFailed();
    }

    void ShuffleScenes()
    {
        for (int i = 0; i < easyScenes.Count; i++)
        {
            int randomIndex = Random.Range(i, easyScenes.Count);
            (easyScenes[i], easyScenes[randomIndex]) =
            (easyScenes[randomIndex], easyScenes[i]);
        }
    }

    void LoadNextMinigame()
    {
        if (currentSceneIndex >= easyScenes.Count)
        {
            Debug.Log("ALL EASY MODE MINIGAMES COMPLETE");
            Time.timeScale = 0f; // STOP WHOLE GAME
            return;
        }

        timer = minigameTimeLimit;
        timerRunning = true;

        SceneManager.LoadScene(easyScenes[currentSceneIndex]);
        currentSceneIndex++;
    }

    public void MinigameCompleted()
    {
        if (!timerRunning) return;

        timerRunning = false;
        score += 20 + Mathf.FloorToInt(timer);
        LoadNextMinigame();
    }

    public void MinigameFailed()
    {
        if (!timerRunning) return;

        timerRunning = false;
        lives--;

        if (lives <= 0)
        {
            Debug.Log("GAME OVER");
            Time.timeScale = 0f; // STOP GAME
        }
        else
        {
            LoadNextMinigame();
        }
    }
}

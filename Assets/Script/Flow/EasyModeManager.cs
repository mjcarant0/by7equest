using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EasyModeManager : MonoBehaviour
{
    public static EasyModeManager Instance;

    public int lives = 3;   // Player lives
    public int score = 0;   // Total score

    private float timer;        // Minigame timer
    private bool timerRunning;  // Is the timer active?

    // Easy Mode minigame scenes
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
            ShuffleScenes(); // Randomize minigame order
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
            Debug.Log("EASY MODE COMPLETE!");
            return;
        }

        timer = 30f;
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
        timerRunning = false;
        lives--;

        if (lives <= 0)
            Debug.Log("GAME OVER");
        else
            LoadNextMinigame();
    }
}

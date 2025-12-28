using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MinigameRandomizer : MonoBehaviour
{
    public static MinigameRandomizer Instance;

    [Header("Minigame Scenes")]
    public List<string> minigameScenes = new List<string>()
    {
        "SliceEmAll",
        "Karate",
        "SimonSays"
    };

    private int currentIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Shuffle();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Shuffle()
    {
        for (int i = 0; i < minigameScenes.Count; i++)
        {
            int rand = Random.Range(i, minigameScenes.Count);
            (minigameScenes[i], minigameScenes[rand]) =
                (minigameScenes[rand], minigameScenes[i]);
        }
    }

    public string GetNextMinigameName()
    {
        if (currentIndex >= minigameScenes.Count)
        {
            Shuffle();
            currentIndex = 0;
        }

        return minigameScenes[currentIndex];
    }

    public void LoadNextMinigame()
    {
        if (Instance == null)
        {
            Debug.LogError("MinigameRandomizer.Instance is null in LoadNextMinigame!");
            return;
        }

        if (currentIndex >= minigameScenes.Count)
        {
            Shuffle();
            currentIndex = 0;
        }

        if (GameModeManager.Instance != null)
        {
            // Set timer according to difficulty
            GameModeManager.Instance.timer =
                GameModeManager.Instance.GetTimeLimitForExternalCall();
            GameModeManager.Instance.StartTimerExternally();
        }
        else
        {
            Debug.LogError("GameModeManager.Instance is null when loading minigame!");
        }

        string nextScene = minigameScenes[currentIndex];
        currentIndex++;
        
        Debug.Log($"Loading minigame: {nextScene}");
        SceneManager.LoadScene(nextScene);
    }
}


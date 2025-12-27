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
            Debug.LogError("MinigameRandomizer.Instance is null!");
            return;
        }

        if (currentIndex >= minigameScenes.Count)
        {
            Shuffle();
            currentIndex = 0;
        }

        // Set timer according to difficulty
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.timer =
                GameModeManager.Instance.GetTimeLimitForExternalCall();
            GameModeManager.Instance.StartTimerExternally();
        }

        string nextScene = minigameScenes[currentIndex];
        currentIndex++;
        SceneManager.LoadScene(nextScene);
    }
}

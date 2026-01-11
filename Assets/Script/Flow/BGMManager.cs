using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;

    [Header("Scene Names")]
    public string modeDisplaySceneName = "ModeDisplay";
    public string gameStartSceneName = "GameStart";
    public string gameEndSceneName = "GameEnd";
    public string closingSceneName = "ClosingScene";

    private string lastSceneName = "";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Play BGM if starting in ModeDisplay, GameStart, or GameEnd
        if ((currentScene == modeDisplaySceneName ||
             currentScene == gameStartSceneName ||
             currentScene == gameEndSceneName) &&
             !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }

        lastSceneName = currentScene;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        // Always play on ModeDisplay (new mode) if not already playing
        if (currentScene == modeDisplaySceneName)
        {
            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
        // Always play on GameEnd if not already playing
        else if (currentScene == gameEndSceneName)
        {
            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
        // GameEnd → GameStart or ModeDisplay → GameStart: persist BGM
        else if (currentScene == gameStartSceneName &&
                (lastSceneName == modeDisplaySceneName || lastSceneName == gameEndSceneName))
        {
            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
        // Stop in ClosingScene
        else if (currentScene == closingSceneName)
        {
            if (bgmSource.isPlaying)
                bgmSource.Stop();
        }
        // Stop in any other scene
        else
        {
            if (bgmSource.isPlaying)
                bgmSource.Stop();
        }

        lastSceneName = currentScene;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

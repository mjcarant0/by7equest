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
        lastSceneName = SceneManager.GetActiveScene().name;

        // Play if starting in ModeDisplay, GameStart, or GameEnd
        if ((lastSceneName == modeDisplaySceneName ||
             lastSceneName == gameStartSceneName ||
             lastSceneName == gameEndSceneName) &&
             !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        // Always play in GameEnd
        if (currentScene == gameEndSceneName)
        {
            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
        // ModeDisplay → GameStart or GameEnd → GameStart: persist BGM
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

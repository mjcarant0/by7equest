using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentBGM : MonoBehaviour
{
    public static PersistentBGM Instance;

    public AudioSource bgmSource;

    [Header("Menu Scenes That Keep BGM")]
    public string landingSceneName = "LandingPage";
    public string creditsSceneName = "Credits";
    public string scoreSceneName = "Score";

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
        // Play on initial load if starting on landing page
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == landingSceneName ||
            scene.name == creditsSceneName ||
            scene.name == scoreSceneName)
        {
            if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

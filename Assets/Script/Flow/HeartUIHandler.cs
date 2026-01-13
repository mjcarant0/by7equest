using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class HeartUIHandler : MonoBehaviour
{
    // Shared life state across scenes; handlers can exist in multiple scenes.
    private static int sharedLives;
    private static bool sharedInitialized;

    [Header("Colored Hearts (Hearts GameObject)")]
    public SpriteRenderer[] coloredHearts;

    [Header("Black & White Hearts (Hearts BW GameObject)")]
    public SpriteRenderer[] bwHearts;

    [Header("Config")]
    public int maxLives = 3;

    [Header("Reference Discovery")]
    [Tooltip("Name of the GameObject that contains colored heart sprites in each scene")]
    public string coloredHeartsRootName = "Hearts";

    [Tooltip("Name of the GameObject that contains BW heart sprites in each scene")]
    public string bwHeartsRootName = "Hearts BW";

    [Header("Debug")]
    [Tooltip("Enable verbose logging for heart state changes")]
    public bool debugLogs = false;

    private int currentLives;

    private void OnValidate()
    {
        // Clamp maxLives to available heart pairs to avoid indexing issues in editor.
        if (coloredHearts != null && bwHearts != null)
        {
            int limit = Mathf.Min(coloredHearts.Length, bwHearts.Length);
            if (maxLives > limit) maxLives = limit;
        }
    }

    private void Awake()
    {
        // Initialize shared lives once so first scene shows hearts immediately.
        if (!sharedInitialized)
        {
            sharedLives = maxLives;
            sharedInitialized = true;
        }

        currentLives = sharedLives;

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (debugLogs) Debug.Log("[HeartUIHandler] Awake and registered sceneLoaded");

        EnsureHeartReferences(forceRefresh: false);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (debugLogs) Debug.Log("[HeartUIHandler] Start → Sync from shared");
        currentLives = sharedLives;
        UpdateHearts();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameModeManager.Instance == null) return;

        if (debugLogs) Debug.Log($"[HeartUIHandler] OnSceneLoaded: {scene.name} | currentLives={currentLives}");

        // Keep existing assignment; refresh only if missing.
        EnsureHeartReferences(forceRefresh: false);

        if (scene.name == GameModeManager.Instance.landingPage)
        {
            if (debugLogs) Debug.Log("[HeartUIHandler] Landing page loaded → ResetLives");
            ResetLives();
        }

        if (scene.name == GameModeManager.Instance.gameEndScene)
        {
            if (debugLogs) Debug.Log("[HeartUIHandler] GameEnd loaded → HandleMinigameResult");
            HandleMinigameResult();
        }

        // Keep visuals in sync when entering any scene that has hearts visible.
        UpdateHearts();
    }

    private void HandleMinigameResult()
    {
        if (GameModeManager.Instance.lastMinigameScore == 0)
        {
            if (debugLogs) Debug.Log("[HeartUIHandler] Minigame failed → LoseLife");
            LoseLife();
        }
        else if (debugLogs)
        {
            Debug.Log("[HeartUIHandler] Minigame success → no life lost");
        }
    }

    private void ResetLives()
    {
        if (!EnsureHeartReferences()) return;

        sharedLives = maxLives;
        currentLives = sharedLives;

        for (int i = 0; i < maxLives; i++)
        {
            coloredHearts[i].gameObject.SetActive(true);
            bwHearts[i].gameObject.SetActive(false);
        }

        // If additional hearts exist in arrays beyond maxLives, hide them.
        for (int i = maxLives; i < coloredHearts.Length; i++)
        {
            coloredHearts[i].gameObject.SetActive(false);
        }
        for (int i = maxLives; i < bwHearts.Length; i++)
        {
            bwHearts[i].gameObject.SetActive(false);
        }

        if (debugLogs) Debug.Log($"[HeartUIHandler] ResetLives → currentLives={currentLives}, maxLives={maxLives}");
    }

    private void LoseLife()
    {
        if (!EnsureHeartReferences()) return;

        sharedLives = Mathf.Max(0, sharedLives - 1);
        currentLives = sharedLives;
        if (debugLogs) Debug.Log($"[HeartUIHandler] LoseLife → currentLives={currentLives}");
        UpdateHearts();

        // Stop run if no lives left
        if (currentLives <= 0 && GameModeManager.Instance != null)
        {
            if (debugLogs) Debug.Log("[HeartUIHandler] Lives depleted → ending run");
            GameModeManager.Instance.FinalizeSession("");
        }
    }

    private void UpdateHearts()
    {
        if (!EnsureHeartReferences()) return;

        for (int i = 0; i < maxLives; i++)
        {
            if (i >= coloredHearts.Length || i >= bwHearts.Length)
            {
                if (debugLogs) Debug.LogWarning($"[HeartUIHandler] Index {i} out of bounds for hearts arrays");
                continue;
            }

            if (i < currentLives)
            {
                coloredHearts[i].gameObject.SetActive(true);
                coloredHearts[i].enabled = true;
                bwHearts[i].gameObject.SetActive(false);
                bwHearts[i].enabled = false;
            }
            else
            {
                coloredHearts[i].gameObject.SetActive(false);
                coloredHearts[i].enabled = false;
                bwHearts[i].gameObject.SetActive(true);
                bwHearts[i].enabled = true;
            }
        }

        if (debugLogs) Debug.Log($"[HeartUIHandler] UpdateHearts done. currentLives={currentLives}");
    }

    private bool EnsureHeartReferences(bool forceRefresh = false)
    {
        bool valid = ArraysValid();
        if (!valid || forceRefresh)
        {
            TryRefreshHeartReferences();
            valid = ArraysValid();
        }

        if (!valid && debugLogs)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool expectHearts = GameModeManager.Instance != null &&
                                (sceneName == GameModeManager.Instance.gameStartScene ||
                                 sceneName == GameModeManager.Instance.gameEndScene);

            if (expectHearts)
            {
                Debug.LogWarning("[HeartUIHandler] Heart references missing; ensure hearts exist in scene or names are correct.");
            }
        }

        // Clamp maxLives to available pairs after refresh
        if (coloredHearts != null && bwHearts != null)
        {
            int limit = Mathf.Min(coloredHearts.Length, bwHearts.Length);
            if (maxLives > limit) maxLives = limit;
        }

        return valid;
    }

    private bool ArraysValid()
    {
        return coloredHearts != null && bwHearts != null &&
               coloredHearts.Length > 0 && bwHearts.Length > 0 &&
               coloredHearts.All(r => r != null) && bwHearts.All(r => r != null);
    }

    private void TryRefreshHeartReferences()
    {
        // Prefer roots in the active scene to avoid mixing with DontDestroyOnLoad objects.
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject coloredRoot = FindInScene(activeScene, coloredHeartsRootName);
        GameObject bwRoot = FindInScene(activeScene, bwHeartsRootName);

        // Fallback to global find if not found in-scene.
        if (coloredRoot == null) coloredRoot = GameObject.Find(coloredHeartsRootName);
        if (bwRoot == null) bwRoot = GameObject.Find(bwHeartsRootName);

        if (coloredRoot != null)
        {
            // Keep natural hierarchy order (no reordering) so inspector element order is preserved.
            coloredHearts = coloredRoot.GetComponentsInChildren<SpriteRenderer>(true).ToArray();
        }

        if (bwRoot != null)
        {
            bwHearts = bwRoot.GetComponentsInChildren<SpriteRenderer>(true).ToArray();
        }

        if (debugLogs)
        {
            Debug.Log($"[HeartUIHandler] Refreshed hearts. Colored found: {coloredHearts?.Length ?? 0}, BW found: {bwHearts?.Length ?? 0}");
        }
    }

    private GameObject FindInScene(Scene scene, string name)
    {
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var t = FindChildByName(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindChildByName(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    public static void StaticResetLives()
    {
        sharedLives = 3;
        sharedInitialized = true;
        Debug.Log("[HeartUIHandler] Static lives reset to 3");
    }
}
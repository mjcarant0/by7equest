using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Displays leaderboard on ScoreScene
public class ScoreSceneLeaderboard : MonoBehaviour
{
    [Header("UI References")]
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    
    [Header("Loading & Error")]
    public GameObject loadingIndicator;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI noDataText;
    
    [Header("Settings")]
    public int maxEntries = 10;
    public bool autoLoadOnEnable = true;
    
    [Header("Entry Colors")]
    public Color firstPlaceColor = new Color(1f, 0.84f, 0f);    
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f); 
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);    
    public Color normalColor = Color.white;
    
    private List<GameObject> currentEntries = new List<GameObject>();

    private void OnEnable()
    {
        if (autoLoadOnEnable)
        {
            LoadLeaderboard();
        }
    }

    // Load and display leaderboard (highest to lowest)
    public void LoadLeaderboard()
    {
        if (PlayFabDatabase.Instance == null)
        {
            ShowError("Database not initialized!");
            return;
        }

        // If not connected, connect first (without player name - just read-only access)
        if (!PlayFabDatabase.Instance.IsConnected())
        {
            Debug.Log("[ScoreSceneLeaderboard] Connecting to database for read access...");
            PlayFabDatabase.Instance.ConnectToDatabase("Viewer"); 
            StartCoroutine(WaitForConnectionAndLoad());
            return;
        }

        // Clear existing entries
        ClearLeaderboard();
        
        // Show loading
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
        
        if (errorText != null)
            errorText.gameObject.SetActive(false);
            
        if (noDataText != null)
            noDataText.gameObject.SetActive(false);

        // Fetch leaderboard from database (highest to lowest)
        PlayFabDatabase.Instance.GetLeaderboard(maxEntries, OnLeaderboardLoaded);
    }
    
    private System.Collections.IEnumerator WaitForConnectionAndLoad()
    {
        // Wait up to 5 seconds for connection
        float timeout = 5f;
        float elapsed = 0f;

        while (!PlayFabDatabase.Instance.IsConnected() && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (PlayFabDatabase.Instance.IsConnected())
        {
            LoadLeaderboard(); 
        }
        else
        {
            ShowError("Connection timeout!");
        }
    }

    private void OnLeaderboardLoaded(List<LeaderboardEntry> entries)
    {

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        if (entries == null || entries.Count == 0)
        {
            ShowNoData();
            return;
        }

        // Display entries (already sorted highest to lowest from database)
        foreach (var entry in entries)
        {
            CreateLeaderboardEntry(entry);
        }

        Debug.Log($"[ScoreSceneLeaderboard] Displayed {entries.Count} entries");
    }

    private void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
        {
            Debug.LogError("[ScoreSceneLeaderboard] Missing prefab or container!");
            return;
        }

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        currentEntries.Add(entryObj);

        // Find text components
        TextMeshProUGUI[] texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (texts.Length >= 3)
        {
            // Format: Rank | Name | Score
            texts[0].text = $"#{entry.rank}";
            texts[1].text = entry.playerName;
            texts[2].text = entry.score.ToString();
            
            // Color based on rank
            Color entryColor = GetColorForRank(entry.rank);
            foreach (var text in texts)
            {
                text.color = entryColor;
            }
        }
        else if (texts.Length == 1)
        {
            // Fallback: single text
            texts[0].text = $"#{entry.rank}  {entry.playerName}  -  {entry.score}";
            texts[0].color = GetColorForRank(entry.rank);
        }
    }

    private Color GetColorForRank(int rank)
    {
        switch (rank)
        {
            case 1: return firstPlaceColor;
            case 2: return secondPlaceColor;
            case 3: return thirdPlaceColor;
            default: return normalColor;
        }
    }

    private void ClearLeaderboard()
    {
        foreach (var entry in currentEntries)
        {
            if (entry != null)
                Destroy(entry);
        }
        currentEntries.Clear();
    }

    private void ShowError(string message)
    {
        Debug.LogWarning($"[ScoreSceneLeaderboard] {message}");
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
        
        if (noDataText != null)
            noDataText.gameObject.SetActive(false);
    }

    private void ShowNoData()
    {
        Debug.Log("[ScoreSceneLeaderboard] No leaderboard data yet");
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
            
        if (errorText != null)
            errorText.gameObject.SetActive(false);
        
        if (noDataText != null)
        {
            noDataText.text = "No scores yet. Be the first to play!";
            noDataText.gameObject.SetActive(true);
        }
    }

    public void RefreshLeaderboard()
    {
        LoadLeaderboard();
    }
}

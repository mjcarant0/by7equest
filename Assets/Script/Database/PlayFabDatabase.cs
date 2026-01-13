using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;

/// <summary>
/// Simplified PlayFab Database Manager
/// Only stores player name and score
/// </summary>
public class PlayFabDatabase : MonoBehaviour
{
    public static PlayFabDatabase Instance;

    [Header("Database Status")]
    public bool isConnected = false;
    public bool showDebugLogs = true; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LogDebug("[PlayFabDatabase] Instance created");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // Clear instance reference when destroyed to prevent stale references
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        
    }

    #region Database Connection

    /// <summary>
    /// Connect to PlayFab database using player name
    /// </summary>
    public void ConnectToDatabase(string playerName)
    {
        // Set Title ID from secrets file
        if (!string.IsNullOrEmpty(PlayFabSecrets.TitleId) && PlayFabSecrets.TitleId != "YOUR_TITLE_ID_HERE")
        {
            PlayFabSettings.TitleId = PlayFabSecrets.TitleId;
            LogDebug($"[PlayFabDatabase] Title ID set from PlayFabSecrets");
        }
        else if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            Debug.LogError("[PlayFabDatabase] Title ID not set! Please configure PlayFabSecrets.cs with your Title ID.");
            return;
        }

        // Validate player name
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }

        // Use GUID + player name to ensure completely unique account each time
        string uniqueId = System.Guid.NewGuid().ToString() + "_" + playerName.ToLower().Trim();
        
        LogDebug($"[PlayFabDatabase] Connecting with name: {playerName}");
        
        var request = new LoginWithCustomIDRequest
        {
            CustomId = uniqueId,
            CreateAccount = true
        };

        LogDebug("[PlayFabDatabase] Connecting to database...");
        
        PlayFabClientAPI.LoginWithCustomID(request, OnConnectSuccess, OnConnectFailure);
    }

    private void OnConnectSuccess(LoginResult result)
    {
        isConnected = true;
        LogDebug($"[PlayFabDatabase] Connected! ID: {result.PlayFabId}");
    }

    private void OnConnectFailure(PlayFabError error)
    {
        isConnected = false;
        Debug.LogError($"[PlayFabDatabase] Connection failed: {error.GenerateErrorReport()}");
        Debug.LogError($"[PlayFabDatabase] Error: {error.ErrorMessage}");
        Debug.LogError($"[PlayFabDatabase] Title ID used: {PlayFabSettings.TitleId}");
        
        // If 409 conflict, try with a random ID
        if (error.HttpCode == 409)
        {
            Debug.LogWarning("[PlayFabDatabase] Retrying with new random ID...");
            RetryWithRandomId();
        }
    }
    
    private void RetryWithRandomId()
    {
        string randomId = System.Guid.NewGuid().ToString();
        
        var request = new LoginWithCustomIDRequest
        {
            CustomId = randomId,
            CreateAccount = true
        };
        
        LogDebug($"[PlayFabDatabase] Retrying connection with random ID...");
        PlayFabClientAPI.LoginWithCustomID(request, OnConnectSuccess, OnRetryFailure);
    }
    
    private void OnRetryFailure(PlayFabError error)
    {
        isConnected = false;
        Debug.LogError($"[PlayFabDatabase] Retry also failed: {error.GenerateErrorReport()}");
        Debug.LogError($"[PlayFabDatabase] Please check your Title ID in PlayFab Dashboard");
    }

    #endregion

    #region Save to Database

    /// Save player name and score to database
    public void SaveToDatabase(string playerName, int score, Action<bool> callback = null)
    {
        if (!isConnected)
        {
            Debug.LogError("[PlayFabDatabase] Cannot save - not connected to database!");
            callback?.Invoke(false);
            return;
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }

        LogDebug($"[PlayFabDatabase] Saving to database: {playerName} - {score}");

        // First update display name
        UpdatePlayerName(playerName, (nameSuccess) =>
        {
            if (nameSuccess)
            {
                // Then submit score
                SubmitScoreToDatabase(score, callback);
            }
            else
            {
                // Still try to submit score even if name update failed
                SubmitScoreToDatabase(score, callback);
            }
        });
    }

    private void UpdatePlayerName(string playerName, Action<bool> callback)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        LogDebug($"[PlayFabDatabase] Updating player name: {playerName}");

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            (result) =>
            {
                LogDebug($"[PlayFabDatabase] Player name updated successfully");
                callback?.Invoke(true);
            },
            (error) =>
            {
                Debug.LogError($"[PlayFabDatabase] Failed to update name: {error.GenerateErrorReport()}");
                callback?.Invoke(false);
            });
    }

    private void SubmitScoreToDatabase(int score, Action<bool> callback)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "HighScore",
                    Value = score,
                    Version = null  // Let PlayFab handle versioning automatically
                }
            }
        };

        LogDebug($"[PlayFabDatabase] Submitting score: {score}");

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            (result) =>
            {
                LogDebug("[PlayFabDatabase] ✓ Score saved to database successfully!");
                callback?.Invoke(true);
            },
            (error) =>
            {
                Debug.LogError($"[PlayFabDatabase] ✗ Failed to save score: {error.GenerateErrorReport()}");
                Debug.LogError($"[PlayFabDatabase] Error details: {error.ErrorMessage}");
                Debug.LogError($"[PlayFabDatabase] HTTP Code: {error.HttpCode}");
                callback?.Invoke(false);
            });
    }

    #endregion

    #region Retrieve from Database

    // Get leaderboard from database (highest to lowest)
    public void GetLeaderboard(int maxResults, Action<List<LeaderboardEntry>> callback)
    {
        if (!isConnected)
        {
            Debug.LogError("[PlayFabDatabase] Cannot get leaderboard - not connected!");
            callback?.Invoke(new List<LeaderboardEntry>());
            return;
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = maxResults
        };

        LogDebug($"[PlayFabDatabase] Fetching top {maxResults} entries from database...");

        PlayFabClientAPI.GetLeaderboard(request,
            (result) =>
            {
                LogDebug($"[PlayFabDatabase] Retrieved {result.Leaderboard.Count} entries");
                
                List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
                foreach (var entry in result.Leaderboard)
                {
                    entries.Add(new LeaderboardEntry
                    {
                        rank = entry.Position + 1,
                        playerName = entry.DisplayName ?? "Anonymous",
                        score = entry.StatValue
                    });
                }
                
                callback?.Invoke(entries);
            },
            (error) =>
            {
                Debug.LogError($"[PlayFabDatabase] Failed to get leaderboard: {error.GenerateErrorReport()}");
                callback?.Invoke(new List<LeaderboardEntry>());
            });
    }

    #endregion

    #region Helper Methods

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

    public bool IsConnected()
    {
        // Check both our flag AND PlayFab's actual authentication state
        return isConnected && PlayFabClientAPI.IsClientLoggedIn();
    }

    #endregion
}

#region Data Classes

[System.Serializable]
public class LeaderboardEntry
{
    public int rank;
    public string playerName;
    public int score;
}

#endregion

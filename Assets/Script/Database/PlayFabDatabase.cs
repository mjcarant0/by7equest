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
    [Header("PlayFab Config")]
    [Tooltip("Optional: Set Title ID here to avoid local secrets.")]
    public string titleId;

    // Connection state
    private System.Action connectionSuccessCallback = null;
    private bool isLoginInProgress = false;
    private List<System.Action> pendingConnectionCallbacks = new List<System.Action>();

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
    public void ConnectToDatabase(string playerName, System.Action onSuccess = null)
    {
        // Reset connection state before fresh login to avoid stale cached state
        isConnected = false;
        connectionSuccessCallback = onSuccess;
        if (onSuccess != null)
        {
            pendingConnectionCallbacks.Add(onSuccess);
        }
        LogDebug("[PlayFabDatabase] Resetting connection state for fresh login...");
        
        // If a login is already in progress (common in WebGL iframe), just queue the callback and wait
        if (isLoginInProgress)
        {
            LogDebug("[PlayFabDatabase] Login already in progress; queued callback");
            return;
        }
        
        // Prefer Title ID from inspector override, then secrets file, else existing settings
        if (!string.IsNullOrWhiteSpace(titleId))
        {
            PlayFabSettings.TitleId = titleId.Trim();
            LogDebug("[PlayFabDatabase] Title ID set from inspector override");
        }
        else if (!string.IsNullOrEmpty(PlayFabSecrets.TitleId) && PlayFabSecrets.TitleId != "YOUR_TITLE_ID_HERE")
        {
            PlayFabSettings.TitleId = PlayFabSecrets.TitleId;
            LogDebug("[PlayFabDatabase] Title ID set from PlayFabSecrets");
        }
        else if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            Debug.LogError("[PlayFabDatabase] Title ID not set! Please configure Title ID via inspector or PlayFabSecrets.cs.");
            return;
        }

        // Validate player name
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Anonymous";
        }

        // Always generate a fresh CustomId per connection to create a brand-new account
        string uniqueId = System.Guid.NewGuid().ToString() + "_" + playerName.ToLower().Trim();
        
        LogDebug($"[PlayFabDatabase] Connecting with name: {playerName}");
        
        // On WebGL, clear any stale auth to avoid session confusion inside itch.io iframe
        #if UNITY_WEBGL
        try { PlayFabClientAPI.ForgetAllCredentials(); LogDebug("[PlayFabDatabase] WebGL: cleared credentials before login"); } catch {}
        #endif
        
        var request = new LoginWithCustomIDRequest
        {
            CustomId = uniqueId,
            CreateAccount = true
        };

        LogDebug("[PlayFabDatabase] Connecting to database...");
        isLoginInProgress = true;
        PlayFabClientAPI.LoginWithCustomID(request, OnConnectSuccess, OnConnectFailure);
    }

    private void OnConnectSuccess(LoginResult result)
    {
        isConnected = true;
        isLoginInProgress = false;
        LogDebug($"[PlayFabDatabase] Connected! ID: {result.PlayFabId}");
        
        // Ensure the single callback is included in queued callbacks, then invoke all
        if (connectionSuccessCallback != null)
        {
            pendingConnectionCallbacks.Add(connectionSuccessCallback);
            connectionSuccessCallback = null;
        }
        if (pendingConnectionCallbacks.Count > 0)
        {
            LogDebug($"[PlayFabDatabase] Invoking {pendingConnectionCallbacks.Count} queued success callback(s)...");
            var callbacks = new List<System.Action>(pendingConnectionCallbacks);
            pendingConnectionCallbacks.Clear();
            foreach (var cb in callbacks)
            {
                try { cb?.Invoke(); } catch (Exception ex) { Debug.LogError($"[PlayFabDatabase] Callback threw: {ex.Message}"); }
            }
        }
    }

    private void OnConnectFailure(PlayFabError error)
    {
        isConnected = false;
        isLoginInProgress = false;
        Debug.LogError($"[PlayFabDatabase] Connection failed: {error.GenerateErrorReport()}");
        Debug.LogError($"[PlayFabDatabase] Error: {error.ErrorMessage}");
        Debug.LogError($"[PlayFabDatabase] Title ID used: {PlayFabSettings.TitleId}");
        pendingConnectionCallbacks.Clear();
        
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
        isLoginInProgress = true;
        PlayFabClientAPI.LoginWithCustomID(request, OnConnectSuccess, OnRetryFailure);
    }
    
    private void OnRetryFailure(PlayFabError error)
    {
        isConnected = false;
        isLoginInProgress = false;
        Debug.LogError($"[PlayFabDatabase] Retry also failed: {error.GenerateErrorReport()}");
        Debug.LogError($"[PlayFabDatabase] Please check your Title ID in PlayFab Dashboard");
        pendingConnectionCallbacks.Clear();
    }

    #endregion

    #region Save to Database

    /// Save player name and score to database
    public void SaveToDatabase(string playerName, int score, Action<bool> callback = null)
    {
        Debug.Log("[PlayFabDatabase] === SaveToDatabase CALLED ===");
        Debug.Log($"[PlayFabDatabase] Input: playerName='{playerName}', score={score}");
        
        if (!IsConnected())
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
        Debug.Log("[PlayFabDatabase] Step 1: Updating player name...");
        UpdatePlayerName(playerName, (nameSuccess) =>
        {
            Debug.Log($"[PlayFabDatabase] Step 1 complete: nameSuccess={nameSuccess}");
            if (nameSuccess)
            {
                // Then submit score and backup to PlayerData
                Debug.Log("[PlayFabDatabase] Step 2: Submitting score...");
                SubmitScoreToDatabase(playerName, score, callback);
            }
            else
            {
                // Still try to submit score even if name update failed
                Debug.Log("[PlayFabDatabase] Step 2: Submitting score (despite name update failure)...");
                SubmitScoreToDatabase(playerName, score, callback);
            }
        });
    }

    private void UpdatePlayerName(string playerName, Action<bool> callback)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName
        };

        Debug.Log($"[PlayFabDatabase] [UpdatePlayerName] Sending request with displayName='{playerName}'");

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            (result) =>
            {
                Debug.Log($"[PlayFabDatabase] [UpdatePlayerName] ✓ Success callback invoked!");
                LogDebug($"[PlayFabDatabase] Player name updated successfully");
                callback?.Invoke(true);
            },
            (error) =>
            {
                Debug.LogError($"[PlayFabDatabase] [UpdatePlayerName] ✗ Error callback invoked!");
                Debug.LogError($"[PlayFabDatabase] Failed to update name: {error.GenerateErrorReport()}");
                callback?.Invoke(false);
            });
    }

    private void SubmitScoreToDatabase(string playerName, int score, Action<bool> callback)
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

        Debug.Log($"[PlayFabDatabase] [SubmitScoreToDatabase] Sending score request: StatisticName='HighScore', Value={score}");

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            (result) =>
            {
                Debug.Log("[PlayFabDatabase] [SubmitScoreToDatabase] ✓ UpdatePlayerStatistics success callback invoked!");
                LogDebug("[PlayFabDatabase] ✓ Score saved to database successfully!");
                // Verify stats are present right after saving for debugging
                FetchAndLogPlayerStatistics();
                // Also save to PlayerData as a backup for debugging/analytics
                var dataReq = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        { "PlayerName", playerName },
                        { "HighScore", score.ToString() }
                    }
                };
                Debug.Log($"[PlayFabDatabase] [SubmitScoreToDatabase] Saving PlayerData: PlayerName='{playerName}', HighScore={score}");
                PlayFabClientAPI.UpdateUserData(dataReq,
                    _ => { 
                        Debug.Log("[PlayFabDatabase] [SubmitScoreToDatabase] ✓ UpdateUserData success callback invoked!");
                        Debug.Log("[PlayFabDatabase] PlayerData saved (PlayerName, HighScore)"); 
                    },
                    err => { 
                        Debug.LogError("[PlayFabDatabase] [SubmitScoreToDatabase] ✗ UpdateUserData error callback invoked!");
                        Debug.LogWarning($"[PlayFabDatabase] Failed to save PlayerData: {err.GenerateErrorReport()}"); 
                    });
                callback?.Invoke(true);
            },
            (error) =>
            {
                Debug.LogError("[PlayFabDatabase] [SubmitScoreToDatabase] ✗ UpdatePlayerStatistics error callback invoked!");
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

    private void FetchAndLogPlayerStatistics()
    {
        var req = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { "HighScore" }
        };

        PlayFabClientAPI.GetPlayerStatistics(req,
            res =>
            {
                if (res.Statistics == null || res.Statistics.Count == 0)
                {
                    Debug.LogWarning("[PlayFabDatabase] GetPlayerStatistics returned no entries for HighScore");
                }
                else
                {
                    foreach (var s in res.Statistics)
                    {
                        Debug.Log($"[PlayFabDatabase] Stat fetched → {s.StatisticName} = {s.Value} (ver {s.Version})");
                    }
                }
            },
            err =>
            {
                Debug.LogError($"[PlayFabDatabase] Failed to fetch player statistics: {err.GenerateErrorReport()}");
            });
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

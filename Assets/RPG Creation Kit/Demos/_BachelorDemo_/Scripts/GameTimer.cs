using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Tooltip("Accumulated play time in seconds (counts only while the player is actively playing, not paused or in menus).")]
    [SerializeField] private float totalPlayTimeSeconds = 0f;

    [Tooltip("PlayerPrefs key used to persist the timer between sessions.")]
    [SerializeField] private string playerPrefsKey = "TotalPlayTimeSeconds";

    [Tooltip("If true the timer value will be saved to PlayerPrefs on quit/pause.")]
    [SerializeField] private bool autoSave = true;

    // Expose read-only access
    public float TotalPlayTimeSeconds => totalPlayTimeSeconds;
    public TimeSpan TotalPlayTime => TimeSpan.FromSeconds(totalPlayTimeSeconds);

    private void Awake()
    {
        // Load previously saved value if present
        if (PlayerPrefs.HasKey(playerPrefsKey))
            totalPlayTimeSeconds = PlayerPrefs.GetFloat(playerPrefsKey, 0f);
    }

    private void Update()
    {
        // Only accumulate when GameStatus exists and reports the player is actively playing.
        // Use unscaledDeltaTime so the timer measures real-world play time regardless of Time.timeScale.
        if (RPGCreationKit.GameStatus.instance != null && RPGCreationKit.GameStatus.instance.PlayerPlaying())
        {
            totalPlayTimeSeconds += Time.unscaledDeltaTime;
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (autoSave && pause)
            Save();
    }

    private void OnApplicationQuit()
    {
        if (autoSave)
            Save();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(playerPrefsKey, totalPlayTimeSeconds);
        PlayerPrefs.Save();
    }

    public void ResetTimer(bool saveAfterReset = false)
    {
        totalPlayTimeSeconds = 0f;
        if (saveAfterReset)
            Save();
    }

    // Convenience: formatted "hh:mm:ss"
    public string GetFormattedTime()
    {
        var t = TotalPlayTime;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)t.TotalHours, t.Minutes, t.Seconds);
    }
}


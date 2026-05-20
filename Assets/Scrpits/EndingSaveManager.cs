using UnityEngine;

public static class EndingSaveManager
{
    private const string KEY_ENDING = "GameEndingPlayed";

    public enum EndingType
    {
        None = 0,      // Default / New Game
        Cooperate = 1, // Ending 1: współpraca z demonem
        Defeat = 2     // Ending 2: pokonanie demona
    }

    public static void SaveEnding(EndingType ending)
    {
        PlayerPrefs.SetInt(KEY_ENDING, (int)ending);
        PlayerPrefs.Save();
        Debug.Log($"[EndingSave] 💾 Saved ending: {ending}");
    }

    public static EndingType LoadEnding()
    {
        int value = PlayerPrefs.GetInt(KEY_ENDING, 0);
        return (EndingType)value;
    }

    /// <summary>
    /// ✅ Resetuje zapis – przywraca stan "New Game"
    /// </summary>
    public static void ResetEnding()
    {
        PlayerPrefs.DeleteKey(KEY_ENDING);
        PlayerPrefs.Save();
        Debug.Log("[EndingSave] 🗑️ Ending reset to None (New Game)");
    }
}
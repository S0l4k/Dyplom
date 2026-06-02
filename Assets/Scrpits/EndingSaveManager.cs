using UnityEngine;

public static class EndingSaveManager
{
    private const string KEY_ENDING = "GameEndingPlayed";

    public enum EndingType
    {
        None = 0,      
        Cooperate = 1, 
        Defeat = 2    
    }

    public static void SaveEnding(EndingType ending)
    {
        PlayerPrefs.SetInt(KEY_ENDING, (int)ending);
        PlayerPrefs.Save();
    }

    public static EndingType LoadEnding()
    {
        int value = PlayerPrefs.GetInt(KEY_ENDING, 0);
        return (EndingType)value;
    }


    public static void ResetEnding()
    {
        PlayerPrefs.DeleteKey(KEY_ENDING);
        PlayerPrefs.Save();
    }
}
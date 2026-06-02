using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class QuestConfig
{
    public string questName;
    public string hintMessage;   
    public float timeToHint = 15f; 
    public float repeatInterval = 20f; 
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public GameObject questPanel;
    public GameObject questPrefab;

    [Header("Settings")]
    public List<QuestConfig> questConfigs = new List<QuestConfig>();
    public GameNarrativeManager narrativeManager;

    private Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();

    private bool isThoughtActive = false;
    private float lastThoughtEndTime = 0f;

    private class QuestData
    {
        public string name;
        public GameObject uiObject;
        public float startTime;
        public float lastHintTime;
        public QuestConfig config;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (narrativeManager == null) narrativeManager = FindObjectOfType<GameNarrativeManager>();
        if (narrativeManager == null) return;

        if (GameState.IsTalking) return;
        if (GameState.IsTalking || GameState.IsInFlashback) return;
        float currentTime = Time.time;

        foreach (var kvp in activeQuests.Values)
        {
            float timeSinceStart = currentTime - kvp.startTime;
            float timeSinceLastHint = currentTime - kvp.lastHintTime;

            bool shouldShowHint = false;

            if (timeSinceStart >= kvp.config.timeToHint && kvp.lastHintTime == kvp.startTime)
            {
                shouldShowHint = true;
            }

            else if (timeSinceLastHint >= kvp.config.repeatInterval && kvp.lastHintTime != kvp.startTime)
            {
                shouldShowHint = true;
            }

            if (shouldShowHint)
            {
                TryShowHint(kvp.config.hintMessage);
            }
        }
    }

    private void TryShowHint(string message)
    {
        if (GameState.IsTalking) return;
        if (GameState.IsInspecting || GameState.IsInFlashback)
            return;

        if (narrativeManager != null && narrativeManager.IsNarrativeBusy())
            return;

        if (isThoughtActive) return;

        if (Time.time - lastThoughtEndTime < 2.0f) return;

        isThoughtActive = true;
        StartCoroutine(RunHintSequence(message));
    }

    private IEnumerator RunHintSequence(string message)
    {
        if (narrativeManager != null)
        {
            yield return StartCoroutine(narrativeManager.ShowThought(message, 0.05f, 4.0f));
        }

        isThoughtActive = false;
        lastThoughtEndTime = Time.time;
        foreach (var kvp in activeQuests.Values)
        {
            kvp.lastHintTime = Time.time;
        }
    }

    public void AddQuest(string questName)
    {
        if (activeQuests.ContainsKey(questName)) return;

        QuestConfig config = questConfigs.Find(c => c.questName == questName);

        if (config == null)
        {
            config = new QuestConfig
            {
                questName = questName,
                hintMessage = $"I need to: {questName}",
                timeToHint = 10f,
                repeatInterval = 20f
            };
        }

        GameObject questObj = Instantiate(questPrefab, questPanel.transform);
        questObj.GetComponentInChildren<TMPro.TMP_Text>().text = questName;

        QuestData data = new QuestData
        {
            name = questName,
            uiObject = questObj,
            startTime = Time.time,
            lastHintTime = Time.time,
            config = config
        };

        activeQuests.Add(questName, data);
    }

    public void CompleteQuest(string questName)
    {
        if (!activeQuests.ContainsKey(questName)) return;

        Destroy(activeQuests[questName].uiObject);
        activeQuests.Remove(questName);      
    }
    public void ResetAllQuests()
    {
        ClearAllQuests();
    }

    public void ClearAllQuests()
    {
        foreach (var kvp in activeQuests)
        {
            if (kvp.Value.uiObject != null) Destroy(kvp.Value.uiObject);
        }
        activeQuests.Clear();
    }

}
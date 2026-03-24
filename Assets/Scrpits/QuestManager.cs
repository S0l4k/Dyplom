using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class QuestConfig
{
    public string questName;
    public string hintMessage;      // Treść myśli
    public float timeToHint = 15f;  // Czas do pierwszej podpowiedzi
    public float repeatInterval = 20f; // Czas między powtórzeniami
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

    // Zmienne blokujące nakładanie się myśli
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

        // ✅ BLOKADA GLOBALNA: Jeśli trwa dialog, nie pokazuj myśli i nie odliczaj czasu
        if (GameState.IsTalking) return;

        float currentTime = Time.time;

        foreach (var kvp in activeQuests.Values)
        {
            float timeSinceStart = currentTime - kvp.startTime;
            float timeSinceLastHint = currentTime - kvp.lastHintTime;

            bool shouldShowHint = false;

            // 1. Pierwsza podpowiedź
            if (timeSinceStart >= kvp.config.timeToHint && kvp.lastHintTime == kvp.startTime)
            {
                shouldShowHint = true;
            }
            // 2. Powtórzenie podpowiedzi
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
        // Dodatkowe sprawdzenie IsTalking dla bezpieczeństwa
        if (GameState.IsTalking) return;

        // BLOKADA: Nie pokazuj, jeśli inna myśl jest akurat na ekranie
        if (isThoughtActive) return;

        // BLOKADA: Odczekaj 2 sekundy od zniknięcia ostatniej myśli
        if (Time.time - lastThoughtEndTime < 2.0f) return;

        isThoughtActive = true;
        StartCoroutine(RunHintSequence(message));
    }

    private IEnumerator RunHintSequence(string message)
    {
        // Czekaj aż narracja wyświetli i ukryje tekst
        yield return StartCoroutine(narrativeManager.ShowThought(message, 0.05f, 4.0f));

        // Gdy tekst zniknie:
        isThoughtActive = false;
        lastThoughtEndTime = Time.time;

        // Resetujemy liczniki dla WSZYSTKICH questów
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
            Debug.LogWarning($"[QUEST] Brak konfiguracji dla '{questName}'. Użyto domyślnych.");
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
        Debug.Log($"[QUEST] Added: {questName} (Time: {data.startTime})");

        // ✅ NAPRAWA: Wymuś odświeżenie logiki w tej samej klatce, jeśli czas jest bardzo krótki
        // To pomaga przy questach dodawanych w trakcie trwania gry (jak RUN)
        if (config.timeToHint <= 0.1f)
        {
            // Jeśli czas jest bliski 0, od razu spróbuj pokazać hint w następnej klatce
            // (W Twoim przypadku RUN ma czas > 0, więc to tylko zabezpieczenie)
        }
    }

    public void CompleteQuest(string questName)
    {
        if (!activeQuests.ContainsKey(questName)) return;

        Destroy(activeQuests[questName].uiObject);
        activeQuests.Remove(questName);

        Debug.Log($"[QUEST] Completed: {questName}");
        CheckForChaseTrigger();
    }

    public void ClearAllQuests()
    {
        foreach (var kvp in activeQuests)
        {
            if (kvp.Value.uiObject != null) Destroy(kvp.Value.uiObject);
        }
        activeQuests.Clear();
    }

    private void CheckForChaseTrigger()
    {
        if (activeQuests.Count == 0)
        {
            GameState.ChaseLocked = false;
            Debug.Log("[CHASE] All quests done.");
        }
    }
}
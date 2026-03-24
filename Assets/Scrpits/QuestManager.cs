using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public GameObject questPanel;
    public GameObject questPrefab;

    private Dictionary<string, GameObject> activeQuests = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        
      
    }

    public void AddQuest(string questName)
    {
        if (activeQuests.ContainsKey(questName))
            return;

        GameObject questObj = Instantiate(questPrefab, questPanel.transform);
        questObj.GetComponentInChildren<TMPro.TMP_Text>().text = questName;

        activeQuests.Add(questName, questObj);

        Debug.Log($"[QUEST] Dodano quest: {questName}");
    }

    public void CompleteQuest(string questName)
    {
        if (!activeQuests.ContainsKey(questName))
            return;

        Destroy(activeQuests[questName]);
        activeQuests.Remove(questName);

        Debug.Log($"[QUEST] Ukończono quest: {questName}");

        CheckForChaseTrigger();
    }
    public void ClearAllQuests()
    {
        foreach (var kvp in activeQuests)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        activeQuests.Clear();
        Debug.Log("[QuestManager] Wszystkie questy wyczyszczone");
    }
    private void CheckForChaseTrigger()
    {
        if (activeQuests.Count == 0)
        {
            GameState.ChaseLocked = false;
            Debug.Log("[CHASE] Wszystkie questy ukończone – chase odblokowany!");
        }
    }
}

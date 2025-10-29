using UnityEngine;
using TMPro;

public class Dialog : MonoBehaviour
{
    public DialogNode[] dialogNodes;
    public GameObject[] answers;
    public TextMeshProUGUI dialogText;

    private int currentNode = 0;
    private bool optionsActive = false;

    public bool happy = false;

    void Awake()
    {
        HideAll();
    }

    void Update()
    {
        if (!optionsActive) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectOption(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectOption(1);
    }

    public void StartDialog()
    {
        currentNode = 0;
        ShowNode();
    }

    void ShowNode()
    {
        HideAll();

        DialogNode node = dialogNodes[currentNode];

        dialogText.text = node.npcLine;
        dialogText.gameObject.SetActive(true);

        for (int i = 0; i < node.responses.Length; i++)
        {
            answers[i].SetActive(true);
            answers[i].GetComponent<TMP_Text>().text = node.responses[i];
        }

        optionsActive = true;
    }

    void SelectOption(int optionIndex)
    {
        DialogNode node = dialogNodes[currentNode];

        if (node.responseEvents != null &&
            optionIndex < node.responseEvents.Length &&
            node.responseEvents[optionIndex] != null)
        {
            node.responseEvents[optionIndex].Invoke();
        }

        if (node.nextNodeIndex.Length <= optionIndex ||
            node.nextNodeIndex[optionIndex] < 0)
        {
            EndDialog();
            return;
        }

        currentNode = node.nextNodeIndex[optionIndex];
        ShowNode();
    }

    public void Happiness()
    {
        happy = true;
        Debug.Log("Krzysiu jest zadowolony!");
    }

    void EndDialog()
    {
        HideAll();
        Debug.Log("Dialog zakoñczony");
    }

    void HideAll()
    {
        dialogText.gameObject.SetActive(false);

        for (int i = 0; i < answers.Length; i++)
            answers[i].SetActive(false);

        optionsActive = false;
    }
}
using UnityEngine;
using TMPro;

public class Dialog : MonoBehaviour
{
    [SerializeField] private string[] sentences1;
    [SerializeField] private string[] sentences2;
    public GameObject[] answers;
    public TextMeshProUGUI dialogText;

    void Awake()
    {
        Debug.Log("Dialog Awake start");

        for (int i = 0; i < answers.Length; i++)
        {
            Debug.Log($"Wy³¹czam: {answers[i].name}");
            answers[i].SetActive(false);
        }

        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(false);
            Debug.Log("Ukry³em dialogText");
        }

        Debug.Log("Dialog Awake koniec");
    }

    void Start()
    {
    }

    public void ShowAnswers()
    {
        for (int i = 0; i < answers.Length; i++)
            answers[i].SetActive(true);

        if (dialogText != null)
            dialogText.gameObject.SetActive(false);
    }

    public void DialogOption1()
    {
        for (int i = 0; i < answers.Length; i++)
            answers[i].SetActive(false);

        dialogText.gameObject.SetActive(true);
        dialogText.text = sentences1.Length > 0 ? sentences1[0] : "";
    }

    public void DialogOption2()
    {
        for (int i = 0; i < answers.Length; i++)
            answers[i].SetActive(false);

        dialogText.gameObject.SetActive(true);
        dialogText.text = sentences2.Length > 0 ? sentences2[0] : "";
    }
}
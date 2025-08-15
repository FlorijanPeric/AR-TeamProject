using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyMenuHandler : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }
    public static Difficulty SelectedDifficulty { get; private set; }

    public ScriptsForGame gameScript;
    public GameObject uiPrefab;
    public Transform uiParent;

    private GameObject currentPopup;

    void Start()
    {
        if (gameScript == null)
        {
            gameScript = FindObjectOfType<ScriptsForGame>();
        }
        ShowDifficultyMenu();
    }

    // Ta nova metoda se lahko pokliče z gumba "Restart"
    public void ShowRestartMenu()
    {
        ShowDifficultyMenu();
    }

    public void ShowDifficultyMenu()
    {
        if (currentPopup != null) return;
        Debug.Log("Prikazujem meni težavnosti.");

        currentPopup = Instantiate(uiPrefab, uiParent);
        currentPopup.transform.localScale = Vector3.one;
        currentPopup.transform.localPosition = new Vector3(0f, 0f, 0f);

        // Sedaj iskanje gumbov po tagih
        foreach (Button btn in currentPopup.GetComponentsInChildren<Button>(true))
        {
            btn.onClick.RemoveAllListeners();

            if (btn.CompareTag("Easy"))
            {
                btn.onClick.AddListener(() => {
                    SetDifficultyAndStart(Difficulty.Easy);
                });
            }
            else if (btn.CompareTag("Medium"))
            {
                btn.onClick.AddListener(() => {
                    SetDifficultyAndStart(Difficulty.Medium);
                });
            }
            else if (btn.CompareTag("Hard"))
            {
                btn.onClick.AddListener(() => {
                    SetDifficultyAndStart(Difficulty.Hard);
                });
            }
        }
    }

    private void SetDifficultyAndStart(Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;
        Debug.Log("Izbrana težavnost: " + SelectedDifficulty + ". Začenjam igro.");

        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }

        if (gameScript != null)
        {
            gameScript.resetOther();
        }
        else
        {
            Debug.LogError("ScriptsForGame ni nastavljen!");
        }
    }
}
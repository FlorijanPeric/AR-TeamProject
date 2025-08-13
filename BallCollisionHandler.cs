//BallCollisionHandler.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallCollisionHandler : MonoBehaviour
{
    public GameObject scripts;
    
    private GameObject currentPopup;
    public GameObject uiPrefab;     // assign the UI prefab here
    public Transform uiParent;
  
    
    void Start(){
    }
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other)
    {
        ScriptsForGame gameScript=scripts.GetComponent<ScriptsForGame>();
        if (other.CompareTag("Goalie"))
        {
            gameScript.StopAllCoroutines();
            Debug.Log("Blocked by goalie!");
            
                currentPopup = Instantiate(uiPrefab, uiParent);
                currentPopup.transform.localScale = Vector3.one* 0.5f; // keeps intended size
                currentPopup.transform.localPosition = Vector3.zero;
                Button targetButton = null;
                Button target2=null;
               // var btn = currentPopup.GetComponentInChildren<Button>();
            foreach (var btn in currentPopup.GetComponentsInChildren<Button>())
        {
            if (btn.CompareTag("Restart"))  // use your tag here
            {
                targetButton = btn;
                
            }else{
                target2=btn;
            }
        }
        targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(onRestart);

            var popupInstance = currentPopup; 

            target2.onClick.RemoveAllListeners();
            target2.onClick.AddListener(() => {
                Destroy(popupInstance); 
                currentPopup = null;
                gameScript.resetOther();
            });

        }
        else if(other.CompareTag("Goal")){
            gameScript.OnGoalScored();
            Debug.Log("Hit the goal");

        }
    }
    public void onRestart(){
        ScriptsForGame gameScript=scripts.GetComponent<ScriptsForGame>();

        //gameScript=FindObjectOfType<ScriptsForGame>();
        Destroy(currentPopup);
        currentPopup=null;
        gameScript.reset();
    }
}

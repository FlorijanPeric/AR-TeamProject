using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ScriptsForGame : MonoBehaviour
{
    // UI element za prikaz rezultata
    public TextMeshProUGUI scoreUi;

    // Referenca na žogo
    public GameObject soccerBall;
    public Transform pillarL;
    public Transform pillarR;
    public GameObject confetR;
    public GameObject confetY;
    public GameObject confetB;
    public GameObject[] spawnPrefabs;

    // Moč strela (večja = hitrejši strel)
    public float swipeForce;

    // Referenca na transformacijo vratarja
    public Transform goalieTransform;
    // Animator vratarja
    public Animator goalieAnimator;

    // SPREMENLJIVKE ZA NOVO LOGIKO IN IGRO
    public Transform goalLine; // Referenca na ravnino golove črte
    private Vector3 goalieStartPos;
    private Vector3 goalieTargetPos;
    private float goalieMoveSpeed;
    private float goalieReactionTime; 
    private float goalieAccuracyOffset; 
    private bool shouldMoveGoalie = false;
    private Vector3 startPos;
    private bool isMoving = false;
    private Vector2 swipeStart;
    private Vector2 swipeEnd;
    private Camera mainCamera;
    private Rigidbody ballRb;
    public int score = 0;

    void Awake()
    {
        mainCamera = Camera.main;
        ballRb = soccerBall.GetComponent<Rigidbody>();
        
        if (goalieAnimator == null && goalieTransform != null)
        {
            goalieAnimator = goalieTransform.GetComponent<Animator>();
            if (goalieAnimator == null)
            {
                Debug.LogWarning("Animator na vratarju ni najden!");
            }
        }
        
        if (goalieAnimator != null)
        {
            goalieAnimator.SetTrigger("Idle");
        }
        
        if (goalieTransform != null)
        {
            goalieStartPos = goalieTransform.position;
        }
    }

    void Start()
    {
        startPos = soccerBall.transform.position;
    }

    void Update()
    {
        if (shouldMoveGoalie)
        {
            goalieTransform.position = Vector3.Lerp(goalieTransform.position, goalieTargetPos, goalieMoveSpeed * Time.deltaTime);

            if (Vector3.Distance(goalieTransform.position, goalieTargetPos) < 0.1f)
            {
                shouldMoveGoalie = false;
            }
        }

#if UNITY_EDITOR
        if (Mouse.current.leftButton.wasPressedThisFrame)
            swipeStart = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            swipeEnd = Mouse.current.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd);
        }
#else
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            swipeStart = Touchscreen.current.primaryTouch.position.ReadValue();
        if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            swipeEnd = Touchscreen.current.primaryTouch.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd);
        }
#endif
    }

    void TrySwipe(Vector2 start, Vector2 end)
    {
        Ray rayStart = mainCamera.ScreenPointToRay(start);
        Ray rayEnd = mainCamera.ScreenPointToRay(end);

        if (Physics.Raycast(rayStart, out RaycastHit hitStart) &&
            Physics.Raycast(rayEnd, out RaycastHit hitEnd))
        {
            if (hitStart.collider.gameObject.name == soccerBall.name)
            {
                swipeForce = Random.Range(20.0f, 50.0f);
                Vector3 dir = (hitEnd.point - hitStart.point).normalized;

                ballRb.velocity = Vector3.zero;
                ballRb.AddForce(dir * swipeForce, ForceMode.Impulse);

                TriggerGoalieAnimation(dir);
                StartCoroutine(MoveGoalieWithDelay(dir));
                
                isMoving = true;
            }
            else
            {
                Debug.Log("Klik ni bil na žogo, ampak na: " + hitStart.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Raycast ni zadel ničesar.");
        }
    }

    private void TriggerGoalieAnimation(Vector3 direction)
    {
        if (goalieAnimator == null) return;

        goalieAnimator.ResetTrigger("DiveLeft");
        goalieAnimator.ResetTrigger("DiveRight");
        goalieAnimator.ResetTrigger("DiveUp");
        goalieAnimator.ResetTrigger("Idle");

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                goalieAnimator.SetTrigger("DiveRight");
            else
                goalieAnimator.SetTrigger("DiveLeft");
        }
        else
        {
            goalieAnimator.SetTrigger("DiveUp");
        }
    }

    IEnumerator MoveGoalieWithDelay(Vector3 direction)
    {
        switch (DifficultyMenuHandler.SelectedDifficulty)
        {
            case DifficultyMenuHandler.Difficulty.Easy:
                goalieMoveSpeed = 2f;
                goalieReactionTime = 0.5f; 
                goalieAccuracyOffset = 1.0f; 
                break;
            case DifficultyMenuHandler.Difficulty.Medium:
                goalieMoveSpeed = 4f;
                goalieReactionTime = 0.25f; 
                goalieAccuracyOffset = 0.5f; 
                break;
            case DifficultyMenuHandler.Difficulty.Hard:
                goalieMoveSpeed = 6f;
                goalieReactionTime = 0f; 
                goalieAccuracyOffset = 0f; 
                break;
            default:
                goalieMoveSpeed = 3f;
                goalieReactionTime = 0f;
                goalieAccuracyOffset = 0f;
                break;
        }

        yield return new WaitForSeconds(goalieReactionTime);

        RaycastHit hit;
        if (Physics.Raycast(soccerBall.transform.position, ballRb.velocity.normalized, out hit))
        {
            Vector3 target = hit.point;
            target.x += Random.Range(-goalieAccuracyOffset, goalieAccuracyOffset);
            target.y += Random.Range(-goalieAccuracyOffset, goalieAccuracyOffset);
            goalieTargetPos = new Vector3(target.x, goalieStartPos.y + (target.y - goalLine.position.y), goalieStartPos.z);
        }
        else
        {
            Vector3 target = soccerBall.transform.position + ballRb.velocity.normalized * 50f;
            target.x += Random.Range(-goalieAccuracyOffset, goalieAccuracyOffset);
            target.y += Random.Range(-goalieAccuracyOffset, goalieAccuracyOffset);
            goalieTargetPos = new Vector3(target.x, goalieStartPos.y + (target.y - goalLine.position.y), goalieStartPos.z);
        }

        shouldMoveGoalie = true;
    }

    // DODANA MANJKAJOČA METODA ZA PONASTAVITEV
    public void resetOther()
    {
        ResetBall();
        if (goalieTransform != null)
        {
            goalieTransform.position = goalieStartPos;
        }
        if (goalieAnimator != null)
        {
            goalieAnimator.SetTrigger("Idle");
        }
    }

    // DODANA MANJKAJOČA METODA ZA ZABIT GOL
    public void OnGoalScored()
    {
        score++;
        scoreUi.text = "Score: " + score.ToString(); // POPRAVLJENO: Dodan "Score: "
        resetOther(); // POPRAVLJENO: Kličemo resetOther, ki ponastavi žogo in golmana


        
        //popup ne dela s kofeti!!! 
        GoHardLeft();
        GoHardRight();
        
    }

    // DODANA MANJKAJOČA METODA ZA PONASTAVITEV
    public void reset()
    {
        resetOther();
        score = 0;
        scoreUi.text = "Score: 0";
    }

    // Skupna logika za ponastavitev žoge
    /*void ResetBall()
    {
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
        shouldMoveGoalie = false;
    }
    */
    void ResetBall()
    {
        // Disable physics temporarily
        ballRb.isKinematic = true;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Reset position & rotation
        soccerBall.transform.position = startPos;
        soccerBall.transform.rotation = Quaternion.identity;

        // Re-enable physics
        ballRb.isKinematic = false;

        isMoving = false;
        shouldMoveGoalie = false;
    }


    // DODANI MANJKAJOČI TESTNI METODI ZA AR
    public void GoHardLeft()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarL.position + Vector3.up * 33f;
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation);
            obj.AddComponent<FunnyFloat>(); // Komentirano, ker "FunnyFloat" morda ne obstaja
        }
        
    }

    public void GoHardRight()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarR.position + Vector3.up * 33f;
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation);
            obj.AddComponent<FunnyFloat>(); // Komentirano, ker "FunnyFloat" morda ne obstaja
        }
    }

    public void stopBall()
    {
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
        shouldMoveGoalie = false;
    }
}

/*
SPREMENI GOAL LINE ČE SE VRNES NA STARO VERZIJO
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class ScriptsForGame : MonoBehaviour
{
    // UI element za prikaz rezultata
    public TextMeshProUGUI scoreUi;

    // Referenca na žogo
    public GameObject soccerBall;
    public Transform pillarL;
    public Transform pillarR;
    public GameObject confetR;
    public GameObject confetY;
    public GameObject confetB;
    public GameObject[] spawnPrefabs;

    // Moč strela (večja = hitrejši strel)
    public float swipeForce;

    // Referenca na transformacijo vratarja (ni nujno, če ne uporabljaš)
    public Transform goalieTransform;
    // Animator vratarja (za animacije DiveLeft, DiveRight, DiveUp, Idle)
    public Animator goalieAnimator;

    // Dodane spremenljivke za gibanje vratarja
    private Vector3 goalieStartPos;
    private Vector3 goalieTargetPos;
    private float goalieMoveSpeed;
    private bool shouldMoveGoalie = false;

    // Začetna pozicija žoge (da jo lahko resetiramo)
    private Vector3 startPos;
    // Ali je žoga trenutno v gibanju
    private bool isMoving = false;
    // Za zaznavanje swipe-a (dotik ali miš)
    private Vector2 swipeStart;
    private Vector2 swipeEnd;
    // Kamera (za raycast)
    private Camera mainCamera;
    // Rigidbody žoge
    private Rigidbody ballRb;
    // Rezultat
    public int score = 0;

    void Awake()
    {
        mainCamera = Camera.main;
        ballRb = soccerBall.GetComponent<Rigidbody>();
        
        if (goalieAnimator == null && goalieTransform != null)
        {
            goalieAnimator = goalieTransform.GetComponent<Animator>();
            if (goalieAnimator == null)
            {
                Debug.LogWarning("Animator na vratarju ni najden!");
            }
        }
        
        if (goalieAnimator != null)
        {
            goalieAnimator.Play("Idle");
        }
        
        // Shranimo začetno pozicijo golmana
        if (goalieTransform != null)
        {
            goalieStartPos = goalieTransform.position;
        }
    }


    void Start()
    {
        startPos = soccerBall.transform.position;
    }

    void Update()
    {
        // Gibanje golmana
        if (shouldMoveGoalie)
        {
            // Golmana premikamo proti ciljni poziciji
            goalieTransform.position = Vector3.Lerp(goalieTransform.position, goalieTargetPos, goalieMoveSpeed * Time.deltaTime);

            // Ko je dovolj blizu, prenehamo z gibanjem
            if (Vector3.Distance(goalieTransform.position, goalieTargetPos) < 0.1f)
            {
                shouldMoveGoalie = false;
            }
        }

#if UNITY_EDITOR
        if (Mouse.current.leftButton.wasPressedThisFrame)
            swipeStart = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            swipeEnd = Mouse.current.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd);
        }
#else
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            swipeStart = Touchscreen.current.primaryTouch.position.ReadValue();
        if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            swipeEnd = Touchscreen.current.primaryTouch.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd);
        }
#endif
    }

    void TrySwipe(Vector2 start, Vector2 end)
    {
        Ray rayStart = mainCamera.ScreenPointToRay(start);
        Ray rayEnd = mainCamera.ScreenPointToRay(end);

        if (Physics.Raycast(rayStart, out RaycastHit hitStart) &&
            Physics.Raycast(rayEnd, out RaycastHit hitEnd))
        {
            if (hitStart.collider.gameObject == soccerBall)
            {
                swipeForce = Random.Range(20.0f,50.0f);
                Vector3 dir = (hitEnd.point - hitStart.point).normalized;

                //debug
                Debug.Log("Smer strela: " + dir); 

                ballRb.velocity = Vector3.zero;
                ballRb.AddForce(dir * swipeForce, ForceMode.Impulse);

                TriggerGoalieAnimation(dir);

                // Določimo in začnemo premikati golmana
                MoveGoalieToIntercept(dir);
                
                isMoving = true;
            }
            else
            {
                Debug.Log("Klik ni bil na žogo, ampak na: " + hitStart.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Raycast ni zadel ničesar.");
        }
    }

    // Izračuna ciljno pozicijo in nastavi premikanje vratarja
    void MoveGoalieToIntercept(Vector3 direction)
    {
        // Izračunamo približno ciljno pozicijo v golu
        // Na podlagi smeri strela (direction.x in direction.y)
        float goalieX = direction.x * 2.0f; // Večja vrednost = premik dlje
        float goalieY = direction.y * 1.5f; // Večja vrednost = premik navzgor
        
        goalieTargetPos = new Vector3(goalieStartPos.x + goalieX, goalieStartPos.y + goalieY, goalieStartPos.z);
        
        // Prilagodimo hitrost glede na izbrano težavnost
        switch (DifficultyMenuHandler.SelectedDifficulty)
        {
            case DifficultyMenuHandler.Difficulty.Easy:
                goalieMoveSpeed = 2f;
                break;
            case DifficultyMenuHandler.Difficulty.Medium:
                goalieMoveSpeed = 4f;
                break;
            case DifficultyMenuHandler.Difficulty.Hard:
                goalieMoveSpeed = 6f;
                break;
            default:
                goalieMoveSpeed = 3f;
                break;
        }

        //debug
        Debug.Log("Ciljna pozicija golmana: " + goalieTargetPos);
        shouldMoveGoalie = true;
    }


    void TriggerGoalieAnimation(Vector3 direction)
    {
        if (goalieAnimator == null) return;

        goalieAnimator.ResetTrigger("DiveRight");
        goalieAnimator.ResetTrigger("DiveLeft");
        goalieAnimator.ResetTrigger("DiveUp");
        
        if (Mathf.Abs(direction.x) > 0.3f && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                goalieAnimator.SetTrigger("DiveRight");
            }
            else
            {
                goalieAnimator.SetTrigger("DiveLeft");
            }
        }
        else if (direction.y > 0.3f)
        {
            goalieAnimator.SetTrigger("DiveUp");
        }
        else
        {
            goalieAnimator.SetTrigger("Idle");
        }
    }

    public void OnGoalScored()
    {
        score++;
        scoreUi.text = "Score: " + score;
        ResetBall();
        GoHardLeft();
        GoHardRight();
        
        if (goalieAnimator != null)
            goalieAnimator.Play("Idle");
    }

    public void GoHardLeft()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarL.position + Vector3.up * 33f;
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation);
            obj.AddComponent<FunnyFloat>();
        }
    }

    public void GoHardRight()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarR.position + Vector3.up * 33f;
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation);
            obj.AddComponent<FunnyFloat>();
        }
    }

    public void resetOther()
    {
        ResetBall();
        
        if (goalieAnimator != null)
            goalieAnimator.Play("Idle");
            
        // Resetiramo pozicijo golmana
        if (goalieTransform != null)
        {
            goalieTransform.position = goalieStartPos;
        }
    }

    public void reset()
    {
        ResetBall();
        score = 0;
        scoreUi.text = "Score: 0";
        
        if (goalieAnimator != null)
            goalieAnimator.Play("Idle");
            
        // Resetiramo pozicijo golmana
        if (goalieTransform != null)
        {
            goalieTransform.position = goalieStartPos;
        }
    }

    void ResetBall()
    {
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
        shouldMoveGoalie = false;
    }

    public void stopBall(){
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
        shouldMoveGoalie = false;
    }
}

/*ScriptsForGame
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class ScriptsForGame : MonoBehaviour
{
    // UI element za prikaz rezultata
    public TextMeshProUGUI scoreUi;

    // Referenca na žogo
    //tle so tudi gameObjecti za spawnanje kinda konfetov
    public GameObject soccerBall;
    //First pillars
    public Transform pillarL;
    public Transform pillarR;
    //second Confetis
    public GameObject confetR;
    public GameObject confetY;
    public GameObject confetB;
    public GameObject[] spawnPrefabs;


    // Moč strela (večja = hitrejši strel)
    public float swipeForce;

    // Referenca na transformacijo vratarja (ni nujno, če ne uporabljaš)
    public Transform goalieTransform;

    // Animator vratarja (za animacije DiveLeft, DiveRight, DiveUp, Idle)
    public Animator goalieAnimator;

    // Začetna pozicija žoge (da jo lahko resetiramo)
    private Vector3 startPos;

    // Ali je žoga trenutno v gibanju
    private bool isMoving = false;

    // Za zaznavanje swipe-a (dotik ali miš)
    private Vector2 swipeStart;
    private Vector2 swipeEnd;

    // Kamera (za raycast)
    private Camera mainCamera;

    // Rigidbody žoge
    private Rigidbody ballRb;

    // Rezultat
    public int score = 0;

    void Awake()
    {
        // Pridobi kamero in Rigidbody žoge
        mainCamera = Camera.main;
        ballRb = soccerBall.GetComponent<Rigidbody>();

        // Če animator ni ročno nastavljen, ga poišči pri goalieTransform
        if (goalieAnimator == null && goalieTransform != null)
        {
            goalieAnimator = goalieTransform.GetComponent<Animator>();
            if (goalieAnimator == null)
            {
                Debug.LogWarning("Animator na vratarju ni najden!");
            }
        }

        // Vratar čaka v "Idle" stanju
        if (goalieAnimator != null)
        {
            goalieAnimator.Play("Idle");
        }
    }


    void Start()
    {
        // Shrani začetno pozicijo žoge
        startPos = soccerBall.transform.position;
    }

    void Update()
    {
#if UNITY_EDITOR
        // V editorju zaznaj klik z miško
        if (Mouse.current.leftButton.wasPressedThisFrame)
            swipeStart = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            swipeEnd = Mouse.current.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd); // Poizkusi strel
        }
#else
        // Na telefonu zaznaj dotik
        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            swipeStart = Touchscreen.current.primaryTouch.position.ReadValue();
        if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            swipeEnd = Touchscreen.current.primaryTouch.position.ReadValue();
            TrySwipe(swipeStart, swipeEnd); // Poizkusi strel
        }
#endif
    }

    // Obdelava swipe gesta
    void TrySwipe(Vector2 start, Vector2 end)
    {
        Ray rayStart = mainCamera.ScreenPointToRay(start);
        Ray rayEnd = mainCamera.ScreenPointToRay(end);

        if (Physics.Raycast(rayStart, out RaycastHit hitStart) &&
            Physics.Raycast(rayEnd, out RaycastHit hitEnd))
        {
            if (hitStart.collider.gameObject == soccerBall)
            {
                swipeForce = Random.Range(20.0f,50.0f);
                Vector3 dir = (hitEnd.point - hitStart.point).normalized;

                ballRb.velocity = Vector3.zero;
                ballRb.AddForce(dir * swipeForce, ForceMode.Impulse);

                // Sproži animacijo vratarja glede na smer
                TriggerGoalieAnimation(dir);

                isMoving = true;
            }
            else
            {
                Debug.Log("Klik ni bil na žogo, ampak na: " + hitStart.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Raycast ni zadel ničesar.");
        }
    }



    // Izberi pravo animacijo vratarja glede na smer strela
    void TriggerGoalieAnimation(Vector3 direction)
    {
        if (goalieAnimator == null) return;

        int anim = Random.Range(2,4);
        if(isMoving == true){
            if(anim == 1){
                goalieAnimator.SetTrigger("DiveRight");
                goalieAnimator.ResetTrigger("DiveLeft");
                goalieAnimator.ResetTrigger("DiveUp");

            }
            if(anim == 2){
                goalieAnimator.SetTrigger("DiveLeft");
                goalieAnimator.ResetTrigger("DiveRight");
                goalieAnimator.ResetTrigger("DiveUp");
            }
            if(anim == 3){
                goalieAnimator.SetTrigger("DiveUp");
                goalieAnimator.ResetTrigger("DiveLeft");
                goalieAnimator.ResetTrigger("DiveRight");
            }
        }
        else {
            goalieAnimator.SetTrigger("Idle");
        }
         // Primerjaj, ali je swipe bolj horizontalen ali vertikalen
        /*if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // Horizontalni strel → levo ali desno
            if (direction.x > 0)
                goalieAnimator.SetTrigger("DiveRight");
            else
                goalieAnimator.SetTrigger("DiveLeft");
        }
        else
        {
            // Vertikalni strel → naprej
            goalieAnimator.SetTrigger("DiveUp");
        }
        ------------------------------!!!
    }

    // Ko damo gol (kliče se iz druge skripte, npr. BallCollision)
    public void OnGoalScored()
    {

        score++;
        scoreUi.text = "Score: " + score;
        ResetBall();
        GoHardLeft();
        GoHardRight();

        if (goalieAnimator != null)
            goalieAnimator.Play("Idle");
    }
    public void GoHardLeft()
    {
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarL.position + Vector3.up * 33f; // 1.5 units above the pillar
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation); // Random rotation
            obj.AddComponent<FunnyFloat>(); // Add the floating/rotation logic
        }
    }
    public void GoHardRight()
    {
    for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Vector3 spawnPos = pillarR.position + Vector3.up * 33f; // 1.5 units above the pillar
            GameObject obj = Instantiate(spawnPrefabs[randomIndex], spawnPos, Random.rotation); // Random rotation
            obj.AddComponent<FunnyFloat>(); // Add the floating/rotation logic
        }
    }
    private void DoTransform() {
        
    }

    // Reset po obrambi ali mimo gola
    public void resetOther()
    {
        ResetBall();

        if (goalieAnimator != null)
            goalieAnimator.Play("Jumping");
    }

    // Ročni reset igre (npr. gumb)
    public void reset()
    {
        ResetBall();
        score = 0;
        scoreUi.text = "Score: 0";

        if (goalieAnimator != null)
            goalieAnimator.Play("Jumping");
    }

    // Skupna logika za ponastavitev žoge
    void ResetBall()
    {
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
    }

    public void stopBall(){
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
    }
}
*/
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

    // Animator vratarja (za animacije DiveLeft, DiveRight, DiveUp, Jumping)
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

        // Vratar čaka v "Jumping" stanju
        if (goalieAnimator != null)
        {
            goalieAnimator.Play("Jumping");
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
        */
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
            goalieAnimator.Play("Jumping");
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
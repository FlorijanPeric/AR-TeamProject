/*using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;


public class ScriptsForGame : MonoBehaviour
{
    public TextMeshProUGUI scoreUi;

    public GameObject soccerBall;
    public Transform goalTransform;
    public float moveSpeed = 20f; // Increase speed for fast movement

    private Vector3 startPos;
    private bool isMoving = false;
    private InputAction touchAction;
    public int score=0;

    void Awake()
    {
        // Set up touch or click action
        touchAction = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/primaryTouch/press");
        touchAction.AddBinding("<Mouse>/rightButton"); // for testing in editor
        touchAction.performed += _ => OnTouch();
        touchAction.Enable();
    }

    void Start()
    {
        startPos = soccerBall.transform.position;
    }

    public void OnDestroy()
    {
        touchAction.Disable();
        touchAction.Enable();
    }

    void OnTouch()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveBallToGoalThenReset());
        }
    }
    public void resetOther(){
        Debug.Log("Came in");
        soccerBall.transform.position = startPos;
        isMoving=false;
        //OnDestroy();
    }

    IEnumerator MoveBallToGoalThenReset()
    {
        isMoving = true;

        // Move quickly toward goal
        while (Vector3.Distance(soccerBall.transform.position, goalTransform.position) > 0.01f)
        {
            soccerBall.transform.position = Vector3.MoveTowards(
                soccerBall.transform.position,
                goalTransform.position,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Snap to goal to ensure precision
        soccerBall.transform.position = goalTransform.position;
        score+=1;
        scoreUi.text="Score: "+score;

        // Short pause (optional)
        yield return new WaitForSeconds(0.1f);

        // Instantly return to start
        soccerBall.transform.position = startPos;

        isMoving = false;
    }
    public void reset(){
        Debug.Log("Came in");
        soccerBall.transform.position = startPos;
        isMoving=false;
        score=0;
        scoreUi.text="Score: 0";
        OnDestroy();
    }
    /*private void OnCollisionEnter(Collision collision){
    if (collision.gameObject.CompareTag("Goalie")){
        Debug.Log("Shot blocked by the goalie!");

        // Optional: Cancel the move if still in progress
        StopAllCoroutines();
        soccerBall.transform.position = startPos;
        isMoving = false;

        // Optional: Show message on screen
        scoreUi.text = "Saved!";
        }
    }
    
}
*/
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class ScriptsForGame : MonoBehaviour
{
    public TextMeshProUGUI scoreUi;
    public GameObject soccerBall;
    public float swipeForce = 30f;
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
    }

    void Start()
    {
        startPos = soccerBall.transform.position;
    }

    void Update()
    {
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
        if (isMoving) return;

        Ray ray = mainCamera.ScreenPointToRay(start);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == soccerBall)
            {
                Vector2 swipe = end - start;
                Vector3 dir = new Vector3(swipe.x, 0, swipe.y).normalized;
                ballRb.velocity = Vector3.zero;
                ballRb.AddForce(dir * swipeForce, ForceMode.Impulse);
                isMoving = true;
            }
        }
    }

    public void OnGoalScored()
    {
        score++;
        scoreUi.text = "Score: " + score;
        ResetBall();
    }

    public void resetOther()
    {
        ResetBall();
    }

    public void reset()
    {
        ResetBall();
        score = 0;
        scoreUi.text = "Score: 0";
    }

    void ResetBall()
    {
        ballRb.velocity = Vector3.zero;
        soccerBall.transform.position = startPos;
        isMoving = false;
    }
}


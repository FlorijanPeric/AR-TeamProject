/*using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTrigger : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public Transform goalTransform; // Assign in Inspector
    public float minShotForce = 30f;
    public float maxShotForce = 60f;

    public string imageAName = "GoalTriggerA";
    public string imageBName = "GoalTriggerB";

    private ScriptsForGame gameLogic;

    void Awake()
    {
        // Automatically find the ScriptsForGame component on the same object
        gameLogic = GetComponent<ScriptsForGame>();
        if (gameLogic == null)
        {
            Debug.LogError("ScriptsForGame component not found on the same GameObject!");
        }
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState != TrackingState.Tracking)
                continue;

            // If the detected image is one of our triggers, shoot the ball
            if (trackedImage.referenceImage.name == imageAName ||
                trackedImage.referenceImage.name == imageBName)
            {
                TriggerShot(trackedImage.transform);
            }
        }
    }

    void TriggerShot(Transform imageTransform)
    {
        Debug.Log($"Detected image {imageTransform.name} → Shooting ball toward goal");

        Rigidbody rb = gameLogic.soccerBall.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        // Position ball slightly above the tracked image
        gameLogic.soccerBall.transform.position = imageTransform.position + Vector3.up * 0.2f;

        // Direction: from ball to goal
        Vector3 direction = (goalTransform.position - gameLogic.soccerBall.transform.position).normalized;

        // Random shot force
        float shotForce = Random.Range(minShotForce, maxShotForce);
        rb.AddForce(direction * shotForce, ForceMode.Impulse);

        // Celebration effects
        gameLogic.GoHardLeft();
        gameLogic.GoHardRight();
    }
}
*/
//Fix za celo skripto kr origi je melo milijon problemov
/*
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTrigger : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public Transform goalTransform;
    public float minShotForce = 30f;
    public float maxShotForce = 60f;

    public string imageAName = "GoalTriggerA";
    public string imageBName = "GoalTriggerB";

    private ScriptsForGame gameLogic;
    private Rigidbody ballRb;

    private bool shotInProgress = false;

    void Awake()
    {
        gameLogic = GetComponent<ScriptsForGame>();
        if (gameLogic == null)
        {
            Debug.LogError("ScriptsForGame component not found!");
        }
        ballRb = gameLogic.soccerBall.GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState != TrackingState.Tracking)
                continue;

            if ((trackedImage.referenceImage.name == imageAName ||
                 trackedImage.referenceImage.name == imageBName) && !shotInProgress)
            {
                shotInProgress = true;
                TriggerShot(trackedImage.transform);
            }
        }
    }

    void TriggerShot(Transform imageTransform)
    {
        Debug.Log($"Detected image {imageTransform.name} → Shooting ball toward goal");

        ballRb.velocity = Vector3.zero;

        gameLogic.soccerBall.transform.position = imageTransform.position + Vector3.up * 0.2f;

        Vector3 direction = (goalTransform.position - gameLogic.soccerBall.transform.position).normalized;
        float shotForce = Random.Range(minShotForce, maxShotForce);

        ballRb.AddForce(direction * shotForce, ForceMode.Impulse);

        gameLogic.GoHardLeft();
        gameLogic.GoHardRight();
    }

    void Update()
    {
        if (shotInProgress)
        {
            // If ball stopped moving without scoring, reset shot state so next shot can be taken
            if (ballRb.velocity.magnitude < 0.1f)
            {
                Debug.Log("Ball stopped moving without scoring, ready for next shot.");
                shotInProgress = false;
            }
        }
    }

    // Called by goal collider script when goal detected
    public void ScoreGoal()
    {
        Debug.Log("Score confirmed by goal zone!");
        gameLogic.AddScore();  // Assuming you have this method to update score
        shotInProgress = false;  // Allow next shot
    }
}
*/
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class ARImageTrigger : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public Transform goalTransform;
    public float minShotForce = 30f;
    public float maxShotForce = 60f;

    public string imageAName = "GoalTriggerA";
    public string imageBName = "GoalTriggerB";

    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Canvas uiCanvas;

    private ScriptsForGame gameLogic;
    private bool shotInProgress = false;

    void Awake()
    {
        gameLogic = GetComponent<ScriptsForGame>();
        if (gameLogic == null)
        {
            Debug.LogError("ScriptsForGame component not found on the same GameObject!");
        }
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState != TrackingState.Tracking)
                continue;

            string detectedName = trackedImage.referenceImage.name;

            if ((detectedName == imageAName || detectedName == imageBName) && !shotInProgress)
            {
                shotInProgress = true;
                TriggerShot(trackedImage.transform);
            }
            else if (detectedName != imageAName && detectedName != imageBName)
            {
                ShowPopup("This picture is not compatible");
            }
        }
    }

    void TriggerShot(Transform imageTransform)
    {
        Debug.Log($"Detected image {imageTransform.name} → Shooting ball toward goal");

        Rigidbody rb = gameLogic.soccerBall.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;

        gameLogic.soccerBall.transform.position = imageTransform.position + Vector3.up * 0.2f;

        // Base direction to the goal
        Vector3 direction = goalTransform.position - gameLogic.soccerBall.transform.position;

        // Add wild but realistic random offset
        float horizontalOffset = Random.Range(-2f, 2f); // wider left/right spread
        float verticalOffset = Random.Range(-1f, 1f); // higher/lower spread

        direction += (goalTransform.right * horizontalOffset) + (goalTransform.up * verticalOffset);
        direction = direction.normalized;

        // Random shot force
        float shotForce = Random.Range(minShotForce, maxShotForce);
        rb.AddForce(direction * shotForce, ForceMode.Impulse);

        // Start celebration
        gameLogic.GoHardLeft();
        gameLogic.GoHardRight();

        // Wait 5 seconds before allowing another shot
        Invoke(nameof(ResetShot), 5f);
    }



    void ShowPopup(string message)
    {
        if (popupPrefab == null || uiCanvas == null) return;

        GameObject popupInstance = Instantiate(popupPrefab, uiCanvas.transform);
        Text popupText = popupInstance.GetComponentInChildren<Text>();
        if (popupText != null)
            popupText.text = message;

        Button continueButton = popupInstance.GetComponentInChildren<Button>();
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                Destroy(popupInstance);
            });
        }
    }

    void ResetShot()
    {
        shotInProgress = false;
    }
}

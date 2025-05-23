using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTrigger : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public ScriptsForGame gameLogic;
    public float shotForce ;

    private bool imageATriggered = false;
    private bool imageBTriggered = false;

    public string imageAName = "GoalTriggerA"; // Match this to your image name in XR Reference Image Library
    public string imageBName = "GoalTriggerB";

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

            if (trackedImage.referenceImage.name == imageAName && !imageATriggered)
            {
                imageATriggered = true;
                TriggerShot("A");
            }
            else if (trackedImage.referenceImage.name == imageBName && !imageBTriggered)
            {
                imageBTriggered = true;
                TriggerShot("B");
            }
        }
    }

    void TriggerShot(string imageTag)
    {
        Debug.Log("Detected image: " + imageTag + " → Shooting ball");

        // Shoot the ball
        Rigidbody rb = gameLogic.soccerBall.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        Vector3 direction = Vector3.forward + Vector3.up * 0.7f;
        shotForce = Random.Range(30f, 60f);
        rb.AddForce(direction.normalized * shotForce, ForceMode.Impulse);

        // Trigger confetti
        gameLogic.GoHardLeft();
        gameLogic.GoHardRight();
    }
}

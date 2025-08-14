//FunnyFloat.cs
using UnityEngine;

public class FunnyFloat : MonoBehaviour
{
    private float floatSpeed;
    private Vector3 rotationSpeed;

    void Start()
    {
        floatSpeed = Random.Range(8f, 15f);
        rotationSpeed = new Vector3(
            Random.Range(-180f, 180f),
            Random.Range(-180f, 180f),
            Random.Range(-180f, 180f)
        );

        Destroy(gameObject, 2f); // Destroy after 2 seconds
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}

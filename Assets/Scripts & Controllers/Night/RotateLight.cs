using UnityEngine;

public class RotateLight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float rotationSpeed = 50f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}

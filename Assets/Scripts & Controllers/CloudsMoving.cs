using UnityEngine;

public class CloudsMoving : MonoBehaviour
{
    public float speed = 0.5f;  
    public float threshold = -128f; 
    public float resetPosition = 128f;

    void Update()
    {

        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x <= threshold)
        {
            Vector3 pos = transform.position;
            pos.x = resetPosition;
            transform.position = pos;
        }
    }
}

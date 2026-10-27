using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }
}

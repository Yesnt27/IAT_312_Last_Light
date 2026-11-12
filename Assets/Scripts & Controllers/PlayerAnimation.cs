using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private int isWalkingHash = Animator.StringToHash("IsWalking");
    private int isJumpingHash = Animator.StringToHash("IsJumping");



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        animator.SetBool(isWalkingHash, isMoving);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger(isJumpingHash);
        }
    }
}

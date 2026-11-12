using UnityEngine;

// On the Player: plays the walk / jump / idle animations (Animator on the Art child, controller
// Visual Assets/player/player.controller) from what PlayerController is doing, so they follow the real controls:
// nothing animates while the player is frozen (dialogue, puzzles), and the jump pose shows only when actually in the air.
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimation : MonoBehaviour
{
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Jump Pose")]
    [Tooltip("Seconds off the ground (while not rising) before the jump pose plays, so walking down stairs stays a walk.")]
    [SerializeField] private float fallTimeBeforeJumpPose = 0.15f;
    [Tooltip("Rising faster than this (a jump) shows the jump pose straight away.")]
    [SerializeField] private float risingSpeed = 1f;

    private PlayerController controller;
    private float airTime;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        airTime = controller.IsGrounded ? 0f : airTime + Time.deltaTime;
        bool inAir = !controller.IsGrounded && (controller.VerticalSpeed > risingSpeed || airTime > fallTimeBeforeJumpPose);

        animator.SetBool(IsWalkingHash, controller.MoveInput != 0f);
        animator.SetBool(IsJumpingHash, inAir);
    }
}

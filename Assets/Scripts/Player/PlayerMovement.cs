using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private PlayerInputReader inputReader;

    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        Vector2 input = inputReader.MoveInput;

        Vector3 direction =
            transform.right * input.x +
            transform.forward * input.y;

        direction = Vector3.ClampMagnitude(direction, 1f);

        Vector3 horizontalVelocity = direction * moveSpeed;

        if (grounded && inputReader.JumpPressed)
        {
            verticalVelocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 velocity =
            horizontalVelocity + verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}
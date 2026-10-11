using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Crouch")] 
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchCameraDrop = 0.45f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    [SerializeField] private Transform cameraPivot;
    
    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;
    
    private CharacterController controller;
    private PlayerInputReader inputReader;

    private Vector3 verticalVelocity;
    private Vector3 standingCenter;
    private float standingHeight;
    private float standingCameraY;
    
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();
        
        standingHeight = controller.height;
        standingCenter = controller.center;

        if (cameraPivot != null)
            standingCameraY = cameraPivot.localPosition.y;

        crouchHeight = Mathf.Clamp(
            crouchHeight,
            controller.radius * 2f,
            standingHeight
        );
    }

    private void Update()
    {
        HandleCrouch();
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

        IsSprinting =
            !IsCrouching &&
            inputReader.SprintHeld &&
            input.sqrMagnitude > 0.01f &&
            controller.isGrounded;
        
        float currentSpeed;
        
        if (IsCrouching)
            currentSpeed = crouchSpeed;
        else if (IsSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = moveSpeed;
        
        Vector3 direction =
            transform.right * input.x +
            transform.forward * input.y;

        direction = Vector3.ClampMagnitude(direction, 1f);
        
        Vector3 horizontalVelocity = direction * currentSpeed;

        if (grounded && inputReader.JumpPressed && !IsCrouching)
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
    
    private void HandleCrouch()
    {
        if (inputReader.CrouchPressed)
        {
            if (IsCrouching)
            {
                if (CanStandUp())
                    IsCrouching = false;
            }
            else
            {
                IsCrouching = true;
            }
        }

        float targetHeight = IsCrouching
            ? crouchHeight
            : standingHeight;

        controller.height = Mathf.MoveTowards(
            controller.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        controller.center = standingCenter + Vector3.up *
            ((controller.height - standingHeight) * 0.5f);

        if (cameraPivot != null)
        {
            float targetCameraY = standingCameraY -
                                  (IsCrouching ? crouchCameraDrop : 0f);

            Vector3 cameraPosition = cameraPivot.localPosition;

            cameraPosition.y = Mathf.MoveTowards(
                cameraPosition.y,
                targetCameraY,
                crouchTransitionSpeed * Time.deltaTime
            );

            cameraPivot.localPosition = cameraPosition;
        }
    }
    
    private bool CanStandUp()
    {
        float radius = controller.radius;
        float height = standingHeight;

        Vector3 worldCenter = transform.TransformPoint(
            standingCenter
        );

        Vector3 bottom = worldCenter +
                         Vector3.up * (-height * 0.5f + radius);

        Vector3 top = worldCenter +
                      Vector3.up * (height * 0.5f - radius);

        Collider[] hits = Physics.OverlapCapsule(
            bottom,
            top,
            radius * 0.95f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == controller ||
                hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}
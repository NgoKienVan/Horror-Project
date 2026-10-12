
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float proneSpeed = 0.8f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchCameraDrop = 0.45f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    [SerializeField] private Transform cameraPivot;

    [Header("Prone")]
    [SerializeField] private float proneHeight = 0.6f;
    [SerializeField] private float proneCameraDrop = 1.1f;
    [SerializeField] private float proneTransitionSpeed = 8f;

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
    public bool IsProne { get; private set; }

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

        proneHeight = Mathf.Clamp(
            proneHeight,
            controller.radius * 2f,
            crouchHeight
        );
    }

    private void Update()
    {
        HandlePosture();
        HandleMovement();
    }

    private void HandlePosture()
    {
        if (inputReader.PronePressed)
        {
            if (IsProne)
            {
                if (CanFitHeight(standingHeight))
                {
                    IsProne = false;
                    IsCrouching = false;
                }
                else if (CanFitHeight(crouchHeight))
                {
                    IsProne = false;
                    IsCrouching = true;
                }
            }
            else
            {
                IsProne = true;
                IsCrouching = false;
                IsSprinting = false;
            }
        }

        if (inputReader.CrouchPressed)
        {
            if (IsProne)
            {
                if (CanFitHeight(crouchHeight))
                {
                    IsProne = false;
                    IsCrouching = true;
                }
            }
            else if (IsCrouching)
            {
                if (CanFitHeight(standingHeight))
                    IsCrouching = false;
            }
            else
            {
                IsCrouching = true;
            }
        }

        float targetHeight = IsProne
            ? proneHeight
            : IsCrouching
                ? crouchHeight
                : standingHeight;

        float transitionSpeed = IsProne
            ? proneTransitionSpeed
            : crouchTransitionSpeed;

        controller.height = Mathf.MoveTowards(
            controller.height,
            targetHeight,
            transitionSpeed * Time.deltaTime
        );

        controller.center = standingCenter + Vector3.up *
            ((controller.height - standingHeight) * 0.5f);

        if (cameraPivot != null)
        {
            float cameraDrop = IsProne
                ? proneCameraDrop
                : IsCrouching
                    ? crouchCameraDrop
                    : 0f;

            float targetCameraY = standingCameraY - cameraDrop;

            Vector3 cameraPosition = cameraPivot.localPosition;

            cameraPosition.y = Mathf.MoveTowards(
                cameraPosition.y,
                targetCameraY,
                transitionSpeed * Time.deltaTime
            );

            cameraPivot.localPosition = cameraPosition;
        }
    }

    private void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        Vector2 input = inputReader.MoveInput;

        IsSprinting =
            !IsCrouching &&
            !IsProne &&
            inputReader.SprintHeld &&
            input.sqrMagnitude > 0.01f &&
            grounded;

        float currentSpeed;

        if (IsProne)
            currentSpeed = proneSpeed;
        else if (IsCrouching)
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

        if (grounded &&
            inputReader.JumpPressed &&
            !IsCrouching &&
            !IsProne)
        {
            verticalVelocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private bool CanFitHeight(float targetHeight)
    {
        float radius = controller.radius;

        targetHeight = Mathf.Max(targetHeight, radius * 2f);

        Vector3 targetCenter = standingCenter +
            Vector3.up * ((targetHeight - standingHeight) * 0.5f);

        Vector3 worldCenter = transform.TransformPoint(targetCenter);

        Vector3 bottom = worldCenter +
            Vector3.up * (-targetHeight * 0.5f + radius);

        Vector3 top = worldCenter +
            Vector3.up * (targetHeight * 0.5f - radius);

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
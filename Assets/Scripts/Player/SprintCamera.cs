using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SprintCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Field of View")]
    [SerializeField] private float normalFOV = 75f;
    [SerializeField] private float sprintFOV = 88f;

    [Header("Transition")]
    [SerializeField] private float fovChangeSpeed = 8f;

    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        playerCamera.fieldOfView = normalFOV;
    }

    private void Update()
    {
        float targetFOV =
            playerMovement == playerMovement.IsSprinting
                ? sprintFOV
                : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            fovChangeSpeed * Time.deltaTime
        );
    }
}
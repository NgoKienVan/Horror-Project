using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Mouse Settings")]
    [SerializeField] private float sensitivity = 0.08f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    private PlayerInputReader inputReader;
    private float pitch;
    private bool cursorLocked;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        HandleCursor();

        if (!cursorLocked)
            return;

        HandleLook();
    }

    private void HandleLook()
    {
        Vector2 mouseDelta = inputReader.LookInput;

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        transform.Rotate(
            Vector3.up * mouseX,
            Space.World
        );

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.localRotation =
            Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleCursor()
    {
        if (inputReader.CursorTogglePressed)
        {
            SetCursorLocked(!cursorLocked);
        }
        else if (!cursorLocked && inputReader.ClickPressed)
        {
            SetCursorLocked(true);
        }
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;

        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
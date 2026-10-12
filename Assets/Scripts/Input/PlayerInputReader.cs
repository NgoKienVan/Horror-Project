using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    [Header("Basic Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    
    [Header("Advanced Actions")]
    [SerializeField] private InputActionReference proneAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;
    
    [Header("Camera Actions")]
    [SerializeField] private InputActionReference cursorToggleAction;
    [SerializeField] private InputActionReference clickAction;

    
    public Vector2 MoveInput => moveAction.action.ReadValue<Vector2>();
    
    public Vector2 LookInput => lookAction.action.ReadValue<Vector2>();
    
    public bool JumpPressed => jumpAction.action.WasPressedThisFrame();
    
    public bool SprintHeld => sprintAction.action.IsPressed();
    
    public bool CrouchPressed => crouchAction.action.WasPressedThisFrame();
    
    public bool PronePressed => proneAction.action.WasPressedThisFrame();
    
    public bool CursorTogglePressed => cursorToggleAction.action.WasPressedThisFrame();
    
    public bool ClickPressed => clickAction.action.WasPressedThisFrame();

    private void OnEnable()
    {
        SetActionsEnabled(true);
    }

    private void OnDisable()
    {
        SetActionsEnabled(false);
    }

    private void SetActionsEnabled(bool enabled)
    {
        SetAction(moveAction, enabled);
        SetAction(lookAction, enabled);
        SetAction(jumpAction, enabled);
        
        SetAction(sprintAction, enabled);
        SetAction(crouchAction, enabled);
        SetAction(proneAction, enabled);
        SetAction(cursorToggleAction, enabled);
        SetAction(clickAction, enabled);
    }

    private void SetAction(InputActionReference reference, bool enabled)
    {
        if (reference == null || reference.action == null)
            return;
        
        if(enabled)
            reference.action.Enable();
        else
            reference.action.Disable();
    }
}

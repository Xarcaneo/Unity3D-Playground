using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input actions and stores input values for movement, looking around, jumping, and sprinting.
/// </summary>
public class AssetsInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    [Tooltip("Stores the input value for player movement.")]
    public Vector2 move;

    [Tooltip("Stores the input value for looking around.")]
    public Vector2 look;

    [Tooltip("Indicates whether the jump action has been triggered.")]
    public bool jump;

    [Tooltip("Indicates whether the sprint action has been triggered.")]
    public bool sprint;

    [Tooltip("Indicates whether the fire action has been triggered.")]
    public bool fire;

    [Header("Movement Settings")]
    [Tooltip("Determines if movement input should be analog.")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    [Tooltip("Locks or unlocks the cursor.")]
    public bool cursorLocked = true;

    [Tooltip("Determines if cursor input should be used for looking around.")]
    public bool cursorInputForLook = true;

    /// <summary>
    /// Handles the input action for player movement.
    /// </summary>
    /// <param name="value">The input value for movement direction.</param>
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    /// <summary>
    /// Handles the input action for looking around.
    /// </summary>
    /// <param name="value">The input value for look direction.</param>
    public void OnLook(InputValue value)
    {
        if (cursorInputForLook)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    /// <summary>
    /// Handles the input action for jumping.
    /// </summary>
    /// <param name="value">The input value for the jump action.</param>
    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    /// <summary>
    /// Handles the input action for sprinting.
    /// </summary>
    /// <param name="value">The input value for the sprint action.</param>
    public void OnSprint(InputValue value)
    {
        SprintInput(value.isPressed);
    }

    /// <summary>
    /// Handles the input action for firing.
    /// </summary>
    /// <param name="value">The input value for the firing action.</param>
    public void OnFire(InputValue value)
    {
        FireInput(value.isPressed);
    }

    /// <summary>
    /// Sets the movement input value.
    /// </summary>
    /// <param name="newMoveDirection">The new movement direction input value.</param>
    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    /// <summary>
    /// Sets the look input value.
    /// </summary>
    /// <param name="newLookDirection">The new look direction input value.</param>
    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    /// <summary>
    /// Sets the jump input value.
    /// </summary>
    /// <param name="newJumpState">The new jump state input value.</param>
    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    /// <summary>
    /// Sets the sprint input value.
    /// </summary>
    /// <param name="newSprintState">The new sprint state input value.</param>
    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    /// <summary>
    /// Sets the fire input value.
    /// </summary>
    /// <param name="newSprintState">The new fire state input value.</param>
    public void FireInput(bool newFireState)
    {
        fire = newFireState;
    }

    /// <summary>
    /// Sets the cursor state based on the application focus.
    /// </summary>
    /// <param name="hasFocus">Indicates if the application has focus.</param>
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    /// <summary>
    /// Locks or unlocks the cursor based on the specified state.
    /// </summary>
    /// <param name="newState">The new cursor state.</param>
    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerDashInput : MonoBehaviour
{
    public DashAbility dash;
    public InputActionReference dashAction; // bind to your "Dash" action

    void Reset()
    {
        if (!dash) dash = GetComponent<DashAbility>();
    }

    void OnEnable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed += OnDash;
            if (!dashAction.action.enabled) dashAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed -= OnDash;
            if (dashAction.action.enabled) dashAction.action.Disable();
        }
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        dash?.TryDash();
    }
}

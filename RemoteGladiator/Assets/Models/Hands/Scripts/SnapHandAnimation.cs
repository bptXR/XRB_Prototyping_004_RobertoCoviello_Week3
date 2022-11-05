using UnityEngine;
using UnityEngine.InputSystem;

public class SnapHandAnimation : MonoBehaviour
{
    [SerializeField] private Animator handAnimator;
    [SerializeField] private InputActionReference triggerActionRef;
    [SerializeField] private InputActionReference gripActionRef;

    private static readonly int triggerAnimation = Animator.StringToHash("Trigger");
    private static readonly int gripAnimation = Animator.StringToHash("Grip");

    private void OnEnable()
    {
        triggerActionRef.action.performed += TriggerAction_performed;
        triggerActionRef.action.canceled += TriggerAction_canceled;

        gripActionRef.action.performed += GripAction_performed;
        gripActionRef.action.canceled += GripAction_canceled;
    }

    private void TriggerAction_performed(InputAction.CallbackContext obj) => handAnimator.SetFloat(triggerAnimation, 1);
    private void TriggerAction_canceled(InputAction.CallbackContext obj) => handAnimator.SetFloat(triggerAnimation, 0);

    private void GripAction_performed(InputAction.CallbackContext obj) => handAnimator.SetFloat(gripAnimation, 1);
    private void GripAction_canceled(InputAction.CallbackContext obj) => handAnimator.SetFloat(gripAnimation, 0);

    private void OnDisable()
    {
        triggerActionRef.action.performed -= TriggerAction_performed;
        triggerActionRef.action.canceled -= TriggerAction_canceled;

        gripActionRef.action.performed -= GripAction_performed;
        gripActionRef.action.canceled -= GripAction_canceled;
    }
}
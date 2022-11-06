using UnityEngine;
using UnityEngine.InputSystem;

namespace Interaction
{
    [RequireComponent(typeof(SphereCollider))]
    public class HandPointer : MonoBehaviour
    {
        [SerializeField] private InputActionReference triggerActionReference;
        [SerializeField] private SphereCollider sphereCollider;

        private void OnEnable()
        {
            triggerActionReference.action.performed += OnActionPerformed;
            triggerActionReference.action.canceled += OnActionCanceled;
        }

        private void OnActionPerformed(InputAction.CallbackContext obj) => sphereCollider.enabled = true;

        private void OnActionCanceled(InputAction.CallbackContext obj) => sphereCollider.enabled = false;

        private void OnDisable()
        {
            triggerActionReference.action.performed -= OnActionPerformed;
            triggerActionReference.action.canceled -= OnActionCanceled;
        }
    }
}

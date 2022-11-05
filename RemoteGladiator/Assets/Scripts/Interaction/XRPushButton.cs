using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace Interaction
{
    public class XRPushButton : XRBaseInteractable
    {
        class PressInfo
        {
            internal IXRHoverInteractor interactor;
            internal bool inPressRegion = false;
            internal bool wrongSide = false;
        }

        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [SerializeField] private Transform button = null;
        [SerializeField] private float pressDistance = 0.1f;
        [SerializeField] private float pressBuffer = 0.01f;

        [SerializeField]
        [Tooltip("Offset from the button base to start testing for push")]
        float m_ButtonOffset = 0.0f;

        [SerializeField]
        [Tooltip("How big of a surface area is available for pressing the button")]
        float m_ButtonSize = 0.1f;

        [SerializeField]
        [Tooltip("Treat this button like an on/off toggle")]
        bool m_ToggleButton = false;

        [SerializeField]
        [Tooltip("Events to trigger when the button is pressed")]
        UnityEvent m_OnPress;

        [SerializeField]
        [Tooltip("Events to trigger when the button is released")]
        UnityEvent m_OnRelease;

        [SerializeField]
        [Tooltip("Events to trigger when the button pressed value is updated. Only called when the button is pressed")]
        ValueChangeEvent m_OnValueChange;

        bool m_Pressed = false;
        bool m_Toggled = false;
        float m_Value = 0f;
        Vector3 m_BaseButtonPosition = Vector3.zero;

        Dictionary<IXRHoverInteractor, PressInfo> m_HoveringInteractors = new Dictionary<IXRHoverInteractor, PressInfo>();

        /// <summary>
        /// The object that is visually pressed down
        /// </summary>
        public Transform Button { get { return button; } set { button = value; } }

        /// <summary>
        /// The distance the button can be pressed
        /// </summary>
        public float PressDistance { get { return pressDistance; } set { pressDistance = value; } }

        /// <summary>
        /// The distance (in percentage from 0 to 1) the button is currently being held down
        /// </summary>
        public float Value => m_Value;

        /// <summary>
        /// Events to trigger when the button is pressed
        /// </summary>
        public UnityEvent OnPress => m_OnPress;

        /// <summary>
        /// Events to trigger when the button is released
        /// </summary>
        public UnityEvent OnRelease => m_OnRelease;

        /// <summary>
        /// Events to trigger when the button distance value is changed. Only called when the button is pressed
        /// </summary>
        public ValueChangeEvent OnValueChange => m_OnValueChange;

        /// <summary>
        /// Whether or not a toggle button is in the locked down position
        /// </summary>
        public bool ToggleValue
        {
            get { return m_ToggleButton && m_Toggled; }
            set
            {
                if (!m_ToggleButton)
                    return;

                m_Toggled = value;
                if (m_Toggled)
                    SetButtonHeight(-pressDistance);
                else
                    SetButtonHeight(0.0f);
            }
        }

        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            if (interactor is XRRayInteractor)
                return false;

            return base.IsHoverableBy(interactor);
        }

        void Start()
        {
            if (button != null)
                m_BaseButtonPosition = button.position;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_Toggled)
                SetButtonHeight(-pressDistance);
            else
                SetButtonHeight(0.0f);

            hoverEntered.AddListener(StartHover);
            hoverExited.AddListener(EndHover);
        }

        protected override void OnDisable()
        {
            hoverEntered.RemoveListener(StartHover);
            hoverExited.RemoveListener(EndHover);
            base.OnDisable();
        }

        void StartHover(HoverEnterEventArgs args)
        {
            m_HoveringInteractors.Add(args.interactorObject, new PressInfo { interactor = args.interactorObject });
        }

        void EndHover(HoverExitEventArgs args)
        {
            m_HoveringInteractors.Remove(args.interactorObject);

            if (m_HoveringInteractors.Count == 0)
            {
                if (m_ToggleButton && m_Toggled)
                    SetButtonHeight(-pressDistance);
                else
                    SetButtonHeight(0.0f);
            }
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (m_HoveringInteractors.Count > 0)
                {
                    UpdatePress();
                }
            }
        }

        void UpdatePress()
        {
            var minimumHeight = 0.0f;

            if (m_ToggleButton && m_Toggled)
                minimumHeight = -pressDistance;

            // Go through each interactor
            foreach (var pressInfo in m_HoveringInteractors.Values)
            {
                var interactorTransform = pressInfo.interactor.GetAttachTransform(this);
                var localOffset = transform.InverseTransformVector(interactorTransform.position - m_BaseButtonPosition);

                var withinButtonRegion = (Mathf.Abs(localOffset.x) < m_ButtonSize && Mathf.Abs(localOffset.z) < m_ButtonSize);
                if (withinButtonRegion)
                {
                    if (!pressInfo.inPressRegion)
                    {
                        pressInfo.wrongSide = (localOffset.y < m_ButtonOffset);
                    }

                    if (!pressInfo.wrongSide)
                        minimumHeight = Mathf.Min(minimumHeight, localOffset.y - m_ButtonOffset);
                }

                pressInfo.inPressRegion = withinButtonRegion;
            }

            minimumHeight = Mathf.Max(minimumHeight, -(pressDistance + pressBuffer));

            // If button height goes below certain amount, enter press mode
            var pressed = m_ToggleButton ? (minimumHeight <= -(pressDistance + pressBuffer)) : (minimumHeight < -pressDistance);

            var currentDistance = Mathf.Max(0f, -minimumHeight - pressBuffer);
            m_Value = currentDistance / pressDistance;

            if (m_ToggleButton)
            {
                if (pressed)
                {
                    if (!m_Pressed)
                    {
                        m_Toggled = !m_Toggled;

                        if (m_Toggled)
                            m_OnPress.Invoke();
                        else
                            m_OnRelease.Invoke();
                    }
                }
            }
            else
            {
                if (pressed)
                {
                    if (!m_Pressed)
                        m_OnPress.Invoke();
                }
                else
                {
                    if (m_Pressed)
                        m_OnRelease.Invoke();
                }
            }
            m_Pressed = pressed;

            // Call value change event
            if (m_Pressed)
                m_OnValueChange.Invoke(m_Value);

            SetButtonHeight(minimumHeight);
        }

        void SetButtonHeight(float height)
        {
            if (button == null)
                return;

            Vector3 newPosition = button.localPosition;
            newPosition.y = height;
            button.localPosition = newPosition;
        }

        void OnDrawGizmosSelected()
        {
            var pressStartPoint = Vector3.zero;

            if (button != null)
            {
                pressStartPoint = button.localPosition;
            }

            pressStartPoint.y += m_ButtonOffset - (pressDistance * 0.5f);

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(pressStartPoint, new Vector3(m_ButtonSize, pressDistance, m_ButtonSize));
        }

        void OnValidate()
        {
            SetButtonHeight(0.0f);
        }
    }
}

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
        [SerializeField] private float buttonOffset = 0.0f;
        [SerializeField] private float buttonSize = 0.1f;
        [SerializeField] private bool toggleButton = false;
        
        [SerializeField] UnityEvent onPress;
        [SerializeField] UnityEvent onRelease;
        [SerializeField] ValueChangeEvent onValueChange;

        private bool _pressed = false;
        private bool _toggled = false;
        private float _value = 0f;
        private Vector3 _baseButtonPosition = Vector3.zero;

        Dictionary<IXRHoverInteractor, PressInfo> _mHoveringInteractors = new Dictionary<IXRHoverInteractor, PressInfo>();
        
        public Transform Button { get { return button; } set { button = value; } }
        
        public float PressDistance { get { return pressDistance; } set { pressDistance = value; } }
        
        public float Value => _value;
        
        public UnityEvent OnPress => onPress;
        
        public UnityEvent OnRelease => onRelease;
        
        public ValueChangeEvent OnValueChange => onValueChange;
        
        public bool ToggleValue
        {
            get { return toggleButton && _toggled; }
            set
            {
                if (!toggleButton)
                    return;

                _toggled = value;
                if (_toggled)
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
                _baseButtonPosition = button.position;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_toggled)
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
            _mHoveringInteractors.Add(args.interactorObject, new PressInfo { interactor = args.interactorObject });
        }

        void EndHover(HoverExitEventArgs args)
        {
            _mHoveringInteractors.Remove(args.interactorObject);

            if (_mHoveringInteractors.Count == 0)
            {
                if (toggleButton && _toggled)
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
                if (_mHoveringInteractors.Count > 0)
                {
                    UpdatePress();
                }
            }
        }

        void UpdatePress()
        {
            var minimumHeight = 0.0f;

            if (toggleButton && _toggled)
                minimumHeight = -pressDistance;

            // Go through each interactor
            foreach (var pressInfo in _mHoveringInteractors.Values)
            {
                var interactorTransform = pressInfo.interactor.GetAttachTransform(this);
                var localOffset = transform.InverseTransformVector(interactorTransform.position - _baseButtonPosition);

                var withinButtonRegion = (Mathf.Abs(localOffset.x) < buttonSize && Mathf.Abs(localOffset.z) < buttonSize);
                if (withinButtonRegion)
                {
                    if (!pressInfo.inPressRegion)
                    {
                        pressInfo.wrongSide = (localOffset.y < buttonOffset);
                    }

                    if (!pressInfo.wrongSide)
                        minimumHeight = Mathf.Min(minimumHeight, localOffset.y - buttonOffset);
                }

                pressInfo.inPressRegion = withinButtonRegion;
            }

            minimumHeight = Mathf.Max(minimumHeight, -(pressDistance + pressBuffer));

            // If button height goes below certain amount, enter press mode
            var pressed = toggleButton ? (minimumHeight <= -(pressDistance + pressBuffer)) : (minimumHeight < -pressDistance);

            var currentDistance = Mathf.Max(0f, -minimumHeight - pressBuffer);
            _value = currentDistance / pressDistance;

            if (toggleButton)
            {
                if (pressed)
                {
                    if (!_pressed)
                    {
                        _toggled = !_toggled;

                        if (_toggled)
                            onPress.Invoke();
                        else
                            onRelease.Invoke();
                    }
                }
            }
            else
            {
                if (pressed)
                {
                    if (!_pressed)
                        onPress.Invoke();
                }
                else
                {
                    if (_pressed)
                        onRelease.Invoke();
                }
            }
            _pressed = pressed;

            // Call value change event
            if (_pressed)
                onValueChange.Invoke(_value);

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

            pressStartPoint.y += buttonOffset - (pressDistance * 0.5f);

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(pressStartPoint, new Vector3(buttonSize, pressDistance, buttonSize));
        }

        void OnValidate()
        {
            SetButtonHeight(0.0f);
        }
    }
}

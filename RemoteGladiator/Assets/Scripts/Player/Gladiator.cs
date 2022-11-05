using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class Gladiator : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float rotationSpeed = 5;
        [SerializeField] private CharacterController characterController;

        private Vector3 _playerVelocity;
        private bool _groundedPlayer;
        private bool _buttonPressed;
        private Vector2 _joystickValue;
        private float _gravityValue = -9.81f;
        private Animator _anim;

        private bool _isAttacking;
        private bool _isRunning;

        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int DieIndex = Animator.StringToHash("DieIndex");
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int RunIndex = Animator.StringToHash("RunIndex");
        private static readonly int Idle = Animator.StringToHash("Idle");

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            _groundedPlayer = characterController.isGrounded;

            if (_groundedPlayer && _playerVelocity.y < 0)
                _playerVelocity.y = 0f;

            if (_joystickValue.x == 0 && _joystickValue.y == 0)
                _isRunning = false;

            if (_isRunning)
            {
                _playerVelocity.y += _gravityValue * Time.deltaTime;
                characterController.Move(_playerVelocity * Time.deltaTime);

                if (_joystickValue.y == 0f && _joystickValue.x == 0f) return;
                Vector3 moveDirection = new Vector3(_joystickValue.x, 0f, _joystickValue.y);
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                playerTransform.rotation =
                    Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);

                characterController.Move(moveDirection * (Time.deltaTime * moveSpeed));

                if (moveDirection == Vector3.zero) return;
                playerTransform.forward = moveDirection;
            }
            else
            {
                _anim.SetTrigger(Idle);
            }
        }

        public void OnButtonPress()
        {
            _buttonPressed = true;
        }

        public void OnButtonRelease()
        {
            _buttonPressed = false;
        }

        public void OnJoystickValueChangeX(float x)
        {
            _joystickValue.x = x;
            switch (x)
            {
                case > 0f and <= 0.2f:
                    _anim.SetInteger(RunIndex, 5);
                    _anim.SetTrigger(Run);
                    break;
                case > 0.2f and <= 0.6f:
                    _anim.SetInteger(RunIndex, 4);
                    _anim.SetTrigger(Run);
                    break;
                default:
                    _anim.SetInteger(RunIndex, Random.Range(0, 3));
                    _anim.SetTrigger(Run);
                    break;
            }

            _isRunning = true;
        }

        public void OnJoystickValueChangeY(float y)
        {
            _joystickValue.y = y;
            switch (y)
            {
                case > 0f and <= 0.2f:
                    _anim.SetInteger(RunIndex, 5);
                    _anim.SetTrigger(Run);
                    break;
                case > 0.2f and <= 0.6f:
                    _anim.SetInteger(RunIndex, 4);
                    _anim.SetTrigger(Run);
                    break;
                default:
                    _anim.SetInteger(RunIndex, Random.Range(0, 3));
                    _anim.SetTrigger(Run);
                    break;
            }

            _isRunning = true;
        }
    }
}
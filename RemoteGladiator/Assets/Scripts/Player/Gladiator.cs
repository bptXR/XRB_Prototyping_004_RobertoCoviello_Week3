using System;
using System.Collections;
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
        private Vector2 _joystickValue;
        private Animator _anim;
        private float _gravityValue = -50.0f;
        private float _jumpHeight = 1.5f;

        private bool _groundedPlayer;
        private bool _isRunning;
        
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Jump = Animator.StringToHash("Jump");
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

            if (_joystickValue == Vector2.zero)
                _isRunning = false;

            _playerVelocity.y += _gravityValue * Time.deltaTime;
            characterController.Move(_playerVelocity * Time.deltaTime);

            if (_isRunning)
            {
                Vector3 moveDirection = new Vector3(_joystickValue.x, 0f, _joystickValue.y);
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                playerTransform.rotation =
                    Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);

                characterController.Move(moveDirection * (Time.deltaTime * moveSpeed));

                if (moveDirection == Vector3.zero) return;
                playerTransform.forward = moveDirection;
                _anim.SetTrigger(Run);
                print("Is running");
            }
            else
            {
                _anim.SetTrigger(Idle);
                print("Is idle");
            }
        }
        
        public void OnJumpButtonPressed()
        {
            if (!_groundedPlayer) return;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
            _anim.SetTrigger(Jump);
            print("is jumping");
        }

        public void OnJoystickValueChangeX(float x)
        {
            _joystickValue.x = x;
            _isRunning = true;
        }

        public void OnJoystickValueChangeY(float y)
        {
            _joystickValue.y = y;
            _isRunning = true;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            print("Game Over");
            Time.timeScale = 0.1f;
        }
        
    }
}
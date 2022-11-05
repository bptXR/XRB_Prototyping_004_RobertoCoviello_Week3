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
        private bool _groundedPlayer;
        private Vector2 _joystickValue;
        private float _gravityValue = -50.0f;
        private float _jumpHeight = 2.0f;
        private Animator _anim;

        private bool _isAttacking;
        private bool _isRunning;
        private bool _canAttack = true;

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

            if (_joystickValue == Vector2.zero)
                _isRunning = false;

            _playerVelocity.y += _gravityValue * Time.deltaTime;
            characterController.Move(_playerVelocity * Time.deltaTime);

            if (!_isRunning || _isAttacking) return;
            if (_joystickValue.y == 0f && _joystickValue.x == 0f) return;
            Vector3 moveDirection = new Vector3(_joystickValue.x, 0f, _joystickValue.y);
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            playerTransform.rotation =
                Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            characterController.Move(moveDirection * (Time.deltaTime * moveSpeed));

            if (moveDirection == Vector3.zero) return;
            playerTransform.forward = moveDirection;
            _anim.SetInteger(Run, Random.Range(0, 3));
            _anim.SetTrigger(Run);
        }

        public void OnAttackButtonPressed()
        {
            StartCoroutine(PlayAttackAnimation());
        }

        private IEnumerator PlayAttackAnimation()
        {
            print("attack 1");
            if (!_canAttack) yield break;
            _anim.SetInteger(AttackIndex, Random.Range(0, 3));
            _anim.SetTrigger(Attack);
            print("attack 2");
            _canAttack = false;
            yield return new WaitForSeconds(1.5f);
            _canAttack = true;
        }

        public void OnJumpButtonPressed()
        {
            if (!_groundedPlayer) return;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
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

        public void AttackEnemy()
        {
            if (!_isAttacking) return;
            print("Attacked");
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            _isAttacking = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            _isAttacking = false;
        }
    }
}
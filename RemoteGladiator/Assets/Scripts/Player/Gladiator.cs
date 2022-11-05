using UnityEngine;

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

        private void Update()
        {
            _groundedPlayer = characterController.isGrounded;

            if (_groundedPlayer && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }
            _playerVelocity.y += _gravityValue * Time.deltaTime;
            characterController.Move(_playerVelocity * Time.deltaTime);

            if (_joystickValue.y == 0f && _joystickValue.x == 0f) return;
            Vector3 moveDirection = new Vector3(_joystickValue.x, 0f, _joystickValue.y);
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            playerTransform.rotation =
                Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            characterController.Move(moveDirection * (Time.deltaTime * moveSpeed));

            if (moveDirection != Vector3.zero)
            {
                playerTransform.forward = moveDirection;
            }

        }

        public void OnButtonPress() => _buttonPressed = true;
        public void OnButtonRelease() => _buttonPressed = false;
        public void OnJoystickValueChangeX(float x) => _joystickValue.x = x;
        public void OnJoystickValueChangeY(float y) => _joystickValue.y = y;
    }
}
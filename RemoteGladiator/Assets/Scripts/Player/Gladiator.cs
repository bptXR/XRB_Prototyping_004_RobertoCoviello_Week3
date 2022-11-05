using UnityEngine;

namespace Player
{
    public class Gladiator : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float rotationSpeed = 5;

        private bool _buttonPressed;
        private Vector2 _joystickValue;

        private void Update()
        {
            //if (!_buttonPressed) return;

            // Get current player position
            var playerPosition = transform.localPosition;

            // Calculate player velocity and new position
            playerPosition += new Vector3(_joystickValue.x * moveSpeed * Time.deltaTime, 0f,
                _joystickValue.y * moveSpeed * Time.deltaTime);

            // Look in direction of movement
            Vector3 moveDirection = new Vector3(_joystickValue.x, 0f, _joystickValue.y);
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            playerTransform.rotation =
                Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Update player position
            playerTransform.localPosition = playerPosition;
        }

        public void OnButtonPress() => _buttonPressed = true;
        public void OnButtonRelease() => _buttonPressed = false;
        public void OnJoystickValueChangeX(float x) => _joystickValue.x = x;
        public void OnJoystickValueChangeY(float y) => _joystickValue.y = y;
    }
}
using UnityEngine;

namespace Player
{
    public class Gladiator : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float moveSpeed;

        private bool _buttonPressed;
        private Vector2 _joystickValue;

        private void Update()
        {
            //if (!_buttonPressed) return;

            // Get current player position
            var playerPosition = playerTransform.localPosition;

            // Calculate player velocity and new position
            playerPosition += new Vector3(_joystickValue.x * moveSpeed * Time.deltaTime, 0f,
                _joystickValue.y * moveSpeed * Time.deltaTime);

            // Update player position
            playerTransform.localPosition = playerPosition;
        }

        public void OnButtonPress() => _buttonPressed = true;
        public void OnButtonRelease() => _buttonPressed = false;
        public void OnJoystickValueChangeX(float x) => _joystickValue.x = x;
        public void OnJoystickValueChangeY(float y) => _joystickValue.y = y;
    }
}
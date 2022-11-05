using System;
using UnityEngine;

namespace Enemies
{
    public class LookAtPlayer : MonoBehaviour
    {
        private Transform _camera;

        private void Start()
        {
            _camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        private void Update()
        {
            transform.LookAt(transform.position + _camera.forward);
        }
    }
}
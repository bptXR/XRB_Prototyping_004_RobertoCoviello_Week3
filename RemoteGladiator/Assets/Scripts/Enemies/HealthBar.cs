using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Gradient gradient;
        [SerializeField] private Image fill;
        
        
        public void SetMaxHealth(int health)
        {
            slider.maxValue = health;
            slider.value = health;

            fill.color = gradient.Evaluate(1f);
        }
        
        public void SetHealth(int health)
        {
            slider.value = health;

            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
        
    }
}

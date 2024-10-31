using UnityEngine;
    public class FriendlyTarget : MonoBehaviour
    {
        public float maxHp = 100f;
        public float currentHp;

        private void Start()
        {
            currentHp = maxHp;
        }

        public void TakeDamage(float damage)
        {
            currentHp = Mathf.Max(0, currentHp - damage);
        }

        public bool IsAlive()
        {
            return currentHp > 0;
        }

        public void Heal(float healAmount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + healAmount); 
        }
}
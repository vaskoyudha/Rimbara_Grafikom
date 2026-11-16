using UnityEngine;

namespace Rimbara.World
{
    // Small decorative patrol/perch motions, not ecology or navigation AI.
    public sealed class RestoredAmbientMotion : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(2f, 0f, 0f);
        [SerializeField] private float speed = 0.5f;
        private Vector3 origin;
        private Animator animator;
        private void Awake() { origin = transform.localPosition; animator = GetComponentInChildren<Animator>(); }
        public void Configure(Vector3 movement, float rate) { offset = movement; speed = rate; }
        private void Update()
        {
            transform.localPosition = origin + offset * ((Mathf.Sin(Time.time * speed) + 1f) * 0.5f);
            if (animator != null)
            {
                float direction = Mathf.Cos(Time.time * speed);
                animator.SetFloat("Speed", Mathf.Abs(direction) > 0.12f ? 1.2f : 0f);
                animator.SetFloat("MotionSpeed", 1f);
                Vector3 heading = offset * Mathf.Sign(direction);
                heading.y = 0f;
                if (heading.sqrMagnitude > 0.01f) transform.localRotation = Quaternion.LookRotation(heading);
            }
        }
    }
}

using UnityEngine;

namespace TankGame.Player
{
    /// <summary>
    /// Kamerayı tankı takip ettirir
    /// Ana kameraya ekleyin
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        private Transform target;

        /// <summary>
        /// Takip edilecek hedefi ayarla (genellikle oyuncunun tankı)
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            Debug.Log($"Camera şimdi {newTarget.name} objesini takip ediyor");
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            // Hedef pozisyonu hesapla
            Vector3 desiredPosition = target.position + offset;

            // Smooth şekilde hareket et
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}

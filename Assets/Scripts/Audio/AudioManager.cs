using UnityEngine;

namespace TankGame.Audio
{
    /// <summary>
    /// Oyun seslerini yöneten singleton.
    /// Tüm ses efektleri bu sınıf üzerinden çalınır.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip bulletStickSound;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private AudioClip gameEndSound;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Ateş etme sesini çalar
        /// </summary>
        public void PlayFireSound()
        {
            PlaySound(fireSound);
        }

        /// <summary>
        /// Bullet box'a yapıştığında çalar
        /// </summary>
        public void PlayBulletStickSound()
        {
            PlaySound(bulletStickSound);
        }

        /// <summary>
        /// Patlama sesini çalar
        /// </summary>
        public void PlayExplosionSound()
        {
            PlaySound(explosionSound);
        }

        /// <summary>
        /// Oyun bitiş sesini çalar
        /// </summary>
        public void PlayGameEndSound()
        {
            PlaySound(gameEndSound);
        }

        /// <summary>
        /// Verilen ses klibini çalar
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
            else
            {
                // Fallback: AudioSource yoksa direkt çal
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, sfxVolume);
            }
        }

        /// <summary>
        /// Belirtilen pozisyonda ses çalar (3D ses için)
        /// </summary>
        public void PlaySoundAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }

        /// <summary>
        /// SFX ses seviyesini ayarlar
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
    }
}

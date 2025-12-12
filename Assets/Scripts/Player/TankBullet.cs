using UnityEngine;
using Photon.Pun;
using TankGame;

namespace TankGame.Player
{
    /// <summary>
    /// Mermi davranışı - +X yönünde hareket eder
    /// PhotonNetwork.Instantiate ile spawn edilir
    /// Renk bilgisi instantiation data ile gönderilir (RPC race condition önlenir)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PhotonView))]
    public class TankBullet : MonoBehaviourPun
    {
        [Header("Test Mode")]
        [Tooltip("Aktifken tüm bullet'lar tüm renklerdeki kutuları yok edebilir")]
        public static bool TestMode = false;

        [Header("Bullet Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D rb;
        private TankColor bulletColor;
        private bool isDestroyed = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        private void Start()
        {
            // Instantiation data'dan renk bilgisini al
            if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
            {
                bulletColor = (TankColor)(int)photonView.InstantiationData[0];
            }

            // +X yönünde sabit hız ver
            rb.velocity = Vector2.right * speed;

            // Belirli süre sonra yok et
            if (photonView.IsMine)
            {
                Invoke(nameof(DestroyBullet), lifetime);
            }
        }

        private void DestroyBullet()
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Bullet'ın rengini döndürür
        /// </summary>
        public TankColor GetColor()
        {
            return bulletColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Sadece owner collision işlemlerini yapsın
            if (!photonView.IsMine) return;

            // Zaten yok edildiyse işlem yapma (aynı anda 2 kutuya çarpma durumu)
            if (isDestroyed) return;

            // ÖNCELİKLE Box kontrolü yap (owner kontrolünden önce!)
            // Scene object'lerin Owner'ı Master Client olduğu için
            // Box kontrolü owner kontrolünden önce yapılmalı
            Box box = other.GetComponent<Box>();
            if (box != null)
            {
                // Test modunda veya renk eşleşiyorsa box'ı yok et
                if (TestMode || box.GetColor() == bulletColor)
                {
                    // Box ve bullet yok olsun
                    box.RequestDestroy();
                    isDestroyed = true;
                    PhotonNetwork.Destroy(gameObject);
                }
                else
                {
                    // Farklı renk - sadece bullet yok olsun
                    isDestroyed = true;
                    PhotonNetwork.Destroy(gameObject);
                }
                return;
            }

            // Kendi tankımıza çarpmasın (Box değilse kontrol et)
            PhotonView otherPV = other.GetComponent<PhotonView>();
            if (otherPV != null && otherPV.Owner == photonView.Owner)
            {
                return;
            }

            // Başka bir şeye çarptı, bullet'ı yok et
            isDestroyed = true;
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

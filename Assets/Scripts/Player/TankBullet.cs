using UnityEngine;
using Photon.Pun;
using TankGame;

namespace TankGame.Player
{
    /// <summary>
    /// Mermi davranışı - +X yönünde hareket eder
    /// PhotonNetwork.Instantiate ile spawn edilir
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PhotonView))]
    public class TankBullet : MonoBehaviourPun
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;

        private Rigidbody2D rb;
        private TankColor bulletColor;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        private void Start()
        {
            // +X yönünde sabit hız ver
            rb.velocity = Vector2.right * speed;
            Debug.Log("Bullet velocity: " + rb.velocity);

            // Belirli süre sonra yok et
            if (photonView.IsMine)
            {
                Invoke(nameof(DestroyBullet), lifetime);
                Debug.Log("Bullet destroyed");
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
        /// Bullet'ın rengini ayarlar (Tank tarafından çağrılır)
        /// </summary>
        public void SetColor(TankColor color)
        {
            bulletColor = color;
            // Diğer clientlara renk bilgisini gönder
            photonView.RPC(nameof(RPC_SetColor), RpcTarget.Others, (int)color);
        }

        [PunRPC]
        private void RPC_SetColor(int colorIndex)
        {
            bulletColor = (TankColor)colorIndex;
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

            // ÖNCELİKLE Box kontrolü yap (owner kontrolünden önce!)
            // Scene object'lerin Owner'ı Master Client olduğu için
            // Box kontrolü owner kontrolünden önce yapılmalı
            Box box = other.GetComponent<Box>();
            if (box != null)
            {
                // Renk kontrolü yap
                if (box.GetColor() == bulletColor)
                {
                    // Aynı renk - hem box hem bullet yok olsun
                    box.RequestDestroy();
                    PhotonNetwork.Destroy(gameObject);
                    Debug.Log($"Aynı renk! Box ve Bullet yok edildi. Renk: {bulletColor}");
                }
                else
                {
                    // Farklı renk - sadece bullet yok olsun
                    PhotonNetwork.Destroy(gameObject);
                    Debug.Log($"Farklı renk! Sadece Bullet yok edildi. Bullet: {bulletColor}, Box: {box.GetColor()}");
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
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

using UnityEngine;
using Photon.Pun;
using TankGame;
using TankGame.Score;
using TankGame.UI;

namespace TankGame.Tank
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

        [Header("Stick Settings")]
        [Tooltip("Box'a yapıştıktan sonra yok olma gecikmesi")]
        [SerializeField] private float stickDestroyDelay = 1f;

        private Rigidbody2D rb;
        private TankColor bulletColor;
        private bool isDestroyed = false;
        private bool isStuck = false;
        private Box stuckBox = null;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            // Hızlı hareket eden objeler için continuous collision detection
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
            if (photonView.IsMine && !isStuck)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Bullet'ı box'a yapıştırır
        /// </summary>
        private void StickToBox(Box box)
        {
            isStuck = true;
            stuckBox = box;

            // Hareketi durdur
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // Box'ın child'ı yap (box ile birlikte hareket etsin)
            transform.SetParent(box.transform);

            // Lifetime invoke'u iptal et
            CancelInvoke(nameof(DestroyBullet));

            // Tüm clientlarda yapışma efektini göster
            photonView.RPC(nameof(RPC_StickToBox), RpcTarget.Others, box.photonView.ViewID);

            // Gecikme sonrası yok et
            Invoke(nameof(DestroyStuckBox), stickDestroyDelay);
        }

        [PunRPC]
        private void RPC_StickToBox(int boxViewID)
        {
            // Diğer clientlarda da yapışma efektini uygula
            PhotonView boxPV = PhotonView.Find(boxViewID);
            if (boxPV != null)
            {
                Box box = boxPV.GetComponent<Box>();
                if (box != null)
                {
                    isStuck = true;
                    stuckBox = box;

                    // Hareketi durdur
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;

                    // Box'ın child'ı yap
                    transform.SetParent(box.transform);
                }
            }
        }

        /// <summary>
        /// Yapışık box ve bullet'ı yok eder
        /// </summary>
        private void DestroyStuckBox()
        {
            if (!photonView.IsMine) return;
            if (stuckBox == null) return;

            // Box pozisyonunu kaydet
            Vector3 boxPosition = stuckBox.transform.position;

            // Skor ekle
            int teamId = PlayerInfo.GetTeamID(photonView.Owner);
            int points = 0;
            if (ScoreManager.Instance != null)
            {
                points = ScoreManager.Instance.GetPointsPerBox();
                ScoreManager.Instance.AddScore(teamId);
            }

            // Floating text göster - tüm clientlarda
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowFloatingText(points, boxPosition);
            }

            // Box'ı yok et
            stuckBox.RequestDestroy();

            // Bullet'ı yok et
            isDestroyed = true;
            PhotonNetwork.Destroy(gameObject);
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
                // Test modunda veya renk eşleşiyorsa box'a yapış
                if (TestMode || box.GetColor() == bulletColor)
                {
                    // Zaten yapışmışsa işlem yapma
                    if (isStuck) return;

                    // Box'a yapış
                    StickToBox(box);
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

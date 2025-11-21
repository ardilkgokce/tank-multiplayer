using UnityEngine;
using Photon.Pun;

namespace TankGame.Player
{
    /// <summary>
    /// Tank hareketi ve kontrolü
    /// WASD veya Arrow keys ile 2D hareket
    /// Sadece kendi tankını kontrol eder (photonView.IsMine)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PhotonView))]
    public class TankController : MonoBehaviourPun, IPunObservable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Components
        private Rigidbody2D rb;
        private PhotonView pv;

        // Team info
        private int teamID = -1;
        private string playerName = "";

        // Movement input
        private Vector2 moveInput;

        // Network sync
        private Vector3 networkPosition;
        private float lerpSpeed = 10f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pv = GetComponent<PhotonView>();

            // Network sync için initial değerler
            networkPosition = transform.position;
        }

        private void Start()
        {
            // Owner player'dan team bilgisini al
            if (pv.Owner != null)
            {
                teamID = PlayerInfo.GetTeamID(pv.Owner);
                playerName = PlayerInfo.GetPlayerName(pv.Owner);
                Debug.Log($"Tank başlatıldı: {playerName} - {PlayerInfo.GetTeamName(teamID)} - IsMine: {pv.IsMine}");
            }

            // Sprite rengi zaten prefab'da tanımlı (Tank_Green, Tank_Purple vb.)
            // Layer assignment TankGameManager tarafından yapılıyor
        }

        private void Update()
        {
            // Sadece kendi tankımızı kontrol edebiliriz
            if (!pv.IsMine)
            {
                // Diğer oyuncuların tanklarını smooth şekilde senkronize et
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * lerpSpeed);
                return;
            }

            // Input al
            GetInput();
        }

        private void FixedUpdate()
        {
            // Sadece kendi tankımız için physics hesapla
            if (!pv.IsMine)
                return;

            // Hareketi uygula
            ApplyMovement();
        }

        /// <summary>
        /// WASD veya Arrow keys input'unu al
        /// </summary>
        private void GetInput()
        {
            // Horizontal (A/D veya Left/Right)
            float horizontal = Input.GetAxisRaw("Horizontal");

            // Vertical (W/S veya Up/Down)
            float vertical = Input.GetAxisRaw("Vertical");

            // Movement input (yukarı/aşağı/sağ/sol)
            moveInput = new Vector2(horizontal, vertical).normalized;
        }

        /// <summary>
        /// Rigidbody2D ile hareketi uygula
        /// </summary>
        private void ApplyMovement()
        {
            // Yukarı/aşağı/sağ/sol hareket - rotasyon yok
            Vector2 movement = moveInput * moveSpeed;
            rb.velocity = movement;
        }


        #region IPunObservable Implementation

        /// <summary>
        /// Network üzerinden pozisyon senkronizasyonu (rotasyon yok)
        /// </summary>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Kendi verilerimizi gönder
                stream.SendNext(transform.position);
                stream.SendNext(rb.velocity);
            }
            else
            {
                // Diğer oyuncunun verilerini al
                networkPosition = (Vector3)stream.ReceiveNext();
                Vector2 networkVelocity = (Vector2)stream.ReceiveNext();

                // Lag compensation için pozisyon tahmini
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += (Vector3)(networkVelocity * lag);
            }
        }

        #endregion

        #region Debug Helpers

        private void OnDrawGizmos()
        {
            // Editor'da tankın yönünü göster
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 1f);
        }

        #endregion
    }
}

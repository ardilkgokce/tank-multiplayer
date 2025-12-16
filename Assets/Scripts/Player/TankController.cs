using UnityEngine;
using Photon.Pun;
using TankGame;
using TankGame.Game;
using TankGame.MobileInput;
using TankGame.Audio;
using System.Collections;

namespace TankGame.Tank
{
    /// <summary>
    /// Tank hareketi ve kontrolü
    /// WASD veya Arrow keys ile 2D hareket + Joystick desteği (mobil)
    /// Sadece kendi tankını kontrol eder (photonView.IsMine)
    /// Oyun başlayana kadar hareket ve ateş etme devre dışı
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PhotonView))]
    public class TankController : MonoBehaviourPun, IPunObservable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Shooting Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private string bulletPrefabName = "Bullet";

        [Header("Tank Color")]
        [SerializeField] private TankColor tankColor;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Test Mode")]
        [Tooltip("Aktifken tüm bullet'lar tüm renklerdeki kutuları yok edebilir")]
        [SerializeField] private bool testMode = false;

        // Components
        private Rigidbody2D rb;
        private PhotonView pv;

        // Team info
        private int teamID = -1;
        private string playerName = "";

        // Movement input
        private Vector2 moveInput;

        // Network sync - improved interpolation
        private Vector3 networkPosition;
        private Vector2 networkVelocity;
        private Vector3 smoothVelocity = Vector3.zero;
        private float smoothTime = 0.08f; // SmoothDamp için süre (düşük = daha hızlı takip)
        private float teleportThreshold = 3f; // Bu mesafeden fazlaysa teleport
        private float velocityPredictionFactor = 0.5f; // Velocity tahmin çarpanı

        // Shooting
        private float nextFireTime = 0f;
        private bool fireButtonPressed = false;

        // Game state
        private bool canMove = false;

        // Joystick reference (runtime'da atanır)
        private Joystick movementJoystick;

        // Damage flash effect
        private bool isFlashing = false;
        private float flashDuration = 1f;
        private float flashSpeed = 10f;
        private Color originalColor;
        private Coroutine flashCoroutine;

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

            // Test modunu static değişkene aktar (sadece kendi tankımız için)
            if (pv.IsMine)
            {
                TankBullet.TestMode = testMode;
            }

            // Original rengi kaydet (flash efekti için)
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Sprite rengi zaten prefab'da tanımlı (Tank_Green, Tank_Purple vb.)
            // Layer assignment TankGameManager tarafından yapılıyor

            // Oyun başlama event'ine subscribe ol
            GameController.OnGameStarted += OnGameStarted;

            // Oyun zaten başlamışsa hareket aktif
            if (GameController.Instance != null && GameController.Instance.IsGameStarted())
            {
                canMove = true;
            }
        }

        private void OnDestroy()
        {
            GameController.OnGameStarted -= OnGameStarted;
        }

        private void OnGameStarted()
        {
            canMove = true;
            Debug.Log($"Tank hareket aktif: {playerName}");

            // Kendi tankımızsa joystick'i bul ve aktif et
            if (pv.IsMine)
            {
                // MobileInputManager varsa joystick'i al
                if (MobileInputManager.Instance != null)
                {
                    movementJoystick = MobileInputManager.Instance.GetMovementJoystick();
                }
            }
        }

        /// <summary>
        /// Joystick referansını ayarlar (MobileInputManager tarafından çağrılır)
        /// </summary>
        public void SetJoystick(Joystick joystick)
        {
            movementJoystick = joystick;
        }

        /// <summary>
        /// Fire butonundan ateş tetiklenir (UI Button tarafından çağrılır)
        /// </summary>
        public void OnFireButtonDown()
        {
            fireButtonPressed = true;
        }

        /// <summary>
        /// Fire butonu bırakıldığında (UI Button tarafından çağrılır)
        /// </summary>
        public void OnFireButtonUp()
        {
            fireButtonPressed = false;
        }

        private void Update()
        {
            // Sadece kendi tankımızı kontrol edebiliriz
            if (!pv.IsMine)
            {
                // Diğer oyuncuların tanklarını smooth şekilde senkronize et
                SyncRemotePlayer();
                return;
            }

            // Oyun başlamadıysa kontrolleri devre dışı bırak
            if (!canMove)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            // Input al
            GetInput();

            // Ateş etme kontrolü
            HandleShooting();
        }

        private void FixedUpdate()
        {
            // Sadece kendi tankımız için physics hesapla
            if (!pv.IsMine)
                return;

            // Oyun başlamadıysa hareket etme
            if (!canMove)
                return;

            // Hareketi uygula
            ApplyMovement();
        }

        /// <summary>
        /// Uzak oyuncuların pozisyonlarını smooth şekilde senkronize eder
        /// SmoothDamp + Velocity prediction kullanır
        /// </summary>
        private void SyncRemotePlayer()
        {
            // Hedef pozisyonu velocity ile tahmin et (daha akıcı hareket için)
            Vector3 predictedPosition = networkPosition + (Vector3)(networkVelocity * velocityPredictionFactor * Time.deltaTime);

            // Çok uzaktaysa teleport et (lag spike durumları)
            float distance = Vector3.Distance(transform.position, predictedPosition);
            if (distance > teleportThreshold)
            {
                transform.position = predictedPosition;
                smoothVelocity = Vector3.zero;
                return;
            }

            // SmoothDamp ile yumuşak geçiş (Lerp'den daha iyi sonuç verir)
            transform.position = Vector3.SmoothDamp(
                transform.position,
                predictedPosition,
                ref smoothVelocity,
                smoothTime
            );
        }

        /// <summary>
        /// WASD/Arrow keys + Joystick input'unu al
        /// </summary>
        private void GetInput()
        {
            // Önce klavye input'unu al (WASD veya Arrow keys) - test için
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Eğer joystick varsa ve kullanılıyorsa, joystick input'unu ekle
            if (movementJoystick != null)
            {
                // Joystick input'u klavye input'unun üzerine ekle (hangisi daha büyükse o kullanılır)
                float joystickH = movementJoystick.Horizontal;
                float joystickV = movementJoystick.Vertical;

                // Joystick değerleri daha büyükse onları kullan
                if (Mathf.Abs(joystickH) > Mathf.Abs(horizontal))
                    horizontal = joystickH;
                if (Mathf.Abs(joystickV) > Mathf.Abs(vertical))
                    vertical = joystickV;
            }

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

        /// <summary>
        /// Space tuşu veya Fire butonu ile ateş etme
        /// </summary>
        private void HandleShooting()
        {
            // Klavye (Space) veya mobil fire butonu
            bool shouldFire = Input.GetKey(KeyCode.Space) || fireButtonPressed;

            if (shouldFire && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }

        /// <summary>
        /// Mermi oluştur ve ateşle
        /// </summary>
        private void Fire()
        {
            if (firePoint == null)
            {
                Debug.LogWarning("FirePoint atanmamış!");
                return;
            }

            // Ateş sesini takım arkadaşlarına gönder (RPC)
            pv.RPC(nameof(RPC_PlayFireSound), RpcTarget.All, teamID);

            // Bullet'ı network üzerinden oluştur
            // Renk bilgisini instantiation data olarak gönder (RPC race condition önlenir)
            object[] instantiationData = new object[] { (int)tankColor };
            PhotonNetwork.Instantiate(bulletPrefabName, firePoint.position, Quaternion.identity, 0, instantiationData);
        }

        [PunRPC]
        private void RPC_PlayFireSound(int senderTeamID)
        {
            // Sadece aynı takımdakiler ve o takımın izleyicileri duyar
            int myTeamID = PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer);
            if (myTeamID == senderTeamID)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayFireSound();
                }
            }
        }

        /// <summary>
        /// Tank'ın rengini döndürür
        /// </summary>
        public TankColor GetTankColor()
        {
            return tankColor;
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
                networkVelocity = (Vector2)stream.ReceiveNext();

                // Lag compensation için pozisyon tahmini
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                networkPosition += (Vector3)(networkVelocity * lag);
            }
        }

        #endregion

        #region Damage Flash Effect

        /// <summary>
        /// Puan kaybedildiğinde çağrılır - tüm clientlarda kırmızı yanıp sönme
        /// </summary>
        public void TriggerDamageFlash()
        {
            // RPC ile tüm clientlara gönder
            pv.RPC(nameof(RPC_DamageFlash), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_DamageFlash()
        {
            // Önceki flash devam ediyorsa durdur
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            // Yeni flash başlat
            flashCoroutine = StartCoroutine(DamageFlashCoroutine());
        }

        private IEnumerator DamageFlashCoroutine()
        {
            if (spriteRenderer == null) yield break;

            isFlashing = true;
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                // Kırmızı ve beyaz arasında ping-pong
                float t = Mathf.PingPong(elapsed * flashSpeed, 1f);
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Original renge dön
            spriteRenderer.color = originalColor;
            isFlashing = false;
            flashCoroutine = null;
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

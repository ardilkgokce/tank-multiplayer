using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TankGame.Game;
using TankGame.Tank;
using System.Collections;

namespace TankGame.MobileInput
{
    /// <summary>
    /// Mobil input yöneticisi - Joystick ve Fire butonunu yönetir.
    /// Oyuncular için oyun başladığında aktif olur.
    /// İzleyiciler için hiç açılmaz.
    /// </summary>
    public class MobileInputManager : MonoBehaviour
    {
        public static MobileInputManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject mobileInputPanel;
        [SerializeField] private Joystick movementJoystick;
        [SerializeField] private Button fireButton;

        [Header("Blink Settings")]
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private float blinkDuration = 0.3f;
        [SerializeField] private float blinkAlpha = 0.3f;

        // Local player'ın tank controller'ı
        private TankController localTankController;
        private bool isSpectator = false;
        private bool isInitialized = false;
        private Image fireButtonImage;

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

        private void Start()
        {
            // Başlangıçta mobil input'u gizle
            if (mobileInputPanel != null)
            {
                mobileInputPanel.SetActive(false);
            }

            // İzleyici mi kontrol et
            isSpectator = PlayerInfo.GetRole(PhotonNetwork.LocalPlayer) == PlayerInfo.ROLE_SPECTATOR;

            if (isSpectator)
            {
                // İzleyiciler için joystick hiç açılmaz
                Debug.Log("MobileInputManager: İzleyici - joystick devre dışı");
                return;
            }

            // Oyun başlama event'ine subscribe ol
            GameController.OnGameStarted += OnGameStarted;

            // Oyun bitiş event'ine subscribe ol
            GameSessionManager.OnGameEnded += OnGameEnded;

            // Fire button event'lerini ayarla
            SetupFireButton();

            // Fire button'un Image component'ini al
            if (fireButton != null)
            {
                fireButtonImage = fireButton.GetComponent<Image>();
            }

            isInitialized = true;
            Debug.Log("MobileInputManager: Oyuncu - joystick hazır, oyun başlamasını bekliyor");
        }

        private void OnDestroy()
        {
            GameController.OnGameStarted -= OnGameStarted;
            GameSessionManager.OnGameEnded -= OnGameEnded;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Fire butonunun event'lerini ayarlar
        /// </summary>
        private void SetupFireButton()
        {
            if (fireButton == null) return;

            // EventTrigger ile PointerDown ve PointerUp event'lerini yakalayacağız
            var eventTrigger = fireButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = fireButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // PointerDown
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => OnFireButtonDown());
            eventTrigger.triggers.Add(pointerDown);

            // PointerUp
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => OnFireButtonUp());
            eventTrigger.triggers.Add(pointerUp);
        }

        /// <summary>
        /// Oyun başladığında çağrılır
        /// </summary>
        private void OnGameStarted()
        {
            if (isSpectator) return;

            // Mobil input'u aktif et
            if (mobileInputPanel != null)
            {
                mobileInputPanel.SetActive(true);
                Debug.Log("MobileInputManager: Joystick ve Fire butonu aktif edildi");
            }

            // Fire button'u blink yap (yerini göster)
            if (fireButtonImage != null)
            {
                StartCoroutine(BlinkFireButton());
            }

            // Local tank'ı bul ve joystick'i bağla
            FindAndConnectLocalTank();
        }

        /// <summary>
        /// Fire button'u smooth fade ile gösterir (0 -> 0.3 -> 0 şeklinde)
        /// </summary>
        private IEnumerator BlinkFireButton()
        {
            Color color = fireButtonImage.color;

            for (int i = 0; i < blinkCount; i++)
            {
                // Fade in: 0 -> blinkAlpha
                float elapsed = 0f;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / blinkDuration;
                    color.a = Mathf.Lerp(0f, blinkAlpha, t);
                    fireButtonImage.color = color;
                    yield return null;
                }
                color.a = blinkAlpha;
                fireButtonImage.color = color;

                // Fade out: blinkAlpha -> 0
                elapsed = 0f;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / blinkDuration;
                    color.a = Mathf.Lerp(blinkAlpha, 0f, t);
                    fireButtonImage.color = color;
                    yield return null;
                }
                color.a = 0f;
                fireButtonImage.color = color;
            }

            // Son olarak alpha = 0
            color.a = 0f;
            fireButtonImage.color = color;
        }

        /// <summary>
        /// Oyun bittiğinde çağrılır
        /// </summary>
        private void OnGameEnded(int winnerTeamId, int teamAScore, int teamBScore)
        {
            // Mobil input'u gizle
            HideMobileInput();
            Debug.Log("MobileInputManager: Oyun bitti - joystick gizlendi");
        }

        /// <summary>
        /// Local oyuncunun tankını bulur ve joystick'i bağlar
        /// </summary>
        private void FindAndConnectLocalTank()
        {
            // Tüm TankController'ları bul
            TankController[] tanks = FindObjectsOfType<TankController>();

            foreach (var tank in tanks)
            {
                PhotonView pv = tank.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    localTankController = tank;
                    tank.SetJoystick(movementJoystick);
                    Debug.Log("MobileInputManager: Tank'a joystick bağlandı");
                    break;
                }
            }
        }

        /// <summary>
        /// Fire butonu basıldığında
        /// </summary>
        private void OnFireButtonDown()
        {
            if (localTankController != null)
            {
                localTankController.OnFireButtonDown();
            }
        }

        /// <summary>
        /// Fire butonu bırakıldığında
        /// </summary>
        private void OnFireButtonUp()
        {
            if (localTankController != null)
            {
                localTankController.OnFireButtonUp();
            }
        }

        /// <summary>
        /// Hareket joystick'ini döndürür
        /// </summary>
        public Joystick GetMovementJoystick()
        {
            return movementJoystick;
        }

        /// <summary>
        /// Mobil input'u gizler (oyun bittiğinde çağrılabilir)
        /// </summary>
        public void HideMobileInput()
        {
            if (mobileInputPanel != null)
            {
                mobileInputPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Mobil input'u gösterir ve sıfırlar (sahne yenilendiğinde çağrılır)
        /// </summary>
        public void ResetAndShow()
        {
            if (isSpectator) return;

            // Eğer oyun zaten başlamışsa göster
            if (GameController.Instance != null && GameController.Instance.IsGameStarted())
            {
                if (mobileInputPanel != null)
                {
                    mobileInputPanel.SetActive(true);
                }
                FindAndConnectLocalTank();
            }
        }
    }
}

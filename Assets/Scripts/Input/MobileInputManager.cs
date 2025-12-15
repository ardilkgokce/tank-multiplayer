using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TankGame.Game;
using TankGame.Tank;

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

        // Local player'ın tank controller'ı
        private TankController localTankController;
        private bool isSpectator = false;
        private bool isInitialized = false;

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

            // Local tank'ı bul ve joystick'i bağla
            FindAndConnectLocalTank();
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

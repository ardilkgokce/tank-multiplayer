using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using TankGame.Block;
using TankGame.Score;

namespace TankGame.Game
{
    /// <summary>
    /// Oyun akışını kontrol eder.
    /// F1: Oyunu başlat (Master Client)
    /// F5: Sahneyi yenile (Master Client)
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class GameController : MonoBehaviourPunCallbacks
    {
        public static GameController Instance { get; private set; }

        // Room property key
        private const string GAME_PAUSED = "GamePaused";

        // Events
        public static event Action OnGameStarted;
        public static event Action OnGamePaused;
        public static event Action OnSceneReloading;

        [Header("References")]
        [SerializeField] private GameReadyPanel readyPanel;
        [SerializeField] private BlockSpawner blockSpawner;

        [Header("Status UI")]
        [SerializeField] private GameObject masterClientHintPanel;
        [SerializeField] private TMPro.TMP_Text masterClientHintText;

        private bool isGamePaused = true;
        private bool isGameStarted = false;

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
            // Oyun başlangıçta duraklatılmış
            isGamePaused = true;
            isGameStarted = false;

            // Room property'yi ayarla
            if (PhotonNetwork.IsMasterClient)
            {
                SetGamePaused(true);
                PlayerInfo.SetGameStarted(false);
            }

            // Master Client ipuçlarını göster
            UpdateMasterClientHint();

            // Block spawner'ı durdur
            if (blockSpawner != null)
            {
                blockSpawner.StopSpawning();
            }
        }

        private void Update()
        {
            // Sadece Master Client kontrol edebilir
            if (!PhotonNetwork.IsMasterClient) return;

            // F1: Oyunu başlat
            if (Input.GetKeyDown(KeyCode.F1) && !isGameStarted)
            {
                StartGame();
            }

            // F5: Sahneyi yenile
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ReloadScene();
            }
        }

        /// <summary>
        /// Oyunu başlatır (Master Client)
        /// </summary>
        private void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (isGameStarted) return;

            // Her iki takım da hazır mı kontrol et
            if (!PlayerInfo.AreBothTeamsReady())
            {
                Debug.Log("Tüm takımlar hazır değil!");
                if (masterClientHintText != null)
                {
                    masterClientHintText.text = "Tüm takımlar hazır değil!";
                }
                return;
            }

            Debug.Log("Oyun başlatılıyor...");

            // Room property'leri güncelle
            SetGamePaused(false);
            PlayerInfo.SetGameStarted(true);

            isGameStarted = true;
            isGamePaused = false;

            // RPC ile tüm client'lara bildir
            photonView.RPC("RPC_StartGame", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_StartGame()
        {
            isGameStarted = true;
            isGamePaused = false;

            Debug.Log("Oyun başladı!");

            // Ready panelini gizle
            if (readyPanel != null)
            {
                readyPanel.HidePanel();
            }
            else
            {
                // Instance üzerinden bul
                var panel = GameReadyPanel.Instance;
                if (panel != null)
                {
                    panel.HidePanel();
                }
            }

            // Block spawner'ı başlat
            if (blockSpawner != null)
            {
                blockSpawner.StartSpawning();
            }
            else
            {
                // Sahneden bul
                var spawner = FindObjectOfType<BlockSpawner>();
                if (spawner != null)
                {
                    spawner.StartSpawning();
                }
            }

            // Master hint'i gizle
            if (masterClientHintPanel != null)
            {
                masterClientHintPanel.SetActive(false);
            }

            // Event tetikle
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Sahneyi yeniler (Master Client)
        /// </summary>
        private void ReloadScene()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Debug.Log("Sahne yenileniyor...");

            // Takım hazırlık durumlarını sıfırla
            PlayerInfo.ResetTeamReadyStates();
            PlayerInfo.SetGameStarted(false);

            // RPC ile tüm client'lara bildir
            photonView.RPC("RPC_ReloadScene", RpcTarget.All);
        }

        [PunRPC]
        private void RPC_ReloadScene()
        {
            Debug.Log("Sahne yenileniyor (RPC)...");

            // Event tetikle
            OnSceneReloading?.Invoke();

            // Sahneyi yeniden yükle
            PhotonNetwork.LoadLevel("GameScene");
        }

        /// <summary>
        /// Oyunun duraklatılmış olup olmadığını döndürür.
        /// </summary>
        public bool IsGamePaused()
        {
            return isGamePaused || !isGameStarted;
        }

        /// <summary>
        /// Oyunun başlayıp başlamadığını döndürür.
        /// </summary>
        public bool IsGameStarted()
        {
            return isGameStarted;
        }

        /// <summary>
        /// Room property'den oyun durumunu senkronize eder.
        /// </summary>
        private void SetGamePaused(bool paused)
        {
            if (PhotonNetwork.CurrentRoom == null) return;

            Hashtable props = new Hashtable { { GAME_PAUSED, paused } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        private void UpdateMasterClientHint()
        {
            if (masterClientHintPanel != null)
            {
                masterClientHintPanel.SetActive(PhotonNetwork.IsMasterClient);
            }

            if (masterClientHintText != null && PhotonNetwork.IsMasterClient)
            {
                masterClientHintText.text = "F1: Oyunu Başlat | F5: Sahneyi Yenile";
            }
        }

        #region Photon Callbacks

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // Oyun durumu değişti
            if (propertiesThatChanged.ContainsKey(GAME_PAUSED))
            {
                isGamePaused = (bool)propertiesThatChanged[GAME_PAUSED];
                Debug.Log($"Oyun durumu güncellendi: Paused={isGamePaused}");
            }

            if (propertiesThatChanged.ContainsKey(PlayerInfo.GAME_STARTED))
            {
                isGameStarted = (bool)propertiesThatChanged[PlayerInfo.GAME_STARTED];
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            UpdateMasterClientHint();
        }

        #endregion
    }
}

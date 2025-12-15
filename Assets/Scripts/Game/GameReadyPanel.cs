using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

namespace TankGame.Game
{
    /// <summary>
    /// GameScene'de oyuncu/takım ismi giriş paneli.
    /// Oyuncular isim girer, izleyiciler takım ismi girer.
    /// </summary>
    public class GameReadyPanel : MonoBehaviourPunCallbacks
    {
        public static GameReadyPanel Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject readyPanel;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button readyButton;
        [SerializeField] private TMP_Text readyButtonText;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text readyCountText;

        // Events
        public static event Action OnAllPlayersReady;
        public static event Action<string> OnLocalPlayerReady;

        private bool isSpectator = false;
        private int myTeamId = -1;
        private bool isReady = false;

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
            // Oyuncu bilgilerini al
            myTeamId = PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer);
            isSpectator = PlayerInfo.GetRole(PhotonNetwork.LocalPlayer) == PlayerInfo.ROLE_SPECTATOR;

            // GameScene'e girince IS_READY'yi false yap (lobby'de true yapılmıştı)
            PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, false);

            // Panel'i göster
            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
            }

            // UI'ı ayarla
            SetupUI();

            // Button listener
            readyButton.onClick.AddListener(OnReadyClicked);
            nameInput.onValueChanged.AddListener(OnNameChanged);

            // Başlangıçta ready butonu pasif
            readyButton.interactable = false;

            // Hazır sayısını güncelle
            UpdateReadyCount();
        }

        private void SetupUI()
        {
            string teamName = PlayerInfo.GetTeamName(myTeamId);

            if (isSpectator)
            {
                titleText.text = $"{teamName} İzleyicisi";
                descriptionText.text = "Takım ismini girin:";
                nameInput.placeholder.GetComponent<TMP_Text>().text = "Takım ismi...";
            }
            else
            {
                titleText.text = $"{teamName} Oyuncusu";
                descriptionText.text = "Oyuncu isminizi girin:";
                nameInput.placeholder.GetComponent<TMP_Text>().text = "İsminiz...";
            }

            readyButtonText.text = "Hazır";
            statusText.text = "İsim girin ve hazır olun.";
        }

        private void OnNameChanged(string newName)
        {
            // İsim boş değilse ready butonunu aktif et
            readyButton.interactable = !string.IsNullOrWhiteSpace(newName);
        }

        private void OnReadyClicked()
        {
            if (isReady) return;

            string enteredName = nameInput.text.Trim();
            if (string.IsNullOrEmpty(enteredName))
            {
                statusText.text = "Lütfen bir isim girin!";
                return;
            }

            isReady = true;

            // İzleyici takım ismini kaydeder
            if (isSpectator)
            {
                PlayerInfo.SetCustomTeamName(myTeamId, enteredName);
                statusText.text = $"Takım ismi kaydedildi: {enteredName}";
            }
            else
            {
                // Oyuncu kendi ismini kaydeder
                PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.PLAYER_NAME, enteredName);
                statusText.text = $"Hazırsınız: {enteredName}";
            }

            // Ready durumunu güncelle
            PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, true);

            // UI'ı kitle
            nameInput.interactable = false;
            readyButton.interactable = false;
            readyButtonText.text = "Hazır!";

            // Event tetikle
            OnLocalPlayerReady?.Invoke(enteredName);

            // Hazır sayısını güncelle
            UpdateReadyCount();
        }

        private void UpdateReadyCount()
        {
            int totalPlayers = 0;
            int readyPlayers = 0;

            foreach (var player in PhotonNetwork.PlayerList)
            {
                // Sadece oyuncuları say (spectator değil)
                if (PlayerInfo.GetRole(player) == PlayerInfo.ROLE_PLAYER)
                {
                    totalPlayers++;
                    if (PlayerInfo.GetIsReady(player))
                    {
                        readyPlayers++;
                    }
                }
            }

            if (readyCountText != null)
            {
                readyCountText.text = $"Hazır: {readyPlayers}/{totalPlayers}";
            }

            // Tüm oyuncular hazır mı?
            if (totalPlayers > 0 && readyPlayers == totalPlayers)
            {
                statusText.text = "Tüm oyuncular hazır! Master Client F1 ile başlatabilir.";
                OnAllPlayersReady?.Invoke();
            }
        }

        /// <summary>
        /// Paneli gizler (oyun başladığında çağrılır)
        /// </summary>
        public void HidePanel()
        {
            if (readyPanel != null)
            {
                readyPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Paneli gösterir ve sıfırlar (sahne yenilendiğinde çağrılır)
        /// </summary>
        public void ShowAndResetPanel()
        {
            isReady = false;
            nameInput.text = "";
            nameInput.interactable = true;
            readyButton.interactable = false;
            readyButtonText.text = "Hazır";

            // Ready durumunu sıfırla
            PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, false);

            if (readyPanel != null)
            {
                readyPanel.SetActive(true);
            }

            SetupUI();
            UpdateReadyCount();
        }

        #region Photon Callbacks

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // Herhangi bir oyuncunun ready durumu değiştiğinde güncelle
            if (changedProps.ContainsKey(PlayerInfo.IS_READY))
            {
                UpdateReadyCount();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateReadyCount();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            UpdateReadyCount();
        }

        #endregion
    }
}

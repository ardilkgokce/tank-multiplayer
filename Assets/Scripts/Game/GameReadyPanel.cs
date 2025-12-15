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
    /// GameScene'de takım ismi giriş paneli.
    /// Oyuncular takım ismi girer, bir oyuncu hazır olunca tüm takım hazır olur.
    /// İzleyiciler için bu panel hiç açılmaz.
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
        private bool myTeamIsReady = false;

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

            // İzleyiciler için panel hiç açılmaz
            if (isSpectator)
            {
                Debug.Log("GameReadyPanel: İzleyici - panel gizlendi");
                if (readyPanel != null)
                {
                    readyPanel.SetActive(false);
                }
                // İzleyicileri otomatik hazır yap
                PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, true);
                return;
            }

            // GameScene'e girince IS_READY'yi false yap (lobby'de true yapılmıştı)
            PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, false);

            // Takım zaten hazır mı kontrol et
            myTeamIsReady = PlayerInfo.IsTeamReady(myTeamId);
            if (myTeamIsReady)
            {
                // Takım zaten hazır, bu oyuncuyu da hazır yap
                PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, true);
                isReady = true;
            }

            // Panel'i her zaman göster (oyun başlayana kadar açık kalacak)
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

            // Oyuncular takım ismi girer
            titleText.text = $"{teamName} Oyuncusu";
            descriptionText.text = "Takımınız için bir isim girin:";
            nameInput.placeholder.GetComponent<TMP_Text>().text = "Takım ismi...";

            // Eğer takım ismi zaten varsa göster
            string existingTeamName = PlayerInfo.GetCustomTeamName(myTeamId);
            if (!string.IsNullOrEmpty(existingTeamName) && existingTeamName != teamName)
            {
                nameInput.text = existingTeamName;
                nameInput.interactable = false;
                readyButton.interactable = false;
                readyButtonText.text = "Hazır!";
                statusText.text = $"Takım ismi: {existingTeamName}";
            }
            else
            {
                readyButtonText.text = "Hazır";
                statusText.text = "Takım ismi girin. Hazır olunca tüm takım hazır olacak.";
            }
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
                statusText.text = "Lütfen bir takım ismi girin!";
                return;
            }

            isReady = true;

            // Takım ismini kaydet (Room Property olarak)
            PlayerInfo.SetCustomTeamName(myTeamId, enteredName);

            // Takımı hazır olarak işaretle (Room Property)
            PlayerInfo.SetTeamReady(myTeamId, true);

            statusText.text = $"Takım ismi kaydedildi: {enteredName}";

            // Kendi ready durumunu güncelle
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
            // Takım bazlı hazırlık durumunu göster
            bool teamAReady = PlayerInfo.IsTeamReady(PlayerInfo.TEAM_A);
            bool teamBReady = PlayerInfo.IsTeamReady(PlayerInfo.TEAM_B);

            string teamAName = PlayerInfo.GetCustomTeamName(PlayerInfo.TEAM_A);
            string teamBName = PlayerInfo.GetCustomTeamName(PlayerInfo.TEAM_B);

            int readyTeams = 0;
            if (teamAReady) readyTeams++;
            if (teamBReady) readyTeams++;

            if (readyCountText != null)
            {
                readyCountText.text = $"Hazır Takımlar: {readyTeams}/2";
            }

            // Status mesajını duruma göre güncelle
            if (statusText != null)
            {
                if (teamAReady && teamBReady)
                {
                    // Her iki takım da hazır - oyun başlamak üzere
                    statusText.text = "HAZIR OL! Oyun birazdan başlayacak...";
                    OnAllPlayersReady?.Invoke();
                }
                else if (myTeamIsReady)
                {
                    // Sadece kendi takımım hazır - diğer takımı bekliyoruz
                    statusText.text = "Takımınız hazır! Diğer takım bekleniyor...";
                }
                else if (!isReady)
                {
                    // Henüz hazır değiliz
                    statusText.text = "Takım ismi girin. Hazır olunca tüm takım hazır olacak.";
                }
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

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // Takım hazır durumu değişti mi?
            if (propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_A_READY) ||
                propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_B_READY))
            {
                // Kendi takımım hazır olduysa beni de hazır yap (panel kapanmaz!)
                if (!isReady && !isSpectator)
                {
                    bool myTeamNowReady = PlayerInfo.IsTeamReady(myTeamId);
                    if (myTeamNowReady)
                    {
                        isReady = true;
                        myTeamIsReady = true;

                        // Kendimi hazır yap
                        PlayerInfo.UpdatePlayerProperty(PhotonNetwork.LocalPlayer, PlayerInfo.IS_READY, true);

                        // UI'ı güncelle - takım ismi ve bekleme mesajı göster
                        string teamName = PlayerInfo.GetCustomTeamName(myTeamId);
                        nameInput.text = teamName;
                        nameInput.interactable = false;
                        readyButton.interactable = false;
                        readyButtonText.text = "Hazır!";

                        Debug.Log($"Takımım hazır oldu: {teamName}");
                    }
                }

                UpdateReadyCount();
            }

            // Takım ismi değişti mi?
            if (propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_A_NAME) ||
                propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_B_NAME))
            {
                // Eğer kendi takımımın ismi değiştiyse UI'ı güncelle
                if (!isSpectator && !isReady)
                {
                    SetupUI();
                }
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

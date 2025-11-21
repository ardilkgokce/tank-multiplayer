using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

namespace TankGame.Networking
{
    /// <summary>
    /// Photon sunucusuna bağlanma ve oda yönetimi
    /// MenuScene'de kullanılır
    /// </summary>
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private UnityEngine.UI.Button connectButton;
        [SerializeField] private GameObject connectPanel;
        [SerializeField] private GameObject lobbyPanel;

        [Header("Room Settings")]
        [SerializeField] private byte maxPlayersPerRoom = 12; // 10 oyuncu + 2 izleyici

        private void Awake()
        {
            // Sahne senkronizasyonunu aktif et - master client sahne yüklediğinde tüm clientlar da yükler
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            UpdateStatus("Bağlantı için butona tıklayın");

            // Butona bağlan fonksiyonunu ekle
            if (connectButton != null)
            {
                connectButton.onClick.AddListener(Connect);
            }

            // Başlangıçta lobby panelini kapat
            if (lobbyPanel != null)
            {
                lobbyPanel.SetActive(false);
            }
            if (connectPanel != null)
            {
                connectPanel.SetActive(true);
            }

            Debug.Log($"PhotonNetwork.AutomaticallySyncScene = {PhotonNetwork.AutomaticallySyncScene}");
        }

        /// <summary>
        /// Photon sunucusuna bağlan
        /// </summary>
        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                UpdateStatus("Zaten bağlısınız!");
                return;
            }

            UpdateStatus("Photon sunucusuna bağlanılıyor...");

            if (connectButton != null)
            {
                connectButton.interactable = false;
            }

            // Photon sunucusuna bağlan
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1.0";
        }

        #region Photon Callbacks

        /// <summary>
        /// Master sunucuya başarıyla bağlandı
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster: Photon Master sunucusuna bağlandı");
            UpdateStatus("Sunucuya bağlandı. Oda aranıyor...");

            // Random odaya katıl, yoksa oluştur
            PhotonNetwork.JoinRandomRoom();
        }

        /// <summary>
        /// Random oda bulunamadı
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRandomFailed: Oda bulunamadı. Yeni oda oluşturuluyor... ({message})");
            UpdateStatus("Oda bulunamadı. Yeni oda oluşturuluyor...");

            // Yeni oda oluştur
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true
            };

            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        /// <summary>
        /// Odaya başarıyla katıldı
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log($"OnJoinedRoom: Odaya katıldı. Oda: {PhotonNetwork.CurrentRoom.Name}, Oyuncu sayısı: {PhotonNetwork.CurrentRoom.PlayerCount}");
            UpdateStatus($"Odaya katıldınız! Oyuncu: {PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayersPerRoom}");

            // Connect panelini kapat, lobby panelini aç
            if (connectPanel != null)
            {
                connectPanel.SetActive(false);
            }
            if (lobbyPanel != null)
            {
                lobbyPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Yeni oyuncu odaya katıldı
        /// </summary>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom: {newPlayer.NickName} odaya katıldı. Toplam oyuncu: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        /// <summary>
        /// Oyuncu odadan ayrıldı
        /// </summary>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom: {otherPlayer.NickName} odadan ayrıldı. Kalan oyuncu: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        /// <summary>
        /// Bağlantı kesildi
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"OnDisconnected: Photon bağlantısı kesildi. Sebep: {cause}");
            UpdateStatus($"Bağlantı kesildi: {cause}");

            if (connectButton != null)
            {
                connectButton.interactable = true;
            }
        }

        #endregion

        /// <summary>
        /// UI status text'ini güncelle
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[NetworkManager] {message}");
        }
    }
}

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;
using TankGame.Player;

namespace TankGame.Networking
{
    /// <summary>
    /// Oyun sahnesinde oyuncu spawn'lama ve oyun yönetimi
    /// GameScene'de kullanılır - Takım bazlı spawn sistemi
    /// </summary>
    public class TankGameManager : MonoBehaviourPunCallbacks
    {
        [Header("Team A Spawn Points (Sol Bölge)")]
        [SerializeField] private Transform[] teamASpawnPoints;

        [Header("Team B Spawn Points (Sağ Bölge)")]
        [SerializeField] private Transform[] teamBSpawnPoints;

        [Header("Settings")]
        [SerializeField] private float spawnDelay = 0.5f;

        [Header("Spectator")]
        [SerializeField] private GameObject spectatorCameraPrefab; // Resources'ta olmalı

        private void Start()
        {
            Debug.Log("TankGameManager başlatıldı");

            // Spawn noktaları kontrolü
            ValidateSpawnPoints();

            // Oyuncuyu spawn et
            Invoke(nameof(SpawnPlayer), spawnDelay);
        }

        /// <summary>
        /// Spawn noktalarını validate eder.
        /// </summary>
        private void ValidateSpawnPoints()
        {
            if (teamASpawnPoints == null || teamASpawnPoints.Length == 0)
            {
                Debug.LogWarning("Team A spawn noktaları atanmamış!");
            }
            if (teamBSpawnPoints == null || teamBSpawnPoints.Length == 0)
            {
                Debug.LogWarning("Team B spawn noktaları atanmamış!");
            }
        }

        /// <summary>
        /// Oyuncuyu spawn eder (takım ve role göre).
        /// </summary>
        private void SpawnPlayer()
        {
            if (!PhotonNetwork.IsConnected)
            {
                Debug.LogError("Photon'a bağlı değilsiniz! Oyuncu spawn edilemiyor");
                return;
            }

            // Local player'ın team ve role bilgisini al
            PhotonPlayer localPlayer = PhotonNetwork.LocalPlayer;
            string role = PlayerInfo.GetRole(localPlayer);
            int teamID = PlayerInfo.GetTeamID(localPlayer);

            Debug.Log($"Local Player: Role={role}, TeamID={teamID}");

            // Spectator ise spectator camera spawn et
            if (role == PlayerInfo.ROLE_SPECTATOR)
            {
                SpawnSpectatorCamera(teamID);
                return;
            }

            // Oyuncu ise tank spawn et
            SpawnTank(localPlayer, teamID);
        }

        /// <summary>
        /// Tank'ı spawn eder.
        /// </summary>
        private void SpawnTank(PhotonPlayer player, int teamID)
        {
            // Tank prefab ismini al (renk bazlı)
            string tankPrefabName = PlayerInfo.GetTankPrefabName(player);

            // Spawn pozisyonunu al (team bazlı)
            Vector3 spawnPosition = GetTeamSpawnPoint(teamID);

            Debug.Log($"Tank spawn ediliyor: {tankPrefabName} at {spawnPosition} (Team: {PlayerInfo.GetTeamName(teamID)})");

            // Tank'ı network üzerinden instantiate et
            GameObject tank = PhotonNetwork.Instantiate(tankPrefabName, spawnPosition, Quaternion.identity);

            if (tank != null)
            {
                Debug.Log($"Tank başarıyla spawn edildi: {tank.name}");

                // Layer'ı takıma göre ata
                TeamManager.AssignTeamLayer(tank, teamID);

                // Tank'ın PhotonView'ına team bilgisini ekle (sync için)
                PhotonView pv = tank.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    // Custom properties'i tank'a da ekleyebiliriz (isteğe bağlı)
                }

                // Camera'yı bu tank'ı takip edecek şekilde ayarla
                SetupCameraForTank(tank, teamID);
            }
            else
            {
                Debug.LogError($"Tank spawn edilemedi! '{tankPrefabName}' prefabının Resources klasöründe olduğundan emin olun");
            }
        }

        /// <summary>
        /// Spectator camera'sını spawn eder.
        /// </summary>
        private void SpawnSpectatorCamera(int teamID)
        {
            Debug.Log($"Spectator camera spawn ediliyor... (Watching Team: {PlayerInfo.GetTeamName(teamID)})");

            // Spectator kamera prefab'ını spawn et (local only, network'e gerek yok)
            if (spectatorCameraPrefab != null)
            {
                GameObject specCam = Instantiate(spectatorCameraPrefab, Vector3.zero, Quaternion.identity);

                // Spectator camera'yı izleyeceği takıma göre ayarla
                SpectatorController specController = specCam.GetComponent<SpectatorController>();
                if (specController != null)
                {
                    specController.SetWatchingTeam(teamID);
                }

                // Ana kamerayı devre dışı bırak
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    mainCam.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Spectator camera prefab atanmamış!");
            }
        }

        /// <summary>
        /// Takıma göre spawn noktası döner.
        /// </summary>
        private Vector3 GetTeamSpawnPoint(int teamID)
        {
            Transform[] spawnPoints = teamID == PlayerInfo.TEAM_A ? teamASpawnPoints : teamBSpawnPoints;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // Takımdaki oyuncu sayısına göre spawn point seç
                int teamPlayerCount = PlayerInfo.GetTeamPlayerCount(teamID, PlayerInfo.ROLE_PLAYER);
                int spawnIndex = Mathf.Clamp(teamPlayerCount - 1, 0, spawnPoints.Length - 1);

                if (spawnPoints[spawnIndex] != null)
                {
                    return spawnPoints[spawnIndex].position;
                }
            }

            // Default pozisyon
            Debug.LogWarning($"Spawn point bulunamadı! Default pozisyon kullanılıyor.");
            return teamID == PlayerInfo.TEAM_A ? new Vector3(-10, 0, 0) : new Vector3(10, -100, 0);
        }

        /// <summary>
        /// Camera'yı tank için ayarlar.
        /// </summary>
        private void SetupCameraForTank(GameObject tank, int teamID)
        {
            // Ana kamerayı bul
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogWarning("Main Camera bulunamadı!");
                return;
            }

            // CameraFollow script'ini bul
            CameraFollow cameraFollow = mainCam.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(tank.transform);
            }

            // Camera culling mask'ı ayarla (sadece kendi takımını görsün)
            TeamManager.ConfigureCameraForTeam(mainCam, teamID);
        }

        #region Photon Callbacks

        /// <summary>
        /// Yeni oyuncu odaya katıldı
        /// </summary>
        public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
        {
            string playerName = PlayerInfo.GetPlayerName(newPlayer);
            string teamName = PlayerInfo.GetTeamName(PlayerInfo.GetTeamID(newPlayer));
            Debug.Log($"{playerName} ({teamName}) oyuna katıldı. Toplam oyuncu: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        /// <summary>
        /// Oyuncu odadan ayrıldı
        /// </summary>
        public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
        {
            string playerName = PlayerInfo.GetPlayerName(otherPlayer);
            Debug.Log($"{playerName} oyundan ayrıldı. Kalan oyuncu: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        /// <summary>
        /// Odadan ayrıldık
        /// </summary>
        public override void OnLeftRoom()
        {
            Debug.Log("Odadan ayrıldık. Menu sahnesine dönülüyor");
            PhotonNetwork.LoadLevel("MenuScene");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Odadan ayrıl ve menu'ye dön
        /// </summary>
        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        #endregion

        #region Debug Helpers

        private void OnDrawGizmos()
        {
            // Team A spawn noktalarını göster (yeşil)
            if (teamASpawnPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform spawnPoint in teamASpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up);
                    }
                }
            }

            // Team B spawn noktalarını göster (mavi)
            if (teamBSpawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (Transform spawnPoint in teamBSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up);
                    }
                }
            }
        }

        #endregion
    }
}

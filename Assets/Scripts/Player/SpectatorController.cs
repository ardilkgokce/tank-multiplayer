using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PhotonPlayer = Photon.Realtime.Player;

namespace TankGame.Player
{
    /// <summary>
    /// İzleyici (spectator) kamera kontrolü.
    /// Seçilen takımın oyuncularını izler ve aralarında geçiş yapar.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SpectatorController : MonoBehaviour
    {
        [Header("Spectator Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -10);

        [Header("Manual Control")]
        [SerializeField] private bool allowManualControl = true;
        [SerializeField] private float manualMoveSpeed = 10f;

        private Camera cam;
        private int watchingTeamID = -1;
        private List<GameObject> teamTanks = new List<GameObject>();
        private int currentTargetIndex = 0;
        private Transform currentTarget;
        private bool isFollowingPlayer = true;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            // Kamerayı başlat
            if (watchingTeamID != -1)
            {
                Debug.Log($"Spectator camera başlatıldı: {PlayerInfo.GetTeamName(watchingTeamID)} takımı izleniyor");
                TeamManager.ConfigureSpectatorCamera(cam, watchingTeamID);
                FindTeamTanks();
            }
            else
            {
                Debug.LogWarning("Spectator camera için takım ID atanmamış!");
            }
        }

        private void Update()
        {
            // Input handling
            HandleInput();

            // Kamera hareketi
            if (isFollowingPlayer && currentTarget != null)
            {
                FollowTarget();
            }
            else if (allowManualControl)
            {
                ManualCameraControl();
            }
        }

        /// <summary>
        /// İzlenecek takımı ayarlar.
        /// </summary>
        public void SetWatchingTeam(int teamID)
        {
            watchingTeamID = teamID;
            Debug.Log($"İzlenen takım: {PlayerInfo.GetTeamName(teamID)}");

            // Kamera culling mask'ı ayarla
            if (cam != null)
            {
                TeamManager.ConfigureSpectatorCamera(cam, teamID);
            }

            // Takım tanklarını bul
            FindTeamTanks();
        }

        /// <summary>
        /// İzlenecek takımın tanklarını bulur.
        /// </summary>
        private void FindTeamTanks()
        {
            teamTanks.Clear();

            // Tüm PhotonView'ları tara
            PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();

            foreach (PhotonView pv in allPhotonViews)
            {
                PhotonPlayer owner = pv.Owner;
                if (owner != null)
                {
                    int teamID = PlayerInfo.GetTeamID(owner);
                    string role = PlayerInfo.GetRole(owner);

                    // Sadece izlediğimiz takımın oyuncularını ekle
                    if (teamID == watchingTeamID && role == PlayerInfo.ROLE_PLAYER)
                    {
                        teamTanks.Add(pv.gameObject);
                    }
                }
            }

            Debug.Log($"{PlayerInfo.GetTeamName(watchingTeamID)} takımında {teamTanks.Count} tank bulundu");

            // İlk tankı seç
            if (teamTanks.Count > 0)
            {
                currentTargetIndex = 0;
                SetTarget(teamTanks[currentTargetIndex].transform);
            }
        }

        /// <summary>
        /// Input'ları kontrol eder.
        /// </summary>
        private void HandleInput()
        {
            // Tab tuşu ile bir sonraki oyuncuya geç
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchToNextPlayer();
            }

            // Sayı tuşları ile direkt oyuncu seçimi (1-5)
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchToPlayer(i);
                }
            }

            // Space ile follow/manual toggle
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isFollowingPlayer = !isFollowingPlayer;
                Debug.Log($"Spectator mode: {(isFollowingPlayer ? "Following Player" : "Manual Control")}");
            }

            // R tuşu ile tank listesini yenile
            if (Input.GetKeyDown(KeyCode.R))
            {
                FindTeamTanks();
            }
        }

        /// <summary>
        /// Bir sonraki oyuncuya geçer.
        /// </summary>
        private void SwitchToNextPlayer()
        {
            if (teamTanks.Count == 0)
            {
                FindTeamTanks();
                return;
            }

            currentTargetIndex = (currentTargetIndex + 1) % teamTanks.Count;

            if (teamTanks[currentTargetIndex] != null)
            {
                SetTarget(teamTanks[currentTargetIndex].transform);
            }
            else
            {
                // Tank yok edilmiş, listeyi yenile
                FindTeamTanks();
            }
        }

        /// <summary>
        /// Belirli bir oyuncuya geçer (index bazlı).
        /// </summary>
        private void SwitchToPlayer(int index)
        {
            if (index < 0 || index >= teamTanks.Count)
            {
                Debug.LogWarning($"Geçersiz oyuncu index: {index}");
                return;
            }

            currentTargetIndex = index;

            if (teamTanks[currentTargetIndex] != null)
            {
                SetTarget(teamTanks[currentTargetIndex].transform);
            }
            else
            {
                FindTeamTanks();
            }
        }

        /// <summary>
        /// Hedef tankı ayarlar.
        /// </summary>
        private void SetTarget(Transform target)
        {
            currentTarget = target;
            isFollowingPlayer = true;

            if (target != null)
            {
                PhotonView pv = target.GetComponent<PhotonView>();
                if (pv != null && pv.Owner != null)
                {
                    string playerName = PlayerInfo.GetPlayerName(pv.Owner);
                    Debug.Log($"Izlenen oyuncu: {playerName}");
                }
            }
        }

        /// <summary>
        /// Hedefi takip eder (smooth camera movement).
        /// </summary>
        private void FollowTarget()
        {
            if (currentTarget == null)
            {
                // Hedef yok edilmiş, yeni hedef bul
                FindTeamTanks();
                return;
            }

            Vector3 targetPosition = currentTarget.position + cameraOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }

        /// <summary>
        /// Manuel kamera kontrolü (WASD ile).
        /// </summary>
        private void ManualCameraControl()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, vertical, 0) * manualMoveSpeed * Time.deltaTime;
            transform.position += movement;
        }

        private void OnGUI()
        {
            // Basit UI overlay (debug için)
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            string modeText = isFollowingPlayer ? "Following Player" : "Manual Control";
            string teamText = $"Watching: {PlayerInfo.GetTeamName(watchingTeamID)}";
            string playerText = "";

            if (currentTarget != null)
            {
                PhotonView pv = currentTarget.GetComponent<PhotonView>();
                if (pv != null && pv.Owner != null)
                {
                    playerText = $"Player: {PlayerInfo.GetPlayerName(pv.Owner)} ({currentTargetIndex + 1}/{teamTanks.Count})";
                }
            }

            GUI.Label(new Rect(10, 10, 400, 30), $"SPECTATOR MODE | {modeText}", style);
            GUI.Label(new Rect(10, 40, 400, 30), teamText, style);
            GUI.Label(new Rect(10, 70, 400, 30), playerText, style);
            GUI.Label(new Rect(10, 100, 400, 30), "Controls: Tab (next), Space (toggle), R (refresh), 1-5 (select)", style);
        }
    }
}

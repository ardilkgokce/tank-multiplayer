using UnityEngine;
using Photon.Pun;

namespace TankGame.Tank
{
    /// <summary>
    /// İzleyici (spectator) sabit kamera kontrolü.
    /// Takıma göre sabit pozisyonda durur, hareket etmez.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class SpectatorController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -10);

        private Camera cam;
        private int watchingTeamID = -1;

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
                SetCameraPosition();
            }
            else
            {
                Debug.LogWarning("Spectator camera için takım ID atanmamış!");
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

            // Kamera pozisyonunu ayarla
            SetCameraPosition();
        }

        /// <summary>
        /// Kamerayı takıma göre sabit pozisyona yerleştirir.
        /// </summary>
        private void SetCameraPosition()
        {
            // TeamManager'dan takımın kamera pozisyonunu al
            Vector3 teamCameraPos = TeamManager.GetTeamCameraPosition(watchingTeamID);
            transform.position = teamCameraPos + cameraOffset;

            Debug.Log($"Spectator kamera pozisyonu: {transform.position}");
        }
    }
}

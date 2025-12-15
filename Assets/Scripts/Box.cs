using UnityEngine;
using Photon.Pun;
using TankGame.Tank;
using TankGame.Score;
using TankGame.UI;

namespace TankGame
{
    /// <summary>
    /// Box davranışı - Renk bazlı yok edilebilir
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class Box : MonoBehaviourPun
    {
        [Header("Box Settings")]
        [SerializeField] private TankColor boxColor;

        /// <summary>
        /// Box'ın rengini döndürür
        /// </summary>
        public TankColor GetColor()
        {
            return boxColor;
        }

        /// <summary>
        /// Box'ı yok eder (tüm clientlarda aynı anda)
        /// Scene object olduğu için normal Destroy kullanılır
        /// </summary>
        [PunRPC]
        public void DestroyBox()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Box'ı yok etmek için çağrılır
        /// Tüm clientlara RPC gönderir
        /// </summary>
        public void RequestDestroy()
        {
            photonView.RPC(nameof(DestroyBox), RpcTarget.All);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tank mı kontrol et
            TankController tank = other.GetComponent<TankController>();
            if (tank == null) return;

            // Tank'ın PhotonView'ını al
            PhotonView tankPV = other.GetComponent<PhotonView>();
            if (tankPV == null) return;

            // Sadece tank sahibi işlemi yapsın (duplicate önleme)
            if (!tankPV.IsMine) return;

            // Takımdan puan düş
            int teamId = PlayerInfo.GetTeamID(tankPV.Owner);
            int points = 0;
            if (ScoreManager.Instance != null)
            {
                points = ScoreManager.Instance.GetPointsPerBox();
                ScoreManager.Instance.SubtractScore(teamId);
                Debug.Log($"Box tank'a çarptı! Takım {teamId} puan kaybetti.");
            }

            // Floating text göster (negatif puan) - tüm clientlarda
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowFloatingText(-points, transform.position);
            }

            // Box'ı yok et
            RequestDestroy();
        }
    }
}

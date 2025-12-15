using UnityEngine;
using Photon.Pun;

namespace TankGame.UI
{
    /// <summary>
    /// FloatingText'leri tüm clientlarda senkronize eden manager
    /// RPC ile tüm oyunculara floating text gösterir
    /// </summary>
    public class FloatingTextManager : MonoBehaviourPun
    {
        public static FloatingTextManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Tüm clientlarda floating text gösterir
        /// </summary>
        /// <param name="points">Puan değeri (pozitif veya negatif)</param>
        /// <param name="position">World pozisyonu</param>
        public void ShowFloatingText(int points, Vector3 position)
        {
            // RPC ile tüm clientlara gönder
            photonView.RPC(nameof(RPC_ShowFloatingText), RpcTarget.All, points, position);
        }

        [PunRPC]
        private void RPC_ShowFloatingText(int points, Vector3 position)
        {
            // Lokal olarak floating text göster
            FloatingText.Spawn(points, position);
        }
    }
}

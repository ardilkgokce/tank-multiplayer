using UnityEngine;
using Photon.Pun;

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
    }
}

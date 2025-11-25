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
        /// Box'ı yok eder (RPC ile tüm clientlarda)
        /// </summary>
        [PunRPC]
        public void DestroyBox()
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        /// <summary>
        /// Box'ı yok etmek için çağrılır
        /// </summary>
        public void RequestDestroy()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                photonView.RPC(nameof(DestroyBox), RpcTarget.MasterClient);
            }
        }
    }
}

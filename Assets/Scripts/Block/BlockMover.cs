using UnityEngine;
using Photon.Pun;

namespace TankGame.Block
{
    /// <summary>
    /// Blokları sola hareket ettirir
    /// Ekrandan çıkınca otomatik yok eder
    /// Hız ve destroy pozisyonu instantiation data ile senkronize edilir
    /// </summary>
    public class BlockMover : MonoBehaviourPun
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Destroy Settings")]
        [Tooltip("Bu X pozisyonunun soluna geçince blok yok edilir")]
        [SerializeField] private float destroyXPosition = -50f;

        private bool isDestroyed = false;

        private void Start()
        {
            // Instantiation data'dan hız ve destroy pozisyonunu al
            if (photonView.InstantiationData != null && photonView.InstantiationData.Length >= 2)
            {
                moveSpeed = (float)photonView.InstantiationData[0];
                destroyXPosition = (float)photonView.InstantiationData[1];
            }
        }

        private void Update()
        {
            // Sola doğru hareket et
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

            // Ekrandan çıktıysa yok et (sadece Master Client)
            if (!isDestroyed && transform.position.x < destroyXPosition)
            {
                isDestroyed = true;

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
    }
}

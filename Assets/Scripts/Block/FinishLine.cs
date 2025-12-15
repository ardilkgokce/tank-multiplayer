using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TankGame.Tank;
using TankGame.Game;

namespace TankGame.Block
{
    /// <summary>
    /// Finish Line - Sola hareket eder
    /// Takımdaki tüm tanklara değince (geçince) takım finish olur
    /// Oyun başlayana kadar hareket etmez
    /// </summary>
    public class FinishLine : MonoBehaviourPun
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Destroy Settings")]
        [Tooltip("Bu X pozisyonunun soluna geçince finish line yok edilir")]
        [SerializeField] private float destroyXPosition = -50f;

        [Header("Finish Settings")]
        [Tooltip("Finish için gerekli tank sayısı")]
        [SerializeField] private int requiredTankCount = 5;

        // Finish eventi - dışarıdan subscribe edilebilir
        public static event Action<int> OnTeamFinished;

        private int teamId = -1;
        private HashSet<int> passedTanks = new HashSet<int>(); // Geçen tankların PhotonView ID'leri
        private bool hasFinished = false;
        private bool isDestroyed = false;
        private bool canMove = false;

        private void Start()
        {
            // Instantiation data'dan ayarları al
            // [0] = moveSpeed, [1] = destroyXPosition, [2] = requiredTankCount, [3] = teamId
            if (photonView.InstantiationData != null && photonView.InstantiationData.Length >= 4)
            {
                moveSpeed = (float)photonView.InstantiationData[0];
                destroyXPosition = (float)photonView.InstantiationData[1];
                requiredTankCount = (int)photonView.InstantiationData[2];
                teamId = (int)photonView.InstantiationData[3];
                Debug.Log($"FinishLine başlatıldı. TeamId: {teamId}, Gereken tank: {requiredTankCount}");
            }
            else
            {
                Debug.LogWarning($"FinishLine: InstantiationData null veya eksik! Varsayılan değerler kullanılıyor. Data: {photonView.InstantiationData?.Length ?? 0}");
            }

            // Oyun başlama event'ine subscribe ol
            GameController.OnGameStarted += OnGameStarted;

            // Oyun zaten başlamışsa hareket aktif
            if (GameController.Instance != null && GameController.Instance.IsGameStarted())
            {
                canMove = true;
            }
        }

        private void OnDestroy()
        {
            GameController.OnGameStarted -= OnGameStarted;
        }

        private void OnGameStarted()
        {
            canMove = true;
        }

        private void Update()
        {
            // Oyun başlamadıysa hareket etme
            if (!canMove) return;

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Zaten finish olduysa işlem yapma
            if (hasFinished) return;

            // Tank mı kontrol et
            TankController tank = other.GetComponent<TankController>();
            if (tank == null) return;

            // Tank'ın PhotonView'ını al
            PhotonView tankPV = other.GetComponent<PhotonView>();
            if (tankPV == null) return;

            // Tank'ın takımını kontrol et (sadece aynı takımdaki tankları say)
            int tankTeamId = PlayerInfo.GetTeamID(tankPV.Owner);
            if (tankTeamId != teamId) return;

            // Bu tankı daha önce saymadıysak ekle
            if (!passedTanks.Contains(tankPV.ViewID))
            {
                passedTanks.Add(tankPV.ViewID);
                Debug.Log($"Tank geçti! TeamId: {teamId}, Geçen: {passedTanks.Count}/{requiredTankCount}");

                // Tüm tanklar geçtiyse finish
                if (passedTanks.Count >= requiredTankCount)
                {
                    hasFinished = true;
                    Debug.Log($"FINISH! Takım {teamId} tamamladı!");
                    OnTeamFinished?.Invoke(teamId);
                }
            }
        }
    }
}

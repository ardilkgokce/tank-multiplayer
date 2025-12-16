using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using TankGame.Game;

namespace TankGame.Block
{
    /// <summary>
    /// Blokları belirli aralıklarla spawn eder
    /// Master Client spawn işlemini yapar, tüm clientlara senkronize olur
    /// Her iki takım için aynı blok aynı anda spawn edilir
    /// Oyun başlayana kadar spawn yapmaz
    /// </summary>
    public class BlockSpawner : MonoBehaviourPun
    {
        [Header("Spawn Points")]
        [Tooltip("Takım A için spawn noktası")]
        [SerializeField] private Transform spawnPointTeamA;
        [Tooltip("Takım B için spawn noktası")]
        [SerializeField] private Transform spawnPointTeamB;

        [Header("Block Prefabs")]
        [Tooltip("Spawn edilecek blok prefab isimleri (Resources klasöründe olmalı)")]
        [SerializeField] private List<string> blockPrefabNames = new List<string>();

        [Header("Finish Line")]
        [Tooltip("Finish Line prefab ismi (Resources klasöründe olmalı)")]
        [SerializeField] private string finishLinePrefabName = "FinishLine";

        [Header("Spawn Settings")]
        [Tooltip("İlk spawn'dan önce bekleme süresi")]
        [SerializeField] private float initialDelay = 3f;
        [Tooltip("İki spawn arasındaki süre (saniye)")]
        [SerializeField] private float spawnInterval = 4f;
        [Tooltip("Blokların hareket hızı")]
        [SerializeField] private float blockSpeed = 5f;

        [Header("Destroy Settings")]
        [Tooltip("Bu X pozisyonunun soluna geçince blok yok edilir")]
        [SerializeField] private float destroyXPosition = -50f;

        [Header("Finish Line Settings")]
        [Tooltip("Finish için gerekli tank sayısı (takım başına)")]
        [SerializeField] private int requiredTankCount = 5;

        // Şu anki prefab index'i (sırayla spawn için)
        private int currentPrefabIndex = 0;
        private float nextSpawnTime;
        private bool isSpawning = false;
        private bool finishLineSpawned = false;

        private void Start()
        {
            // Başlangıçta spawn yapma, GameController.StartSpawning() ile başlatılacak
            isSpawning = false;
            Debug.Log("BlockSpawner başlatıldı. Oyun başlayana kadar bekleniyor...");
        }

        private void Update()
        {
            // Sadece Master Client spawn yapsın
            if (!PhotonNetwork.IsMasterClient || !isSpawning) return;

            // Spawn zamanı geldiyse
            if (Time.time >= nextSpawnTime)
            {
                SpawnBlock();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }

        /// <summary>
        /// Her iki takım için blok spawn eder
        /// Tüm bloklar spawn edildikten sonra FinishLine spawn eder
        /// </summary>
        private void SpawnBlock()
        {
            if (blockPrefabNames.Count == 0)
            {
                Debug.LogWarning("BlockSpawner: Prefab listesi boş!");
                return;
            }

            string prefabName = blockPrefabNames[currentPrefabIndex];

            // Takım A için spawn
            if (spawnPointTeamA != null)
            {
                SpawnBlockAtPosition(prefabName, spawnPointTeamA.position);
            }

            // Takım B için spawn
            if (spawnPointTeamB != null)
            {
                SpawnBlockAtPosition(prefabName, spawnPointTeamB.position);
            }

            Debug.Log($"Blok spawn edildi: {prefabName} (Index: {currentPrefabIndex + 1}/{blockPrefabNames.Count})");

            // Sonraki prefab'a geç
            currentPrefabIndex++;

            // Tüm bloklar spawn edildiyse, bir sonraki spawn zamanında FinishLine spawn et
            if (currentPrefabIndex >= blockPrefabNames.Count)
            {
                isSpawning = false;
                Invoke(nameof(SpawnFinishLine), spawnInterval);
            }
        }

        /// <summary>
        /// Her iki takım için FinishLine spawn eder
        /// </summary>
        private void SpawnFinishLine()
        {
            if (finishLineSpawned) return;
            if (string.IsNullOrEmpty(finishLinePrefabName)) return;

            finishLineSpawned = true;
            isSpawning = false;

            Debug.Log($"FinishLine spawn ediliyor. RequiredTankCount: {requiredTankCount}");

            // Takım A için FinishLine spawn
            // [0] = moveSpeed, [1] = destroyXPosition, [2] = requiredTankCount, [3] = teamId
            if (spawnPointTeamA != null)
            {
                object[] dataTeamA = new object[] { blockSpeed, destroyXPosition, requiredTankCount, 0 };
                PhotonNetwork.Instantiate(finishLinePrefabName, spawnPointTeamA.position, Quaternion.identity, 0, dataTeamA);
            }

            // Takım B için FinishLine spawn
            if (spawnPointTeamB != null)
            {
                object[] dataTeamB = new object[] { blockSpeed, destroyXPosition, requiredTankCount, 1 };
                PhotonNetwork.Instantiate(finishLinePrefabName, spawnPointTeamB.position, Quaternion.identity, 0, dataTeamB);
            }

            Debug.Log("FinishLine spawn edildi! Blok spawning durdu.");
        }

        /// <summary>
        /// Belirtilen pozisyonda blok spawn eder
        /// </summary>
        private void SpawnBlockAtPosition(string prefabName, Vector3 position)
        {
            // Hız ve destroy pozisyonunu instantiation data olarak gönder
            object[] instantiationData = new object[] { blockSpeed, destroyXPosition };

            // Network üzerinden spawn et
            PhotonNetwork.Instantiate(prefabName, position, Quaternion.identity, 0, instantiationData);
        }

        /// <summary>
        /// Spawn işlemini durdurur
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Spawn işlemini başlatır
        /// </summary>
        public void StartSpawning()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Sahne yenilendiğinde sıfırla
                currentPrefabIndex = 0;
                finishLineSpawned = false;

                isSpawning = true;
                // İlk blok hemen spawn olsun, sonrakiler spawnInterval ile
                nextSpawnTime = Time.time;
                Debug.Log("BlockSpawner spawn başlatıldı. İlk blok hemen spawn oluyor.");
            }
        }

        /// <summary>
        /// Spawn aralığını değiştirir
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = interval;
        }

        /// <summary>
        /// Blok hızını değiştirir
        /// </summary>
        public void SetBlockSpeed(float speed)
        {
            blockSpeed = speed;
        }

        #region Debug Helpers

        private void OnDrawGizmos()
        {
            // Spawn noktalarını göster
            Gizmos.color = Color.green;
            if (spawnPointTeamA != null)
            {
                Gizmos.DrawWireCube(spawnPointTeamA.position, new Vector3(2f, 10f, 0f));
                Gizmos.DrawLine(spawnPointTeamA.position, spawnPointTeamA.position + Vector3.left * 20f);
            }

            Gizmos.color = Color.blue;
            if (spawnPointTeamB != null)
            {
                Gizmos.DrawWireCube(spawnPointTeamB.position, new Vector3(2f, 10f, 0f));
                Gizmos.DrawLine(spawnPointTeamB.position, spawnPointTeamB.position + Vector3.left * 20f);
            }

            // Destroy pozisyonunu göster
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(destroyXPosition, -200f, 0f), new Vector3(destroyXPosition, 200f, 0f));
        }

        #endregion
    }
}

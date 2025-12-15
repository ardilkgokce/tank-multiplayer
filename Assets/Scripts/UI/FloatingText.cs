using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace TankGame.UI
{
    /// <summary>
    /// Puan kazanma/kaybetme için floating text efekti
    /// Object pooling kullanır - performans için
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float lifetime = 1f;

        [Header("Colors")]
        [SerializeField] private Color gainColor = Color.green;
        [SerializeField] private Color loseColor = Color.red;

        private TextMeshPro textMesh;
        private float elapsedTime = 0f;
        private int direction = 1; // 1 = yukarı, -1 = aşağı
        private Color startColor;
        private bool isActive = false;

        // Object Pool
        private static Queue<FloatingText> pool = new Queue<FloatingText>();
        private static GameObject prefabInstance;
        private static Transform poolParent;
        private const int INITIAL_POOL_SIZE = 20;
        private const string PREFAB_NAME = "FloatingText";

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        /// <summary>
        /// Pool'u başlatır (ilk kullanımda otomatik çağrılır)
        /// </summary>
        private static void InitializePool()
        {
            if (poolParent != null) return;

            // Pool parent oluştur
            GameObject parentGO = new GameObject("FloatingTextPool");
            poolParent = parentGO.transform;
            DontDestroyOnLoad(parentGO);

            // Prefab'ı yükle
            prefabInstance = Resources.Load<GameObject>(PREFAB_NAME);
            if (prefabInstance == null)
            {
                Debug.LogError($"FloatingText: '{PREFAB_NAME}' prefab'ı Resources klasöründe bulunamadı!");
                return;
            }

            // Başlangıç pool'unu oluştur
            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                CreateNewInstance();
            }
        }

        /// <summary>
        /// Yeni bir FloatingText instance'ı oluşturur ve pool'a ekler
        /// </summary>
        private static FloatingText CreateNewInstance()
        {
            if (prefabInstance == null) return null;

            GameObject go = Instantiate(prefabInstance, poolParent);
            go.SetActive(false);
            FloatingText ft = go.GetComponent<FloatingText>();
            if (ft != null)
            {
                pool.Enqueue(ft);
            }
            return ft;
        }

        /// <summary>
        /// Pool'dan bir FloatingText alır veya yeni oluşturur
        /// </summary>
        private static FloatingText GetFromPool()
        {
            // Pool boşsa yeni oluştur
            if (pool.Count == 0)
            {
                CreateNewInstance();
            }

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return null;
        }

        /// <summary>
        /// FloatingText'i pool'a geri döndürür
        /// </summary>
        private void ReturnToPool()
        {
            isActive = false;
            gameObject.SetActive(false);

            // Pool parent hala varsa geri ekle
            if (poolParent != null)
            {
                transform.SetParent(poolParent);
                pool.Enqueue(this);
            }
            else
            {
                // Pool yok edilmişse (sahne değişimi) kendini yok et
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Floating text'i başlatır
        /// </summary>
        /// <param name="points">Puan değeri (pozitif = kazanç, negatif = kayıp)</param>
        /// <param name="position">Spawn pozisyonu</param>
        public void Initialize(int points, Vector3 position)
        {
            transform.position = position;
            transform.SetParent(null); // Pool parent'tan çıkar
            elapsedTime = 0f;
            isActive = true;

            // Puan değerine göre ayarla
            if (points >= 0)
            {
                textMesh.text = $"+{points}";
                textMesh.color = gainColor;
                direction = 1; // Yukarı hareket
            }
            else
            {
                textMesh.text = points.ToString();
                textMesh.color = loseColor;
                direction = -1; // Aşağı hareket
            }

            startColor = textMesh.color;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!isActive) return;

            elapsedTime += Time.deltaTime;

            // Yukarı veya aşağı hareket
            transform.Translate(Vector3.up * direction * moveSpeed * Time.deltaTime);

            // Fade out
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / lifetime);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            // Süre dolunca pool'a geri dön
            if (elapsedTime >= lifetime)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// Statik factory metodu - kolayca floating text oluşturmak için
        /// Object pooling kullanır
        /// </summary>
        public static void Spawn(int points, Vector3 position)
        {
            // Pool'u başlat (ilk kullanımda)
            InitializePool();

            // Pool'dan al
            FloatingText ft = GetFromPool();
            if (ft != null)
            {
                ft.Initialize(points, position);
            }
            else
            {
                Debug.LogWarning("FloatingText: Pool'dan instance alınamadı!");
            }
        }

        /// <summary>
        /// Sahne değişiminde pool'u temizler
        /// </summary>
        public static void ClearPool()
        {
            pool.Clear();
            if (poolParent != null)
            {
                Destroy(poolParent.gameObject);
                poolParent = null;
            }
            prefabInstance = null;
        }
    }
}

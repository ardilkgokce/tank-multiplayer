using UnityEngine;
using Photon.Pun;
using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace TankGame.Score
{
    /// <summary>
    /// Takım skorlarını yönetir
    /// Photon Room Properties ile senkronize edilir
    /// </summary>
    public class ScoreManager : MonoBehaviourPunCallbacks
    {
        public static ScoreManager Instance { get; private set; }

        // Room property keys
        private const string SCORE_TEAM_A = "ScoreTeamA";
        private const string SCORE_TEAM_B = "ScoreTeamB";

        [Header("Score Settings")]
        [Tooltip("Kutu yok etme başına kazanılan puan")]
        [SerializeField] private int pointsPerBox = 10;

        // Skor değiştiğinde tetiklenen event
        public static event Action<int, int> OnScoreChanged; // (teamAScore, teamBScore)

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
                return;
            }
        }

        private void Start()
        {
            // Oyun başında skorları sıfırla (sadece Master Client)
            if (PhotonNetwork.IsMasterClient)
            {
                ResetScores();
            }
        }

        /// <summary>
        /// Skorları sıfırlar
        /// </summary>
        public void ResetScores()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            Hashtable props = new Hashtable
            {
                { SCORE_TEAM_A, 0 },
                { SCORE_TEAM_B, 0 }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        /// <summary>
        /// Belirtilen takıma puan ekler
        /// </summary>
        public void AddScore(int teamId, int points = -1)
        {
            if (points < 0) points = pointsPerBox;

            // Mevcut skoru al ve yeni skoru hesapla
            int currentScore = GetTeamScore(teamId);
            int newScore = currentScore + points;

            // Room property'yi güncelle
            string key = teamId == 0 ? SCORE_TEAM_A : SCORE_TEAM_B;
            Hashtable props = new Hashtable { { key, newScore } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            Debug.Log($"Skor eklendi! Takım {teamId}: {currentScore} -> {newScore} (+{points})");
        }

        /// <summary>
        /// Belirtilen takımın skorunu döndürür
        /// </summary>
        public int GetTeamScore(int teamId)
        {
            if (PhotonNetwork.CurrentRoom == null) return 0;

            string key = teamId == 0 ? SCORE_TEAM_A : SCORE_TEAM_B;
            object score;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out score))
            {
                return (int)score;
            }
            return 0;
        }

        /// <summary>
        /// Takım A skorunu döndürür
        /// </summary>
        public int GetTeamAScore() => GetTeamScore(0);

        /// <summary>
        /// Takım B skorunu döndürür
        /// </summary>
        public int GetTeamBScore() => GetTeamScore(1);

        /// <summary>
        /// Kutu başına puanı döndürür
        /// </summary>
        public int GetPointsPerBox() => pointsPerBox;

        #region Photon Callbacks

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // Skor değiştiğinde event tetikle
            if (propertiesThatChanged.ContainsKey(SCORE_TEAM_A) || propertiesThatChanged.ContainsKey(SCORE_TEAM_B))
            {
                int teamAScore = GetTeamAScore();
                int teamBScore = GetTeamBScore();
                OnScoreChanged?.Invoke(teamAScore, teamBScore);
                Debug.Log($"Skor güncellendi - Takım A: {teamAScore}, Takım B: {teamBScore}");
            }
        }

        #endregion
    }
}

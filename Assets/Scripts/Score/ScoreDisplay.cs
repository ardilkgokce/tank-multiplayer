using UnityEngine;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace TankGame.Score
{
    /// <summary>
    /// 2D World Space'te skor gösterimi
    /// TextMeshPro kullanır
    /// Takım ismini de gösterir
    /// </summary>
    public class ScoreDisplay : MonoBehaviourPunCallbacks
    {
        [Header("Team Settings")]
        [Tooltip("Bu display hangi takıma ait? 0 = Team A, 1 = Team B")]
        [SerializeField] private int teamId = 0;

        [Header("Display Settings")]
        [Tooltip("Skor text componenti")]
        [SerializeField] private TextMeshPro scoreText;

        [Header("Team Name Display")]
        [Tooltip("Takım ismi text componenti (opsiyonel)")]
        [SerializeField] private TextMeshPro teamNameText;

        [Header("Format")]
        [Tooltip("Skor format stringi. {0} = takım ismi, {1} = skor değeri")]
        [SerializeField] private string scoreFormat = "{0}: {1}";

        private void Start()
        {
            // Event'e subscribe ol
            ScoreManager.OnScoreChanged += OnScoreChanged;

            // Başlangıç değerlerini göster
            UpdateTeamName();
            UpdateDisplay(0);
        }

        private void OnDestroy()
        {
            // Event'ten unsubscribe ol
            ScoreManager.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int teamAScore, int teamBScore)
        {
            int score = teamId == 0 ? teamAScore : teamBScore;
            UpdateDisplay(score);
        }

        private void UpdateDisplay(int score)
        {
            if (scoreText != null)
            {
                string teamName = PlayerInfo.GetCustomTeamName(teamId);
                scoreText.text = string.Format(scoreFormat, teamName, score);
            }
        }

        private void UpdateTeamName()
        {
            if (teamNameText != null)
            {
                string teamName = PlayerInfo.GetCustomTeamName(teamId);
                teamNameText.text = teamName;
            }
        }

        /// <summary>
        /// TextMeshPro componentini ayarlar (Inspector'dan atanmadıysa)
        /// </summary>
        public void SetScoreText(TextMeshPro text)
        {
            scoreText = text;
        }

        /// <summary>
        /// Takım ID'sini ayarlar
        /// </summary>
        public void SetTeamId(int id)
        {
            teamId = id;
        }

        #region Photon Callbacks

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // Takım ismi değiştiğinde güncelle
            if (propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_A_NAME) ||
                propertiesThatChanged.ContainsKey(PlayerInfo.TEAM_B_NAME))
            {
                UpdateTeamName();
                // Skoru da güncelle (takım ismi değiştiği için)
                if (ScoreManager.Instance != null)
                {
                    int score = ScoreManager.Instance.GetTeamScore(teamId);
                    UpdateDisplay(score);
                }
            }
        }

        #endregion
    }
}

using UnityEngine;
using TMPro;

namespace TankGame.Score
{
    /// <summary>
    /// 2D World Space'te skor gösterimi
    /// TextMeshPro kullanır
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        [Header("Team Settings")]
        [Tooltip("Bu display hangi takıma ait? 0 = Team A, 1 = Team B")]
        [SerializeField] private int teamId = 0;

        [Header("Display Settings")]
        [Tooltip("Skor text componenti")]
        [SerializeField] private TextMeshPro scoreText;

        [Header("Format")]
        [Tooltip("Skor format stringi. {0} = skor değeri")]
        [SerializeField] private string scoreFormat = "SKOR: {0}";

        private void Start()
        {
            // Event'e subscribe ol
            ScoreManager.OnScoreChanged += OnScoreChanged;

            // Başlangıç skorunu göster
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
                scoreText.text = string.Format(scoreFormat, score);
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
    }
}

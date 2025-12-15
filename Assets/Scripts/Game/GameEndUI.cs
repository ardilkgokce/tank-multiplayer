using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace TankGame.Game
{
    /// <summary>
    /// Oyun sonu ekranı
    /// Takım ismiyle kazanan gösterimi ve skor tablosu
    /// F5 ile sahne yenileme ipucu gösterir
    /// </summary>
    public class GameEndUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject endGamePanel;

        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI winnerTeamText;
        [SerializeField] private TextMeshProUGUI teamAScoreText;
        [SerializeField] private TextMeshProUGUI teamBScoreText;

        [Header("Colors")]
        [SerializeField] private Color winColor = Color.green;
        [SerializeField] private Color loseColor = Color.red;
        [SerializeField] private Color drawColor = Color.yellow;

        [Header("Messages")]
        [SerializeField] private string winMessage = "KAZANDINIZ!";
        [SerializeField] private string loseMessage = "KAYBETTİNİZ!";
        [SerializeField] private string drawMessage = "BERABERE!";

        [Header("Hint")]
        [SerializeField] private TextMeshProUGUI hintText;

        private void Start()
        {
            // Paneli gizle
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }

            // Event'e subscribe ol
            GameSessionManager.OnGameEnded += OnGameEnded;
        }

        private void OnDestroy()
        {
            GameSessionManager.OnGameEnded -= OnGameEnded;
        }

        /// <summary>
        /// Oyun bittiğinde çağrılır
        /// </summary>
        private void OnGameEnded(int winnerTeamId, int teamAScore, int teamBScore)
        {
            Debug.Log($"GameEndUI: Oyun bitti! Kazanan: {winnerTeamId}, A: {teamAScore}, B: {teamBScore}");

            // Paneli göster
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(true);
            }

            // Oyuncunun takımını al
            int myTeamId = PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer);

            // Sonuç mesajını ayarla
            SetResultDisplay(winnerTeamId, myTeamId, teamAScore, teamBScore);

            // Hint göster
            if (hintText != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    hintText.text = "Yeni oyun için F5 tuşuna basın";
                }
                else
                {
                    hintText.text = "Oda sahibi yeni oyun başlatabilir";
                }
            }
        }

        /// <summary>
        /// Sonuç gösterimini ayarlar
        /// </summary>
        private void SetResultDisplay(int winnerTeamId, int myTeamId, int teamAScore, int teamBScore)
        {
            // Takım isimlerini al
            string teamAName = PlayerInfo.GetCustomTeamName(PlayerInfo.TEAM_A);
            string teamBName = PlayerInfo.GetCustomTeamName(PlayerInfo.TEAM_B);

            if (resultText != null)
            {
                // Beraberlik kontrolü
                if (winnerTeamId == -1 || teamAScore == teamBScore)
                {
                    resultText.text = drawMessage;
                    resultText.color = drawColor;
                }
                // Kazandık mı kaybettik mi?
                else if (winnerTeamId == myTeamId)
                {
                    resultText.text = winMessage;
                    resultText.color = winColor;
                }
                else
                {
                    resultText.text = loseMessage;
                    resultText.color = loseColor;
                }
            }

            // Kazanan takım ismini göster
            if (winnerTeamText != null)
            {
                if (winnerTeamId == -1 || teamAScore == teamBScore)
                {
                    winnerTeamText.text = "";
                }
                else
                {
                    string winnerName = winnerTeamId == 0 ? teamAName : teamBName;
                    winnerTeamText.text = $"Kazanan: {winnerName}";
                    winnerTeamText.color = winColor;
                }
            }

            // Skorları takım isimleriyle göster
            if (teamAScoreText != null)
            {
                teamAScoreText.text = $"{teamAName}: {teamAScore}";
            }

            if (teamBScoreText != null)
            {
                teamBScoreText.text = $"{teamBName}: {teamBScore}";
            }
        }
    }
}

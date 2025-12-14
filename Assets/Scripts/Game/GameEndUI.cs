using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace TankGame.Game
{
    /// <summary>
    /// Oyun sonu ekranı
    /// Kazandınız/Kaybettiniz gösterimi ve skor tablosu
    /// </summary>
    public class GameEndUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject endGamePanel;

        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI resultText;
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

        [Header("Buttons")]
        [SerializeField] private Button returnToLobbyButton;

        private void Start()
        {
            // Paneli gizle
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }

            // Event'e subscribe ol
            GameSessionManager.OnGameEnded += OnGameEnded;

            // Buton ayarla
            if (returnToLobbyButton != null)
            {
                returnToLobbyButton.onClick.AddListener(OnReturnToLobbyClicked);
            }
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
        }

        /// <summary>
        /// Sonuç gösterimini ayarlar
        /// </summary>
        private void SetResultDisplay(int winnerTeamId, int myTeamId, int teamAScore, int teamBScore)
        {
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

            // Skorları göster
            if (teamAScoreText != null)
            {
                teamAScoreText.text = $"Takım A: {teamAScore}";
            }

            if (teamBScoreText != null)
            {
                teamBScoreText.text = $"Takım B: {teamBScore}";
            }
        }

        /// <summary>
        /// Lobiye dön butonuna tıklandığında
        /// </summary>
        private void OnReturnToLobbyClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// Lobiye dönüş sonrası MenuScene'e geç
        /// </summary>
        public void OnLeftRoom()
        {
            PhotonNetwork.LoadLevel("MenuScene");
        }
    }
}

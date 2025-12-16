using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using TankGame.Audio;

namespace TankGame.Game
{
    /// <summary>
    /// Oyun sonu ekranı
    /// Takım ismiyle kazanan gösterimi ve skor tablosu
    /// F5 ile sahne yenileme ipucu gösterir
    /// 5 saniye sonra leaderboard gösterilir
    /// Master Client leaderboard verisini tüm clientlara RPC ile gönderir
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class GameEndUI : MonoBehaviourPun
    {
        [Header("Panels")]
        [SerializeField] private GameObject endGamePanel;
        [SerializeField] private GameObject leaderboardPanel;

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

        [Header("Leaderboard Settings")]
        [Tooltip("End game paneli kapanıp leaderboard açılana kadar geçen süre")]
        [SerializeField] private float leaderboardDelay = 5f;

        [Header("Leaderboard Texts (10 satır)")]
        [Tooltip("Takım ismi textleri (1-10 sıralama)")]
        [SerializeField] private List<TextMeshProUGUI> leaderboardTeamNames = new List<TextMeshProUGUI>();
        [Tooltip("Takım puan textleri (1-10 sıralama)")]
        [SerializeField] private List<TextMeshProUGUI> leaderboardTeamScores = new List<TextMeshProUGUI>();

        private void Start()
        {
            // Panelleri gizle
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(false);
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

            // Oyun bitiş sesini çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameEndSound();
            }

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

            // 5 saniye sonra leaderboard göster
            StartCoroutine(ShowLeaderboardAfterDelay());
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

        #region Leaderboard

        /// <summary>
        /// Belirli süre sonra leaderboard'u gösterir
        /// </summary>
        private IEnumerator ShowLeaderboardAfterDelay()
        {
            yield return new WaitForSeconds(leaderboardDelay);

            // End game panelini kapat
            if (endGamePanel != null)
            {
                endGamePanel.SetActive(false);
            }

            // Leaderboard panelini aç
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(true);
            }

            // Master Client leaderboard verisini yükleyip tüm clientlara gönderir
            if (PhotonNetwork.IsMasterClient)
            {
                SendLeaderboardToAll();
            }
            else
            {
                // Client bekliyor, boş leaderboard göster
                ClearLeaderboard();
            }
        }

        /// <summary>
        /// Master Client: Leaderboard verisini yükler ve tüm clientlara RPC ile gönderir
        /// </summary>
        private void SendLeaderboardToAll()
        {
            List<LeaderboardEntry> entries = LoadLeaderboardFromCSV();

            // Max 10 entry gönder
            int count = Mathf.Min(entries.Count, 10);

            // İsimleri ve puanları ayrı array'ler olarak hazırla
            string[] names = new string[count];
            int[] scores = new int[count];

            for (int i = 0; i < count; i++)
            {
                names[i] = entries[i].teamName;
                scores[i] = entries[i].score;
            }

            // Tüm clientlara gönder (kendimiz dahil)
            photonView.RPC(nameof(RPC_ReceiveLeaderboard), RpcTarget.All, names, scores);
        }

        /// <summary>
        /// Tüm clientlarda çağrılır - leaderboard verisini gösterir
        /// </summary>
        [PunRPC]
        private void RPC_ReceiveLeaderboard(string[] names, int[] scores)
        {
            // Önce temizle
            ClearLeaderboard();

            // Verileri göster
            int count = Mathf.Min(names.Length, 10);
            for (int i = 0; i < count; i++)
            {
                if (i < leaderboardTeamNames.Count && leaderboardTeamNames[i] != null)
                {
                    leaderboardTeamNames[i].text = $"{i + 1}. {names[i]}";
                }
                if (i < leaderboardTeamScores.Count && leaderboardTeamScores[i] != null)
                {
                    leaderboardTeamScores[i].text = scores[i].ToString();
                }
            }

            Debug.Log($"Leaderboard alındı: {count} takım");
        }

        /// <summary>
        /// Leaderboard'u temizler (boş satırlar gösterir)
        /// </summary>
        private void ClearLeaderboard()
        {
            for (int i = 0; i < leaderboardTeamNames.Count; i++)
            {
                if (leaderboardTeamNames[i] != null)
                {
                    leaderboardTeamNames[i].text = $"{i + 1}. ...";
                }
            }
            for (int i = 0; i < leaderboardTeamScores.Count; i++)
            {
                if (leaderboardTeamScores[i] != null)
                {
                    leaderboardTeamScores[i].text = "";
                }
            }
        }

        /// <summary>
        /// CSV dosyasından leaderboard verilerini yükler
        /// En yüksek puanlı takımları döndürür (aynı takım birden fazla kez varsa en yüksek puanı alır)
        /// </summary>
        private List<LeaderboardEntry> LoadLeaderboardFromCSV()
        {
            Dictionary<string, int> teamBestScores = new Dictionary<string, int>();

            try
            {
                string recordsFolder = Path.Combine(Application.dataPath, "..", "kayitlar");
                string csvFilePath = Path.Combine(recordsFolder, "oyun_sonuclari.csv");

                if (!File.Exists(csvFilePath))
                {
                    Debug.Log("Leaderboard: CSV dosyası bulunamadı.");
                    return new List<LeaderboardEntry>();
                }

                string[] lines = File.ReadAllLines(csvFilePath, System.Text.Encoding.UTF8);

                // İlk satır başlık, atla
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(';');
                    if (parts.Length < 6) continue;

                    // Format: Tarih;Saat;Takim A;Takim A Puan;Takim B;Takim B Puan;Kazanan;Oyun Suresi
                    string teamAName = parts[2];
                    int teamAScore = 0;
                    int.TryParse(parts[3], out teamAScore);

                    string teamBName = parts[4];
                    int teamBScore = 0;
                    int.TryParse(parts[5], out teamBScore);

                    // Takım A için en yüksek puanı kaydet
                    if (!string.IsNullOrEmpty(teamAName))
                    {
                        if (!teamBestScores.ContainsKey(teamAName) || teamBestScores[teamAName] < teamAScore)
                        {
                            teamBestScores[teamAName] = teamAScore;
                        }
                    }

                    // Takım B için en yüksek puanı kaydet
                    if (!string.IsNullOrEmpty(teamBName))
                    {
                        if (!teamBestScores.ContainsKey(teamBName) || teamBestScores[teamBName] < teamBScore)
                        {
                            teamBestScores[teamBName] = teamBScore;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Leaderboard yüklenirken hata: {ex.Message}");
            }

            // Dictionary'yi listeye çevir ve puana göre sırala (yüksekten düşüğe)
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
            foreach (var kvp in teamBestScores)
            {
                entries.Add(new LeaderboardEntry { teamName = kvp.Key, score = kvp.Value });
            }
            entries.Sort((a, b) => b.score.CompareTo(a.score));

            return entries;
        }

        /// <summary>
        /// Leaderboard girdisi için yardımcı struct
        /// </summary>
        private struct LeaderboardEntry
        {
            public string teamName;
            public int score;
        }

        #endregion
    }
}

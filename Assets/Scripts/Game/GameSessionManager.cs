using UnityEngine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TankGame.Block;
using TankGame.Score;

namespace TankGame.Game
{
    /// <summary>
    /// Oyun durumunu yönetir
    /// FinishLine eventlerini dinler, kazananı belirler
    /// </summary>
    public class GameSessionManager : MonoBehaviourPunCallbacks
    {
        public static GameSessionManager Instance { get; private set; }

        // Room property keys
        private const string GAME_STATE = "GameState";
        private const string WINNER_TEAM = "WinnerTeam";
        private const string TEAM_A_FINISHED = "TeamAFinished";
        private const string TEAM_B_FINISHED = "TeamBFinished";

        // Oyun durumları
        public enum GameState
        {
            Playing,        // Oyun devam ediyor
            WaitingFinish,  // Bir takım bitti, diğerini bekliyoruz
            Ended           // Oyun bitti
        }

        [Header("Settings")]
        [Tooltip("İkinci takımı beklemek için maksimum süre (saniye)")]
        [SerializeField] private float waitForSecondTeamDuration = 10f;

        // Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<int, int, int> OnGameEnded; // (winnerTeamId, teamAScore, teamBScore)

        // Durum
        private GameState currentState = GameState.Playing;
        private float waitTimer = 0f;
        private bool isWaiting = false;
        private HashSet<int> finishedTeams = new HashSet<int>();

        private void Awake()
        {
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
            // FinishLine eventine subscribe ol
            FinishLine.OnTeamFinished += OnTeamFinished;

            // Oyun durumunu başlat
            if (PhotonNetwork.IsMasterClient)
            {
                SetGameState(GameState.Playing);

                // Finish durumlarını sıfırla
                Hashtable props = new Hashtable
                {
                    { TEAM_A_FINISHED, false },
                    { TEAM_B_FINISHED, false },
                    { WINNER_TEAM, -1 }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }

        private void OnDestroy()
        {
            FinishLine.OnTeamFinished -= OnTeamFinished;
        }

        private void Update()
        {
            // İkinci takımı bekliyorsak timer'ı güncelle
            if (isWaiting && PhotonNetwork.IsMasterClient)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    // Süre doldu, oyunu bitir
                    EndGame();
                }
            }
        }

        /// <summary>
        /// Bir takım finish line'ı geçtiğinde çağrılır
        /// </summary>
        private void OnTeamFinished(int teamId)
        {
            Debug.Log($"GameSessionManager: Takım {teamId} finish oldu!");

            // Bu takımı finish olarak işaretle
            finishedTeams.Add(teamId);

            // Room property'yi güncelle (Master Client)
            if (PhotonNetwork.IsMasterClient)
            {
                string key = teamId == 0 ? TEAM_A_FINISHED : TEAM_B_FINISHED;
                Hashtable props = new Hashtable { { key, true } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);

                // Durum kontrolü
                CheckGameEnd();
            }
        }

        /// <summary>
        /// Oyun bitiş durumunu kontrol eder
        /// </summary>
        private void CheckGameEnd()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            int finishedCount = finishedTeams.Count;

            if (finishedCount == 1 && currentState == GameState.Playing)
            {
                // İlk takım bitti, diğerini bekle
                SetGameState(GameState.WaitingFinish);
                isWaiting = true;
                waitTimer = waitForSecondTeamDuration;
                Debug.Log($"Bir takım finish oldu. Diğer takım için {waitForSecondTeamDuration} saniye bekleniyor...");
            }
            else if (finishedCount >= 2)
            {
                // Her iki takım da bitti
                EndGame();
            }
        }

        /// <summary>
        /// Oyunu bitirir ve kazananı belirler
        /// </summary>
        private void EndGame()
        {
            if (currentState == GameState.Ended) return;

            isWaiting = false;
            SetGameState(GameState.Ended);

            // Skorları al
            int teamAScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetTeamAScore() : 0;
            int teamBScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetTeamBScore() : 0;

            // Kazananı belirle
            int winnerTeam;
            if (teamAScore > teamBScore)
            {
                winnerTeam = 0; // Team A kazandı
            }
            else if (teamBScore > teamAScore)
            {
                winnerTeam = 1; // Team B kazandı
            }
            else
            {
                // Beraberlik - ilk finish olan kazanır
                // finishedTeams HashSet'inin ilk elemanı ilk finish olan
                winnerTeam = finishedTeams.Count > 0 ? GetFirstFinishedTeam() : -1;
            }

            // Room property'ye kaydet
            Hashtable props = new Hashtable { { WINNER_TEAM, winnerTeam } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            Debug.Log($"OYUN BİTTİ! Kazanan: Takım {winnerTeam} | Takım A: {teamAScore} - Takım B: {teamBScore}");
        }

        /// <summary>
        /// İlk finish olan takımı döndürür (beraberlik durumu için)
        /// </summary>
        private int GetFirstFinishedTeam()
        {
            // Room property'lerden kontrol et
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TEAM_A_FINISHED, out object teamAFinished))
            {
                if ((bool)teamAFinished) return 0;
            }
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TEAM_B_FINISHED, out object teamBFinished))
            {
                if ((bool)teamBFinished) return 1;
            }
            return -1;
        }

        /// <summary>
        /// Oyun durumunu değiştirir
        /// </summary>
        private void SetGameState(GameState state)
        {
            currentState = state;

            Hashtable props = new Hashtable { { GAME_STATE, (int)state } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        /// <summary>
        /// Mevcut oyun durumunu döndürür
        /// </summary>
        public GameState GetCurrentState() => currentState;

        /// <summary>
        /// Kazanan takımı döndürür (-1 = henüz belirlenmedi)
        /// </summary>
        public int GetWinnerTeam()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(WINNER_TEAM, out object winner))
            {
                return (int)winner;
            }
            return -1;
        }

        #region Photon Callbacks

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            // Oyun durumu değişti
            if (propertiesThatChanged.ContainsKey(GAME_STATE))
            {
                currentState = (GameState)(int)propertiesThatChanged[GAME_STATE];
                OnGameStateChanged?.Invoke(currentState);
                Debug.Log($"Oyun durumu değişti: {currentState}");
            }

            // Kazanan belirlendi
            if (propertiesThatChanged.ContainsKey(WINNER_TEAM))
            {
                int winner = (int)propertiesThatChanged[WINNER_TEAM];
                if (winner >= 0)
                {
                    int teamAScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetTeamAScore() : 0;
                    int teamBScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetTeamBScore() : 0;
                    OnGameEnded?.Invoke(winner, teamAScore, teamBScore);
                }
            }

            // Finish durumlarını senkronize et
            if (propertiesThatChanged.ContainsKey(TEAM_A_FINISHED))
            {
                if ((bool)propertiesThatChanged[TEAM_A_FINISHED])
                {
                    finishedTeams.Add(0);
                }
            }
            if (propertiesThatChanged.ContainsKey(TEAM_B_FINISHED))
            {
                if ((bool)propertiesThatChanged[TEAM_B_FINISHED])
                {
                    finishedTeams.Add(1);
                }
            }
        }

        #endregion
    }
}

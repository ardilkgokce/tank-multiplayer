using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Lobby ekranını yöneten sınıf.
/// 4 butonlu basit takım seçimi - herkes seçince Master Client GameScene'e yönlendirir.
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private Button teamAPlayerButton;
    [SerializeField] private Button teamBPlayerButton;
    [SerializeField] private Button teamASpectatorButton;
    [SerializeField] private Button teamBSpectatorButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Player Count UI")]
    [SerializeField] private TMP_Text teamACountText;
    [SerializeField] private TMP_Text teamBCountText;
    [SerializeField] private TMP_Text teamASpectatorCountText;
    [SerializeField] private TMP_Text teamBSpectatorCountText;
    [SerializeField] private TMP_Text selectedTeamText;

    [Header("Settings")]
    [SerializeField] private int maxPlayersPerTeam = 5;
    [SerializeField] private int maxSpectatorsPerTeam = 1;

    // Local state
    private bool hasSelectedTeam = false;

    private void Start()
    {
        statusText.text = "Takımınızı seçin";

        // Button listeners
        teamAPlayerButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_A, PlayerInfo.ROLE_PLAYER));
        teamBPlayerButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_B, PlayerInfo.ROLE_PLAYER));
        teamASpectatorButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_A, PlayerInfo.ROLE_SPECTATOR));
        teamBSpectatorButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_B, PlayerInfo.ROLE_SPECTATOR));

        // Start game button (sadece Master Client için)
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
            startGameButton.gameObject.SetActive(false);
        }

        // Selected team text
        if (selectedTeamText != null)
        {
            selectedTeamText.text = "";
        }

        // Player listesini güncelle
        RefreshPlayerList();
        UpdateStartButton();
    }

    /// <summary>
    /// Takım seçimi yapar (GameScene'e gitmez, sadece kaydeder).
    /// </summary>
    private void SelectTeam(int teamID, string role)
    {
        // Zaten seçim yaptıysa değiştirmeye izin ver
        // Takım/rol dolu mu kontrol et
        if (role == PlayerInfo.ROLE_PLAYER)
        {
            int teamPlayerCount = PlayerInfo.GetTeamPlayerCount(teamID, PlayerInfo.ROLE_PLAYER);
            // Eğer zaten bu takımdaysak sayıdan düş
            if (hasSelectedTeam && PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer) == teamID
                && PlayerInfo.GetRole(PhotonNetwork.LocalPlayer) == PlayerInfo.ROLE_PLAYER)
            {
                // Aynı takım, sorun yok
            }
            else if (teamPlayerCount >= maxPlayersPerTeam)
            {
                statusText.text = $"{PlayerInfo.GetTeamName(teamID)} oyuncu kapasitesi dolu!";
                return;
            }
        }
        else if (role == PlayerInfo.ROLE_SPECTATOR)
        {
            int spectatorCount = PlayerInfo.GetTeamSpectatorCount(teamID);
            // Eğer zaten bu takımın izleyicisiysek sayıdan düş
            if (hasSelectedTeam && PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer) == teamID
                && PlayerInfo.GetRole(PhotonNetwork.LocalPlayer) == PlayerInfo.ROLE_SPECTATOR)
            {
                // Aynı takım, sorun yok
            }
            else if (spectatorCount >= maxSpectatorsPerTeam)
            {
                statusText.text = $"{PlayerInfo.GetTeamName(teamID)} izleyici kapasitesi dolu!";
                return;
            }
        }

        // Tank color index'i hesapla (oyuncu için)
        int tankColorIndex = -1;
        if (role == PlayerInfo.ROLE_PLAYER)
        {
            tankColorIndex = PlayerInfo.GetNextAvailableTankColorIndex(teamID);
            if (tankColorIndex == -1)
            {
                statusText.text = "Takım dolu!";
                return;
            }
        }

        // Custom properties'i ayarla
        PlayerInfo.SetPlayerProperties(
            PhotonNetwork.LocalPlayer,
            "", // İsim GameScene'de girilecek
            teamID,
            role,
            true, // Takım seçildi = hazır (lobby için)
            tankColorIndex
        );

        hasSelectedTeam = true;

        // UI güncelle
        string roleText = role == PlayerInfo.ROLE_PLAYER ? "Oyuncu" : "İzleyici";
        statusText.text = $"{PlayerInfo.GetTeamName(teamID)} - {roleText} seçildi. Diğer oyuncular bekleniyor...";

        if (selectedTeamText != null)
        {
            selectedTeamText.text = $"Seçiminiz: {PlayerInfo.GetTeamName(teamID)} - {roleText}";
        }

        // Butonları pasifleştir (seçim yapıldı)
        DisableTeamButtons();

        RefreshPlayerList();
        UpdateStartButton();
    }

    /// <summary>
    /// Takım butonlarını devre dışı bırakır.
    /// </summary>
    private void DisableTeamButtons()
    {
        if (teamAPlayerButton != null) teamAPlayerButton.interactable = false;
        if (teamBPlayerButton != null) teamBPlayerButton.interactable = false;
        if (teamASpectatorButton != null) teamASpectatorButton.interactable = false;
        if (teamBSpectatorButton != null) teamBSpectatorButton.interactable = false;
    }

    /// <summary>
    /// Takım butonlarını aktif eder.
    /// </summary>
    private void EnableTeamButtons()
    {
        if (teamAPlayerButton != null) teamAPlayerButton.interactable = true;
        if (teamBPlayerButton != null) teamBPlayerButton.interactable = true;
        if (teamASpectatorButton != null) teamASpectatorButton.interactable = true;
        if (teamBSpectatorButton != null) teamBSpectatorButton.interactable = true;
    }

    /// <summary>
    /// Start butonunu günceller.
    /// </summary>
    private void UpdateStartButton()
    {
        if (startGameButton == null) return;

        // Sadece Master Client görebilir
        if (!PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(false);
            return;
        }

        // Herkes takım seçti mi?
        bool allPlayersReady = AreAllPlayersSelectedTeam();
        startGameButton.gameObject.SetActive(true);
        startGameButton.interactable = allPlayersReady;

        if (allPlayersReady)
        {
            statusText.text = "Tüm oyuncular hazır! Oyunu başlatabilirsiniz.";
        }
    }

    /// <summary>
    /// Tüm oyuncular takım seçti mi kontrol eder.
    /// </summary>
    private bool AreAllPlayersSelectedTeam()
    {
        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            // TeamID -1 ise henüz seçim yapmamış
            if (PlayerInfo.GetTeamID(player) == -1)
            {
                return false;
            }
        }
        return PhotonNetwork.PlayerList.Length > 0;
    }

    /// <summary>
    /// Oyunu başlatır (Master Client).
    /// </summary>
    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!AreAllPlayersSelectedTeam()) return;

        statusText.text = "Oyun başlatılıyor...";
        PhotonNetwork.LoadLevel("GameScene");
    }

    /// <summary>
    /// Player sayaçlarını yeniler.
    /// </summary>
    private void RefreshPlayerList()
    {
        // Takım oyuncu sayılarını hesapla
        int teamAPlayerCount = PlayerInfo.GetTeamPlayerCount(PlayerInfo.TEAM_A, PlayerInfo.ROLE_PLAYER);
        int teamBPlayerCount = PlayerInfo.GetTeamPlayerCount(PlayerInfo.TEAM_B, PlayerInfo.ROLE_PLAYER);
        int teamASpectatorCount = PlayerInfo.GetTeamSpectatorCount(PlayerInfo.TEAM_A);
        int teamBSpectatorCount = PlayerInfo.GetTeamSpectatorCount(PlayerInfo.TEAM_B);

        // Sayaçları güncelle
        if (teamACountText != null)
        {
            teamACountText.text = $"{teamAPlayerCount}/{maxPlayersPerTeam}";
        }

        if (teamBCountText != null)
        {
            teamBCountText.text = $"{teamBPlayerCount}/{maxPlayersPerTeam}";
        }

        if (teamASpectatorCountText != null)
        {
            teamASpectatorCountText.text = $"{teamASpectatorCount}/{maxSpectatorsPerTeam}";
        }

        if (teamBSpectatorCountText != null)
        {
            teamBSpectatorCountText.text = $"{teamBSpectatorCount}/{maxSpectatorsPerTeam}";
        }
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        RefreshPlayerList();
        UpdateStartButton();
        if (!hasSelectedTeam)
        {
            statusText.text = $"{newPlayer.NickName} odaya katıldı.";
        }
    }

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
    {
        RefreshPlayerList();
        UpdateStartButton();
        if (!hasSelectedTeam)
        {
            statusText.text = $"{otherPlayer.NickName} odadan ayrıldı.";
        }
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
    {
        RefreshPlayerList();
        UpdateStartButton();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        RefreshPlayerList();
        UpdateStartButton();

        if (PhotonNetwork.IsMasterClient)
        {
            statusText.text = "Artık oda sahibisiniz!";
        }
    }

    #endregion
}

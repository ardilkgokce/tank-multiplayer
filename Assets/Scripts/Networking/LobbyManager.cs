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
/// Oyuncu listesi, takım seçimi, hazır durumu ve oyun başlatma işlemlerini kontrol eder.
/// </summary>
public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Toggle roleToggle; // True = Oyuncu, False = İzleyici
    [SerializeField] private TMP_Text roleToggleLabel;
    [SerializeField] private Button teamAButton;
    [SerializeField] private Button teamBButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text selectedTeamText;
    [SerializeField] private TMP_Text statusText;

    [Header("Player Count UI")]
    [SerializeField] private TMP_Text teamACountText;
    [SerializeField] private TMP_Text teamBCountText;
    [SerializeField] private TMP_Text spectatorCountText;

    [Header("Settings")]
    [SerializeField] private int maxPlayersPerTeam = 5;
    [SerializeField] private int maxSpectators = 2;

    // Local player data
    private string currentPlayerName = "";
    private int currentTeamID = -1;
    private string currentRole = PlayerInfo.ROLE_PLAYER;
    private bool isReady = false;

    private void Start()
    {
        // UI başlangıç durumu
        startGameButton.gameObject.SetActive(false);
        readyButton.interactable = false;
        selectedTeamText.text = "Takım seçilmedi";
        statusText.text = "Lobby'ye hoş geldiniz!";

        // Button listeners
        teamAButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_A));
        teamBButton.onClick.AddListener(() => SelectTeam(PlayerInfo.TEAM_B));
        readyButton.onClick.AddListener(ToggleReady);
        startGameButton.onClick.AddListener(StartGame);
        playerNameInput.onValueChanged.AddListener(OnNameChanged);
        roleToggle.onValueChanged.AddListener(OnRoleToggleChanged);

        // Default role
        roleToggle.isOn = true; // Oyuncu
        OnRoleToggleChanged(true);

        // Player listesini güncelle
        RefreshPlayerList();
    }

    private void OnNameChanged(string newName)
    {
        currentPlayerName = newName.Trim();
        UpdateReadyButtonState();
    }

    private void OnRoleToggleChanged(bool isPlayer)
    {
        currentRole = isPlayer ? PlayerInfo.ROLE_PLAYER : PlayerInfo.ROLE_SPECTATOR;
        roleToggleLabel.text = isPlayer ? "Oyuncu" : "İzleyici";

        // İzleyici seçiliyse takım seçimi gerekli
        UpdateReadyButtonState();
    }

    /// <summary>
    /// Takım seçimi yapar.
    /// </summary>
    private void SelectTeam(int teamID)
    {
        // Takım dolu mu kontrol et
        if (currentRole == PlayerInfo.ROLE_PLAYER)
        {
            int teamPlayerCount = PlayerInfo.GetTeamPlayerCount(teamID, PlayerInfo.ROLE_PLAYER);
            if (teamPlayerCount >= maxPlayersPerTeam)
            {
                statusText.text = $"{PlayerInfo.GetTeamName(teamID)} dolu! Lütfen diğer takımı seçin.";
                return;
            }
        }
        else if (currentRole == PlayerInfo.ROLE_SPECTATOR)
        {
            int spectatorCount = PlayerInfo.GetSpectatorCount();
            if (spectatorCount >= maxSpectators && currentTeamID != teamID)
            {
                statusText.text = "İzleyici sayısı dolu!";
                return;
            }
        }

        currentTeamID = teamID;
        selectedTeamText.text = $"Seçilen: {PlayerInfo.GetTeamName(teamID)}";
        statusText.text = $"{PlayerInfo.GetTeamName(teamID)} seçildi.";

        UpdateReadyButtonState();
    }

    /// <summary>
    /// Hazır butonunun aktiflik durumunu günceller.
    /// </summary>
    private void UpdateReadyButtonState()
    {
        bool canBeReady = !string.IsNullOrEmpty(currentPlayerName) && currentTeamID != -1;
        readyButton.interactable = canBeReady;
    }

    /// <summary>
    /// Hazır durumunu değiştirir.
    /// </summary>
    private void ToggleReady()
    {
        isReady = !isReady;

        // Tank color index'i hesapla
        int tankColorIndex = -1;
        if (currentRole == PlayerInfo.ROLE_PLAYER)
        {
            tankColorIndex = PlayerInfo.GetNextAvailableTankColorIndex(currentTeamID);
            if (tankColorIndex == -1)
            {
                statusText.text = "Takım dolu!";
                isReady = false;
                return;
            }
        }

        // Custom properties'i güncelle
        PlayerInfo.SetPlayerProperties(
            PhotonNetwork.LocalPlayer,
            currentPlayerName,
            currentTeamID,
            currentRole,
            isReady,
            tankColorIndex
        );

        // UI güncelle
        readyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Hazır!" : "Hazır Ol";
        readyButton.interactable = true;
        statusText.text = isReady ? "Hazırsınız!" : "Hazır değilsiniz.";

        // Ready olduktan sonra değişiklik yapılamaz
        if (isReady)
        {
            playerNameInput.interactable = false;
            roleToggle.interactable = false;
            teamAButton.interactable = false;
            teamBButton.interactable = false;
        }
        else
        {
            playerNameInput.interactable = true;
            roleToggle.interactable = true;
            teamAButton.interactable = true;
            teamBButton.interactable = true;
        }

        RefreshPlayerList();
    }

    /// <summary>
    /// Oyunu başlatır (sadece Master Client).
    /// </summary>
    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            statusText.text = "Sadece oda sahibi oyunu başlatabilir!";
            return;
        }

        // Minimum oyuncu kontrolü (opsiyonel)
        int totalPlayers = PlayerInfo.GetTeamPlayerCount(PlayerInfo.TEAM_A, PlayerInfo.ROLE_PLAYER) +
                          PlayerInfo.GetTeamPlayerCount(PlayerInfo.TEAM_B, PlayerInfo.ROLE_PLAYER);

        if (totalPlayers < 2) // En az 2 oyuncu (test için)
        {
            statusText.text = "Oyunu başlatmak için en az 2 oyuncu gerekli!";
            return;
        }

        // Tüm oyuncular hazır mı?
        if (!PlayerInfo.AreAllPlayersReady())
        {
            statusText.text = "Tüm oyuncular hazır değil!";
            return;
        }

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
        int spectatorCount = PlayerInfo.GetSpectatorCount();

        // Sayaçları güncelle (format: "max/current")
        if (teamACountText != null)
        {
            teamACountText.text = $"{maxPlayersPerTeam}/{teamAPlayerCount}";
        }

        if (teamBCountText != null)
        {
            teamBCountText.text = $"{maxPlayersPerTeam}/{teamBPlayerCount}";
        }

        if (spectatorCountText != null)
        {
            spectatorCountText.text = $"{maxSpectators}/{spectatorCount}";
        }

        // Master Client ise Start butonu göster
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
        }
    }

    #region Photon Callbacks

    public override void OnPlayerEnteredRoom(PhotonPlayer newPlayer)
    {
        RefreshPlayerList();
        statusText.text = $"{newPlayer.NickName} odaya katıldı.";
    }

    public override void OnPlayerLeftRoom(PhotonPlayer otherPlayer)
    {
        RefreshPlayerList();
        statusText.text = $"{otherPlayer.NickName} odadan ayrıldı.";
    }

    public override void OnPlayerPropertiesUpdate(PhotonPlayer targetPlayer, Hashtable changedProps)
    {
        RefreshPlayerList();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        RefreshPlayerList();

        if (PhotonNetwork.IsMasterClient)
        {
            statusText.text = "Artık oda sahibisiniz!";
        }
    }

    #endregion
}

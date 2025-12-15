using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PhotonPlayer = Photon.Realtime.Player;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Oyuncu bilgilerini tutan ve Photon Custom Properties ile senkronize eden sınıf.
/// Her oyuncu için isim, takım, rol, hazır durumu ve tank rengi bilgilerini saklar.
/// </summary>
public class PlayerInfo
{
    // Custom Property Key'leri
    public const string PLAYER_NAME = "PlayerName";
    public const string TEAM_ID = "TeamID";
    public const string ROLE = "Role";
    public const string IS_READY = "IsReady";
    public const string TANK_COLOR_INDEX = "TankColorIndex";

    // Role değerleri
    public const string ROLE_PLAYER = "Player";
    public const string ROLE_SPECTATOR = "Spectator";

    // Team ID değerleri
    public const int TEAM_A = 0;
    public const int TEAM_B = 1;

    // Room Property Key'leri (Takım isimleri ve hazırlık durumu)
    public const string TEAM_A_NAME = "TeamAName";
    public const string TEAM_B_NAME = "TeamBName";
    public const string TEAM_A_READY = "TeamAReady";
    public const string TEAM_B_READY = "TeamBReady";
    public const string GAME_STARTED = "GameStarted";

    // Tank renk isimleri (sırayla)
    public static readonly string[] TankColorNames = new string[]
    {
        "Tank_Green",   // 0
        "Tank_Grey",    // 1
        "Tank_Orange",  // 2
        "Tank_Purple",  // 3
        "Tank_Yellow"   // 4
    };

    /// <summary>
    /// Oyuncuya Custom Property'leri set eder.
    /// </summary>
    public static void SetPlayerProperties(PhotonPlayer player, string playerName, int teamID, string role, bool isReady, int tankColorIndex)
    {
        if (player == null) return;

        Hashtable properties = new Hashtable
        {
            { PLAYER_NAME, playerName },
            { TEAM_ID, teamID },
            { ROLE, role },
            { IS_READY, isReady },
            { TANK_COLOR_INDEX, tankColorIndex }
        };

        player.SetCustomProperties(properties);
    }

    /// <summary>
    /// Belirli bir property'yi günceller.
    /// </summary>
    public static void UpdatePlayerProperty(PhotonPlayer player, string key, object value)
    {
        if (player == null) return;

        Hashtable properties = new Hashtable { { key, value } };
        player.SetCustomProperties(properties);
    }

    /// <summary>
    /// Oyuncunun ismini döner.
    /// </summary>
    public static string GetPlayerName(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null) return "Unknown";
        return player.CustomProperties.ContainsKey(PLAYER_NAME) ?
            (string)player.CustomProperties[PLAYER_NAME] : "Unknown";
    }

    /// <summary>
    /// Oyuncunun takım ID'sini döner.
    /// </summary>
    public static int GetTeamID(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null) return -1;
        return player.CustomProperties.ContainsKey(TEAM_ID) ?
            (int)player.CustomProperties[TEAM_ID] : -1;
    }

    /// <summary>
    /// Oyuncunun rolünü döner (Player veya Spectator).
    /// </summary>
    public static string GetRole(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null) return ROLE_PLAYER;
        return player.CustomProperties.ContainsKey(ROLE) ?
            (string)player.CustomProperties[ROLE] : ROLE_PLAYER;
    }

    /// <summary>
    /// Oyuncunun hazır durumunu döner.
    /// </summary>
    public static bool GetIsReady(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null) return false;
        return player.CustomProperties.ContainsKey(IS_READY) ?
            (bool)player.CustomProperties[IS_READY] : false;
    }

    /// <summary>
    /// Oyuncunun tank renk index'ini döner.
    /// </summary>
    public static int GetTankColorIndex(PhotonPlayer player)
    {
        if (player == null || player.CustomProperties == null) return -1;
        return player.CustomProperties.ContainsKey(TANK_COLOR_INDEX) ?
            (int)player.CustomProperties[TANK_COLOR_INDEX] : -1;
    }

    /// <summary>
    /// Oyuncunun tank prefab ismini döner.
    /// </summary>
    public static string GetTankPrefabName(PhotonPlayer player)
    {
        int colorIndex = GetTankColorIndex(player);
        if (colorIndex >= 0 && colorIndex < TankColorNames.Length)
        {
            return TankColorNames[colorIndex];
        }
        return TankColorNames[0]; // Default: Green
    }

    /// <summary>
    /// Belirtilen takımda kaç oyuncu olduğunu sayar.
    /// </summary>
    public static int GetTeamPlayerCount(int teamID, string roleFilter = ROLE_PLAYER)
    {
        int count = 0;
        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            if (GetTeamID(player) == teamID && GetRole(player) == roleFilter)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Belirtilen takımda kaç izleyici olduğunu sayar.
    /// </summary>
    public static int GetSpectatorCount()
    {
        int count = 0;
        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            if (GetRole(player) == ROLE_SPECTATOR)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Belirtilen takımdaki izleyici sayısını döndürür.
    /// </summary>
    public static int GetTeamSpectatorCount(int teamID)
    {
        int count = 0;
        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            if (GetTeamID(player) == teamID && GetRole(player) == ROLE_SPECTATOR)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Takım ismini Room Property'den alır.
    /// </summary>
    public static string GetCustomTeamName(int teamID)
    {
        if (PhotonNetwork.CurrentRoom == null) return GetTeamName(teamID);

        string key = teamID == TEAM_A ? TEAM_A_NAME : TEAM_B_NAME;
        object teamName;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out teamName))
        {
            string name = (string)teamName;
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }
        return GetTeamName(teamID); // Default isim
    }

    /// <summary>
    /// Takım ismini Room Property'ye kaydeder.
    /// </summary>
    public static void SetCustomTeamName(int teamID, string teamName)
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        string key = teamID == TEAM_A ? TEAM_A_NAME : TEAM_B_NAME;
        Hashtable props = new Hashtable { { key, teamName } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    /// <summary>
    /// Oyunun başlayıp başlamadığını kontrol eder.
    /// </summary>
    public static bool IsGameStarted()
    {
        if (PhotonNetwork.CurrentRoom == null) return false;

        object started;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(GAME_STARTED, out started))
        {
            return (bool)started;
        }
        return false;
    }

    /// <summary>
    /// Oyun başlama durumunu ayarlar.
    /// </summary>
    public static void SetGameStarted(bool started)
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        Hashtable props = new Hashtable { { GAME_STARTED, started } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    /// <summary>
    /// Sadece oyuncuların (spectator değil) hazır olup olmadığını kontrol eder.
    /// </summary>
    public static bool AreAllPlayersReadyExcludingSpectators()
    {
        int playerCount = 0;
        int readyCount = 0;

        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            if (GetRole(player) == ROLE_PLAYER)
            {
                playerCount++;
                if (GetIsReady(player))
                {
                    readyCount++;
                }
            }
        }

        return playerCount > 0 && playerCount == readyCount;
    }

    /// <summary>
    /// Tüm oyuncular hazır mı kontrol eder.
    /// </summary>
    public static bool AreAllPlayersReady()
    {
        if (PhotonNetwork.PlayerList.Length == 0) return false;

        foreach (PhotonPlayer player in PhotonNetwork.PlayerList)
        {
            if (!GetIsReady(player))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Belirtilen takımda bir sonraki tank renk index'ini döner.
    /// </summary>
    public static int GetNextAvailableTankColorIndex(int teamID)
    {
        int teamPlayerCount = GetTeamPlayerCount(teamID, ROLE_PLAYER);

        // Her takımda max 5 oyuncu, index 0-4 arası
        if (teamPlayerCount >= 5)
        {
            return -1; // Takım dolu
        }

        return teamPlayerCount; // 0, 1, 2, 3, veya 4
    }

    /// <summary>
    /// Oyuncunun takım adını döner (display için).
    /// </summary>
    public static string GetTeamName(int teamID)
    {
        switch (teamID)
        {
            case TEAM_A: return "Takım A";
            case TEAM_B: return "Takım B";
            default: return "Yok";
        }
    }

    /// <summary>
    /// Takımın hazır olup olmadığını kontrol eder.
    /// </summary>
    public static bool IsTeamReady(int teamID)
    {
        if (PhotonNetwork.CurrentRoom == null) return false;

        string key = teamID == TEAM_A ? TEAM_A_READY : TEAM_B_READY;
        object ready;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out ready))
        {
            return (bool)ready;
        }
        return false;
    }

    /// <summary>
    /// Takımın hazır durumunu ayarlar.
    /// </summary>
    public static void SetTeamReady(int teamID, bool ready)
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        string key = teamID == TEAM_A ? TEAM_A_READY : TEAM_B_READY;
        Hashtable props = new Hashtable { { key, ready } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    /// <summary>
    /// Her iki takım da hazır mı kontrol eder.
    /// </summary>
    public static bool AreBothTeamsReady()
    {
        return IsTeamReady(TEAM_A) && IsTeamReady(TEAM_B);
    }

    /// <summary>
    /// Tüm takım hazırlık durumlarını sıfırlar (sahne yenilendiğinde çağrılır).
    /// </summary>
    public static void ResetTeamReadyStates()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        Hashtable props = new Hashtable
        {
            { TEAM_A_READY, false },
            { TEAM_B_READY, false },
            { TEAM_A_NAME, "" },
            { TEAM_B_NAME, "" }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
}

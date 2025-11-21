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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Takım sistemini yöneten sınıf.
/// Layer assignment, visibility kontrolü ve takım bazlı işlemleri yönetir.
/// GameScene'de kullanılır.
/// </summary>
public class TeamManager : MonoBehaviour
{
    // Layer isimleri (Unity'de manuel olarak oluşturulmalı)
    public const string LAYER_TEAM_A = "TeamA";
    public const string LAYER_TEAM_B = "TeamB";
    public const string LAYER_SPECTATOR = "Spectator";
    public const string LAYER_DEFAULT = "Default";

    // Layer ID'leri (cache için)
    private static int layerTeamA = -1;
    private static int layerTeamB = -1;
    private static int layerSpectator = -1;

    private void Awake()
    {
        // Layer ID'lerini cache'le
        CacheLayers();
    }

    /// <summary>
    /// Layer ID'lerini cache'ler.
    /// </summary>
    private void CacheLayers()
    {
        layerTeamA = LayerMask.NameToLayer(LAYER_TEAM_A);
        layerTeamB = LayerMask.NameToLayer(LAYER_TEAM_B);
        layerSpectator = LayerMask.NameToLayer(LAYER_SPECTATOR);

        if (layerTeamA == -1)
        {
            Debug.LogError($"Layer '{LAYER_TEAM_A}' bulunamadı! Unity Editor'de bu layer'ı oluşturun.");
        }
        if (layerTeamB == -1)
        {
            Debug.LogError($"Layer '{LAYER_TEAM_B}' bulunamadı! Unity Editor'de bu layer'ı oluşturun.");
        }
        if (layerSpectator == -1)
        {
            Debug.LogError($"Layer '{LAYER_SPECTATOR}' bulunamadı! Unity Editor'de bu layer'ı oluşturun.");
        }
    }

    /// <summary>
    /// Bir GameObject'e takım ID'sine göre layer atar.
    /// </summary>
    public static void AssignTeamLayer(GameObject obj, int teamID)
    {
        if (obj == null) return;

        int targetLayer = teamID == PlayerInfo.TEAM_A ? layerTeamA : layerTeamB;

        if (targetLayer == -1)
        {
            Debug.LogWarning($"Layer ID bulunamadı! TeamID: {teamID}");
            return;
        }

        // GameObject ve tüm child'larına layer ata
        SetLayerRecursively(obj, targetLayer);

        Debug.Log($"{obj.name} -> Layer: {LayerMask.LayerToName(targetLayer)}");
    }

    /// <summary>
    /// Spectator için layer atar.
    /// </summary>
    public static void AssignSpectatorLayer(GameObject obj)
    {
        if (obj == null) return;

        if (layerSpectator == -1)
        {
            Debug.LogWarning("Spectator layer bulunamadı!");
            return;
        }

        SetLayerRecursively(obj, layerSpectator);
        Debug.Log($"{obj.name} -> Layer: Spectator");
    }

    /// <summary>
    /// GameObject ve tüm child'larına layer atar.
    /// </summary>
    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Bir takımın tüm tanklarını gösterir/gizler (visibility kontrolü için).
    /// Not: Şu an layer-based collision yeterli olduğu için bu metod opsiyonel.
    /// </summary>
    public static void SetTeamVisibility(int teamID, bool visible)
    {
        // Tüm PhotonView'ları ara
        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();

        foreach (PhotonView pv in allPhotonViews)
        {
            Player owner = pv.Owner;
            if (owner != null && PlayerInfo.GetTeamID(owner) == teamID)
            {
                // SpriteRenderer'ı bul ve aktifliğini değiştir
                SpriteRenderer sr = pv.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = visible;
                }
            }
        }
    }

    /// <summary>
    /// Local player'ın kamerasını sadece kendi takımını görecek şekilde ayarlar.
    /// Kamera pozisyonunu da takıma göre ayarlar.
    /// </summary>
    public static void ConfigureCameraForTeam(Camera cam, int teamID)
    {
        if (cam == null) return;

        int teamLayer = teamID == PlayerInfo.TEAM_A ? layerTeamA : layerTeamB;

        if (teamLayer == -1)
        {
            Debug.LogWarning("Team layer bulunamadı!");
            return;
        }

        // Kamera sadece kendi takımını görsün
        // Culling mask: Default layer + kendi takım layer'ı
        int defaultLayer = LayerMask.NameToLayer(LAYER_DEFAULT);
        cam.cullingMask = (1 << defaultLayer) | (1 << teamLayer);

        // Kamera pozisyonunu takıma göre ayarla
        Vector3 cameraPosition = GetTeamCameraPosition(teamID);
        cam.transform.position = cameraPosition;

        Debug.Log($"Kamera culling mask ayarlandı: Sadece {LayerMask.LayerToName(teamLayer)} görünüyor. Pozisyon: {cameraPosition}");
    }

    /// <summary>
    /// Spectator kamerasını bir takımı görecek şekilde ayarlar.
    /// Kamera pozisyonunu da takıma göre ayarlar.
    /// </summary>
    public static void ConfigureSpectatorCamera(Camera cam, int teamID)
    {
        if (cam == null) return;

        int teamLayer = teamID == PlayerInfo.TEAM_A ? layerTeamA : layerTeamB;

        if (teamLayer == -1)
        {
            Debug.LogWarning("Team layer bulunamadı!");
            return;
        }

        // Spectator kamera sadece izlediği takımı görsün
        int defaultLayer = LayerMask.NameToLayer(LAYER_DEFAULT);
        cam.cullingMask = (1 << defaultLayer) | (1 << teamLayer);

        // Kamera pozisyonunu takıma göre ayarla
        Vector3 cameraPosition = GetTeamCameraPosition(teamID);
        cam.transform.position = cameraPosition;

        Debug.Log($"Spectator kamera ayarlandı: {LayerMask.LayerToName(teamLayer)} takımı görünüyor. Pozisyon: {cameraPosition}");
    }

    /// <summary>
    /// Bir oyuncunun hangi takımda olduğunu döner (local player için).
    /// </summary>
    public static int GetLocalPlayerTeam()
    {
        if (PhotonNetwork.LocalPlayer == null) return -1;
        return PlayerInfo.GetTeamID(PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Bir oyuncunun rolünü döner (local player için).
    /// </summary>
    public static string GetLocalPlayerRole()
    {
        if (PhotonNetwork.LocalPlayer == null) return PlayerInfo.ROLE_PLAYER;
        return PlayerInfo.GetRole(PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Local player bir izleyici mi?
    /// </summary>
    public static bool IsLocalPlayerSpectator()
    {
        return GetLocalPlayerRole() == PlayerInfo.ROLE_SPECTATOR;
    }

    /// <summary>
    /// Belirtilen team layer ID'sini döner.
    /// </summary>
    public static int GetTeamLayerID(int teamID)
    {
        return teamID == PlayerInfo.TEAM_A ? layerTeamA : layerTeamB;
    }

    /// <summary>
    /// Takıma göre kamera pozisyonunu döner.
    /// Team A: (0, 0, -10) - Merkez
    /// Team B: (0, -100, -10) - 100 birim aşağı
    /// </summary>
    public static Vector3 GetTeamCameraPosition(int teamID)
    {
        if (teamID == PlayerInfo.TEAM_A)
        {
            return new Vector3(0, 0, -10);
        }
        else // Team B
        {
            return new Vector3(0, -100, -10);
        }
    }
}

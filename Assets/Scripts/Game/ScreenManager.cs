using UnityEngine;

namespace TankGame.Game
{
    /// <summary>
    /// Ekranı sabit 1920x1080 çözünürlüğünde açar.
    /// Tam ekran modunda çalışır.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        [Header("Target Resolution")]
        [SerializeField] private int targetWidth = 1920;
        [SerializeField] private int targetHeight = 1080;
        [SerializeField] private bool fullScreen = true;

        private void Awake()
        {
            // Sabit çözünürlük ayarla
            Screen.SetResolution(targetWidth, targetHeight, fullScreen);
            Debug.Log($"Ekran çözünürlüğü ayarlandı: {targetWidth}x{targetHeight}, Fullscreen: {fullScreen}");
        }
    }
}

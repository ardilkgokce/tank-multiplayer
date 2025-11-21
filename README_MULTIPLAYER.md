# Tank Multiplayer - Photon PUN2 Altyapısı

Photon PUN2 kullanarak oluşturulmuş basit 2D multiplayer tank oyunu altyapısı.

## 🎮 Özellikler

- ✅ Photon PUN2 ile gerçek zamanlı multiplayer
- ✅ Otomatik oda oluşturma/katılma
- ✅ WASD veya Arrow keys ile tank kontrolü
- ✅ Smooth network senkronizasyonu
- ✅ Kamera takip sistemi
- ✅ Maksimum 4 oyuncu desteği
- ✅ Kolay test edilebilir (2 editor veya editor + build)

## 📁 Proje Yapısı

```
Assets/
├── Scripts/
│   ├── Networking/
│   │   ├── NetworkManager.cs      # Photon bağlantı ve oda yönetimi
│   │   └── GameManager.cs         # Oyuncu spawn ve oyun yönetimi
│   └── Player/
│       ├── TankController.cs      # Tank hareketi ve network sync
│       └── CameraFollow.cs        # Kamera takip sistemi
├── Prefabs/
│   └── Resources/
│       └── Tank.prefab            # Network tank prefabı
├── Scenes/
│   ├── MenuScene.unity            # Bağlantı menüsü
│   └── GameScene.unity            # Oyun sahnesi
└── Imports/
    └── Sprites/
        └── Tank/                  # Tank sprite'ları (hazır)
```

## 🚀 Hızlı Başlangıç

### 1. Kurulum
Detaylı kurulum için `SETUP_GUIDE.md` dosyasına bakın.

**Özet:**
1. Tank.prefab oluştur (`Assets/Prefabs/Resources/Tank.prefab`)
2. MenuScene.unity kur (NetworkManager + UI)
3. GameScene.unity kur (GameManager + SpawnPoints)
4. Build Settings'e sahneleri ekle

### 2. Test
```
Yöntem 1: İki Unity Editor
- Her iki editor'de de Play → BAĞLAN

Yöntem 2: Editor + Build
- Build oluştur (File → Build Settings → Build)
- Build'i çalıştır + Editor'de Play
```

## 📝 Script Özellikleri

### NetworkManager.cs
- Photon sunucusuna otomatik bağlanma
- Random oda bulma veya yeni oda oluşturma
- UI status güncellemeleri
- Oda dolduğunda GameScene'e otomatik geçiş

**Kullanım:**
```csharp
// MenuScene'de NetworkManager GameObject'ine ekleyin
// UI referanslarını Inspector'dan ayarlayın
```

### GameManager.cs
- Oyuncu spawn yönetimi
- Spawn noktaları sistemi
- Oda eventlerini dinleme
- PhotonNetwork.Instantiate ile tank oluşturma

**Kullanım:**
```csharp
// GameScene'de GameManager GameObject'ine ekleyin
// Spawn point'leri Inspector'dan ayarlayın
```

### TankController.cs
- WASD/Arrow keys ile 2D hareket
- `photonView.IsMine` kontrolü (sadece kendi tankını kontrol et)
- Rigidbody2D ile smooth fizik
- OnPhotonSerializeView ile network senkronizasyonu
- Lag compensation

**Özellikler:**
- Move Speed: 5
- Rotation Speed: 200
- Otomatik renk farklılaştırma (kendi tankınız beyaz, diğerleri kırmızı)

**Alternatif Kontrol:**
```csharp
// WASD hareket + fare rotasyon için:
// GetInput() metodunda RotateTowardsMouse() çağırın
```

### CameraFollow.cs
- Tankı smooth şekilde takip eder
- Main Camera'ya eklenir
- TankController tarafından otomatik ayarlanır

## 🔧 Konfigürasyon

### Photon Settings
**Dosya:** `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`

- App ID: `fbfae497-fe9b-4b7b-8a0d-b1adefb00459` (ayarlı)
- Game Version: "1.0"
- Max Players: 4

### Tank Prefab Bileşenleri
1. **SpriteRenderer** - Tank görüntüsü
2. **Rigidbody2D** - Fizik (Gravity: 0, Freeze Rotation Z)
3. **BoxCollider2D** - Çarpışma
4. **PhotonView** - Network senkronizasyonu
5. **PhotonTransformView** - Pozisyon/rotasyon sync
6. **TankController** - Hareket ve kontrol

### Spawn Points
GameScene'de 4 spawn noktası:
- SpawnPoint_1: (-6, 4, 0)
- SpawnPoint_2: (6, 4, 0)
- SpawnPoint_3: (-6, -4, 0)
- SpawnPoint_4: (6, -4, 0)

## 🎯 Beklenen Davranış

### Bağlantı Akışı
1. MenuScene → BAĞLAN butonu
2. "Photon sunucusuna bağlanılıyor..."
3. "Sunucuya bağlandı. Oda aranıyor..."
4. "Odaya katıldınız! Oyuncu: 1/4"
5. GameScene'e otomatik geçiş (1 saniye sonra)

### Oyun İçi
- ✅ Kendi tankınız spawn olur (beyaz renk)
- ✅ WASD ile hareket edebilirsiniz
- ✅ Tank hareket yönüne döner
- ✅ Kamera tankınızı takip eder
- ✅ Diğer oyuncular açık kırmızı renkte görünür
- ✅ Diğer tankların hareketi smooth şekilde senkronize olur
- ✅ Sadece kendi tankınızı kontrol edebilirsiniz

## 🐛 Sorun Giderme

| Sorun | Çözüm |
|-------|-------|
| Tank prefab bulunamadı | `Assets/Prefabs/Resources/Tank.prefab` yolunu kontrol edin |
| PhotonView hatası | Tank prefabına PhotonView eklemeyi unutmayın |
| Kamera hareket etmiyor | Main Camera'ya CameraFollow script ekleyin |
| İkinci oyuncu odaya katılmıyor | PhotonServerSettings'de App ID kontrolü |
| Tanklar titriyor | Rigidbody2D → Interpolate: Interpolate |

## 📚 Sonraki Adımlar

Bu altyapı üzerine eklenebilecekler:

### 1. Silah Sistemi
- [ ] Bullet.prefab (Resources klasöründe)
- [ ] TankWeapon.cs (ateş etme, RPC)
- [ ] BulletPool.cs (performans için)

### 2. Can Sistemi
- [ ] TankHealth.cs
- [ ] HealthBar UI
- [ ] Ölüm/respawn sistemi
- [ ] RPC ile hasar verme

### 3. Oyun Mekanikleri
- [ ] Skor sistemi (kill/death)
- [ ] Scoreboard UI
- [ ] Oyun süresi/kill limiti
- [ ] Team deathmatch

### 4. Görsel İyileştirmeler
- [ ] Patlama efektleri
- [ ] Ses efektleri
- [ ] Particle sistemler
- [ ] Minimap
- [ ] Tank hasarı görselleri

### 5. UI/UX
- [ ] Oyuncu listesi
- [ ] Chat sistemi (PhotonChat kullanarak)
- [ ] Ayarlar menüsü
- [ ] Pause menüsü

## 📖 Dokümantasyon

- **SETUP_GUIDE.md** - Detaylı adım adım kurulum
- **CLAUDE.md** - Proje mimarisi ve geliştirme notları
- **README_MULTIPLAYER.md** - Bu dosya (genel bakış)

## 🔗 Kaynaklar

- [Photon PUN 2 Documentation](https://doc.photonengine.com/pun/current/getting-started/pun-intro)
- [Photon Dashboard](https://dashboard.photonengine.com/) - App ID yönetimi
- Unity Version: 2021.3.45f2
- Photon PUN 2: Free tier

## ⚡ Performans İpuçları

1. **Send Rate**: PhotonView → Send Rate = 20-30 (varsayılan: 20)
2. **Interpolation**: Rigidbody2D → Interpolate aktif
3. **Object Pooling**: Mermi sistemi için BulletPool kullanın
4. **LOD**: Uzak tanklar için sprite değiştirme
5. **Network Culling**: Görünmeyen objeler için Interest Management

## 📄 Lisans

Bu proje eğitim amaçlıdır. Photon PUN2 Free tier kullanmaktadır (CCU limit: 20).

---

**Hazırlayan:** Claude Code
**Tarih:** 2025-01-19
**Unity Version:** 2021.3.45f2
**Photon PUN:** 2.x

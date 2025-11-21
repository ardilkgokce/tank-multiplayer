# Multiplayer Tank Oyunu - Kurulum Kontrol Listesi ✅

Bu listeyi takip ederek kurulumun doğru yapıldığından emin olun.

## 📦 Ön Gereksinimler

- [ ] Unity 2021.3.45f2 yüklü
- [ ] Photon PUN2 asset import edilmiş
- [ ] PhotonServerSettings'de App ID ayarlanmış (`fbfae497-fe9b-4b7b-8a0d-b1adefb00459`)
- [ ] Tank sprite'ları mevcut (`Assets/Imports/Sprites/Tank/`)

---

## 🔧 Script Kurulumu

- [x] `Assets/Scripts/Networking/NetworkManager.cs` oluşturuldu
- [x] `Assets/Scripts/Networking/GameManager.cs` oluşturuldu
- [x] `Assets/Scripts/Player/TankController.cs` oluşturuldu
- [x] `Assets/Scripts/Player/CameraFollow.cs` oluşturuldu

---

## 🎮 Tank Prefab

### Tank GameObject
- [ ] Yeni GameObject oluşturuldu (isim: "Tank")
- [ ] Transform pozisyonu (0, 0, 0)

### Bileşenler
- [ ] **SpriteRenderer** eklendi
  - [ ] Sprite atandı (örn: `Tank/1.png`)
  - [ ] Order in Layer: 0

- [ ] **Rigidbody2D** eklendi
  - [ ] Body Type: Dynamic
  - [ ] Gravity Scale: 0
  - [ ] Linear Drag: 1
  - [ ] Angular Drag: 1
  - [ ] Constraints → Freeze Rotation: Z işaretli

- [ ] **BoxCollider2D** eklendi
  - [ ] Is Trigger: False

- [ ] **PhotonView** eklendi
  - [ ] Observe Options: Reliable Delta Compressed
  - [ ] Observed Components listesi hazır (sonra dolduracağız)

- [ ] **PhotonTransformView** eklendi
  - [ ] Synchronize Position: True
  - [ ] Synchronize Rotation: True
  - [ ] Synchronize Scale: False

- [ ] **TankController** (script) eklendi
  - [ ] Move Speed: 5
  - [ ] Rotation Speed: 200
  - [ ] Sprite Renderer referansı atandı

- [ ] **PhotonView → Observed Components** güncellenmiş
  - [ ] TankController eklendi
  - [ ] PhotonTransformView eklendi

### Prefab Kayıt
- [ ] Tank GameObject `Assets/Prefabs/Resources/` klasörüne sürüklendi
- [ ] Prefab tipi: Original Prefab
- [ ] Dosya yolu doğru: `Assets/Prefabs/Resources/Tank.prefab`
- [ ] Hierarchy'deki Tank objesi silindi

---

## 🎬 MenuScene Kurulumu

### Scene Oluşturma
- [ ] Yeni 2D (URP) scene oluşturuldu
- [ ] `Assets/Scenes/MenuScene.unity` olarak kaydedildi

### UI Elementleri
- [ ] **Canvas** oluşturuldu
  - [ ] Render Mode: Screen Space - Overlay
  - [ ] Canvas Scaler → UI Scale Mode: Scale With Screen Size
  - [ ] Reference Resolution: 1920 x 1080

- [ ] **StatusText** (TextMeshPro) oluşturuldu
  - [ ] Anchor: Top Center
  - [ ] Position Y: -100
  - [ ] Width: 800, Height: 100
  - [ ] Font Size: 36
  - [ ] Alignment: Center
  - [ ] Text: "Bağlantı için butona tıklayın"

- [ ] **ConnectButton** (Button - TextMeshPro) oluşturuldu
  - [ ] Anchor: Middle Center
  - [ ] Position: (0, 0, 0)
  - [ ] Width: 300, Height: 80
  - [ ] Button text: "BAĞLAN"
  - [ ] Font Size: 32

- [ ] **EventSystem** var (otomatik oluşturulmuş olmalı)

### NetworkManager GameObject
- [ ] Empty GameObject oluşturuldu (isim: "NetworkManager")
- [ ] **NetworkManager** (script) eklendi
  - [ ] Status Text referansı atandı
  - [ ] Connect Button referansı atandı
  - [ ] Max Players Per Room: 4

---

## 🏁 GameScene Kurulumu

### Scene Oluşturma
- [ ] Yeni 2D (URP) scene oluşturuldu
- [ ] `Assets/Scenes/GameScene.unity` olarak kaydedildi

### Main Camera
- [ ] **Main Camera** seçildi
- [ ] **CameraFollow** (script) eklendi
  - [ ] Smooth Speed: 5
  - [ ] Offset: (0, 0, -10)
- [ ] Camera → Background: Koyu renk
- [ ] Camera → Size: 8-10

### Spawn Points
- [ ] **SpawnPoints** (parent empty GameObject) oluşturuldu
- [ ] 4 adet child empty GameObject oluşturuldu:
  - [ ] **SpawnPoint_1**: Position (-6, 4, 0)
  - [ ] **SpawnPoint_2**: Position (6, 4, 0)
  - [ ] **SpawnPoint_3**: Position (-6, -4, 0)
  - [ ] **SpawnPoint_4**: Position (6, -4, 0)

### GameManager GameObject
- [ ] Empty GameObject oluşturuldu (isim: "GameManager")
- [ ] **GameManager** (script) eklendi
  - [ ] Spawn Points: 4 element
  - [ ] Her element'e spawn point referansı atandı
  - [ ] Spawn Delay: 0.5
  - [ ] Tank Prefab Name: "Tank"

### Oyun Alanı (Opsiyonel)
- [ ] 4 adet duvar (WallTop, WallBottom, WallLeft, WallRight)
- [ ] Her birinde BoxCollider2D var
- [ ] Zemin sprite'ı eklendi (opsiyonel)

---

## ⚙️ Build Settings

- [ ] **File → Build Settings** açıldı
- [ ] **Scenes in Build** listesine eklendi:
  - [ ] 0: MenuScene
  - [ ] 1: GameScene
- [ ] Platform: PC, Mac & Linux Standalone
- [ ] Target Platform ayarlandı

---

## 🧪 Test Hazırlığı

### Editor Test
- [ ] İki Unity Editor hazır VEYA
- [ ] Build oluşturuldu (File → Build Settings → Build)

### İlk Test
- [ ] MenuScene açık
- [ ] Play butonuna tıkla
- [ ] Console'da hata yok
- [ ] "BAĞLAN" butonu çalışıyor
- [ ] Status text değişiyor

### Network Test
- [ ] İki client (editor veya build) çalışıyor
- [ ] Her ikisi de "BAĞLAN"a tıkladı
- [ ] Aynı odaya katıldılar (Console log kontrolü)
- [ ] GameScene'e otomatik geçiş yaptı

### GameScene Test
- [ ] Tank spawn oldu
- [ ] WASD/Arrow keys ile hareket ediyor
- [ ] Kamera tankı takip ediyor
- [ ] İkinci client'ta tank görünüyor
- [ ] Renk farkı var (beyaz vs açık kırmızı)
- [ ] Diğer tankın hareketi senkronize

---

## 🐛 Sorun Giderme Kontrolleri

Eğer sorun varsa:

- [ ] Console'da hata mesajı var mı?
- [ ] Tank.prefab `Resources` klasöründe mi?
- [ ] Resources klasörü büyük harfle yazılmış mı? (önemli!)
- [ ] PhotonServerSettings'de App ID var mı?
- [ ] İnternet bağlantısı var mı?
- [ ] Build Settings'de her iki sahne de var mı?
- [ ] PhotonView bileşeni Tank prefabında mı?
- [ ] Observed Components listesi dolu mu?

---

## ✅ Tamamlandı!

Tüm kutular işaretlendiyse, multiplayer altyapınız hazır! 🎉

### Sonraki Adımlar:
1. Silah sistemi ekle
2. Can sistemi ekle
3. Skor sistemi ekle
4. Görsel efektler ekle

**Detaylı bilgi için:**
- `SETUP_GUIDE.md` - Adım adım kurulum
- `README_MULTIPLAYER.md` - Genel bakış ve özellikler

**İyi oyunlar!** 🎮

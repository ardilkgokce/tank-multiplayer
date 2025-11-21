# Tank Multiplayer - Kurulum Kılavuzu

Bu dosya, multiplayer tank oyunu altyapısını Unity'de nasıl kuracağınızı adım adım açıklar.

## 📋 İçindekiler
1. [Tank Prefab Oluşturma](#1-tank-prefab-oluşturma)
2. [MenuScene Kurulumu](#2-menuscene-kurulumu)
3. [GameScene Kurulumu](#3-gamescene-kurulumu)
4. [Build Settings](#4-build-settings)
5. [Test Etme](#5-test-etme)

---

## 1. Tank Prefab Oluşturma

### Adım 1.1: Yeni GameObject Oluştur
1. Hierarchy'de sağ tıklayın → **Create Empty**
2. İsmi **"Tank"** olarak değiştirin
3. Transform pozisyonunu **(0, 0, 0)** yapın

### Adım 1.2: Sprite Ekle
1. Tank GameObject'ini seçin
2. Inspector'da **Add Component** → **Sprite Renderer**
3. **Sprite** alanına:
   - `Assets/Imports/Sprites/Tank/` klasöründen bir tank sprite'ı sürükleyin (örn: `1.png`)
4. **Order in Layer** = `0` (varsayılan)

### Adım 1.3: Physics Bileşenleri Ekle
1. Tank GameObject'ini seçin
2. **Add Component** → **Rigidbody 2D**
   - **Body Type**: Dynamic
   - **Gravity Scale**: 0 (2D top-down için yerçekimi yok)
   - **Linear Drag**: 1 (durmak için hafif sürtünme)
   - **Angular Drag**: 1
   - **Constraints**: Freeze Rotation Z (dönerken fizik rotasyonu istemiyoruz)

3. **Add Component** → **Box Collider 2D**
   - Otomatik boyutlandırılacak
   - **Is Trigger**: False (çarpışmalar için)

### Adım 1.4: Photon Bileşenleri Ekle
1. Tank GameObject'ini seçin

2. **Add Component** → **Photon View**
   - **Observe Options**: Reliable Delta Compressed
   - **Observed Components**: Şimdilik boş bırakın (script'i ekledikten sonra ayarlayacağız)

3. **Add Component** → **Photon Transform View**
   - **Synchronize Position**: True
   - **Synchronize Rotation**: True
   - **Synchronize Scale**: False
   - Bu bileşen otomatik olarak PhotonView'ın Observed Components listesine eklenecek

### Adım 1.5: Tank Controller Script Ekle
1. Tank GameObject'ini seçin
2. **Add Component** → **Tank Controller** (yazdığımız script)
3. Inspector'da ayarlar:
   - **Move Speed**: 5
   - **Rotation Speed**: 200
   - **Sprite Renderer**: Tank'ın Sprite Renderer bileşenini sürükleyin

4. **PhotonView** bileşenine dönün:
   - **Observed Components** listesine **Tank Controller** ve **Photon Transform View** ekleyin

### Adım 1.6: Prefab Olarak Kaydet
1. Tank GameObject'ini Hierarchy'den `Assets/Prefabs/Resources/` klasörüne sürükleyin
2. **Original Prefab** seçeneğini seçin
3. Hierarchy'deki Tank objesini silebilirsiniz (artık prefab olarak var)

**✅ Tank Prefab Hazır!** Tank.prefab dosyası `Assets/Prefabs/Resources/` klasöründe olmalı.

---

## 2. MenuScene Kurulumu

### Adım 2.1: Yeni Scene Oluştur
1. **File** → **New Scene**
2. Template olarak **2D (URP)** seçin
3. **File** → **Save As** → `Assets/Scenes/MenuScene.unity`

### Adım 2.2: UI Canvas Oluştur
1. Hierarchy'de sağ tıklayın → **UI** → **Canvas**
2. Canvas Inspector'da:
   - **Render Mode**: Screen Space - Overlay
   - **Canvas Scaler** bileşeninde:
     - **UI Scale Mode**: Scale With Screen Size
     - **Reference Resolution**: 1920 x 1080

### Adım 2.3: Status Text Ekle
1. Canvas'ı sağ tıklayın → **UI** → **Text - TextMeshPro**
   - İsmi **"StatusText"** yapın
   - **Rect Transform**:
     - Anchor: Top Center
     - Pos Y: -100
     - Width: 800, Height: 100
   - **TextMeshPro** bileşeni:
     - Text: "Bağlantı için butona tıklayın"
     - Font Size: 36
     - Alignment: Center
     - Color: Beyaz

2. Eğer TMP importer çıkarsa, **Import TMP Essentials** butonuna tıklayın

### Adım 2.4: Connect Button Ekle
1. Canvas'ı sağ tıklayın → **UI** → **Button - TextMeshPro**
   - İsmi **"ConnectButton"** yapın
   - **Rect Transform**:
     - Anchor: Middle Center
     - Pos: (0, 0, 0)
     - Width: 300, Height: 80
   - Button içindeki **Text (TMP)** objesini seçin:
     - Text: "BAĞLAN"
     - Font Size: 32
     - Alignment: Center
     - Color: Beyaz

### Adım 2.5: NetworkManager Ekle
1. Hierarchy'de sağ tıklayın → **Create Empty**
2. İsmi **"NetworkManager"** yapın
3. **Add Component** → **Network Manager** (yazdığımız script)
4. Inspector'da:
   - **Status Text**: StatusText objesini sürükleyin
   - **Connect Button**: ConnectButton objesini sürükleyin
   - **Max Players Per Room**: 4

### Adım 2.6: Event System Kontrolü
- Hierarchy'de **EventSystem** objesi otomatik oluşturulmuş olmalı
- Yoksa: **GameObject** → **UI** → **Event System**

**✅ MenuScene Hazır!**

---

## 3. GameScene Kurulumu

### Adım 3.1: Yeni Scene Oluştur
1. **File** → **New Scene**
2. Template olarak **2D (URP)** seçin
3. **File** → **Save As** → `Assets/Scenes/GameScene.unity`

### Adım 3.2: Kamera Ayarları
1. **Main Camera** objesini seçin
2. **Add Component** → **Camera Follow** (yazdığımız script)
3. Inspector'da:
   - **Smooth Speed**: 5
   - **Offset**: (0, 0, -10)
4. Camera bileşeninde:
   - **Background**: Koyu gri veya siyah
   - **Size**: 8-10 (oyun alanına göre ayarlayın)

### Adım 3.3: Spawn Noktaları Oluştur
1. Hierarchy'de sağ tıklayın → **Create Empty**
   - İsmi **"SpawnPoints"** yapın (parent obje)

2. SpawnPoints'in altına 4 empty GameObject ekleyin:
   - Hierarchy'de SpawnPoints'e sağ tıklayın → **Create Empty** (4 kez)
   - İsimleri: **SpawnPoint_1**, **SpawnPoint_2**, **SpawnPoint_3**, **SpawnPoint_4**

3. Her spawn point'in pozisyonunu ayarlayın:
   - **SpawnPoint_1**: (-6, 4, 0)
   - **SpawnPoint_2**: (6, 4, 0)
   - **SpawnPoint_3**: (-6, -4, 0)
   - **SpawnPoint_4**: (6, -4, 0)

### Adım 3.4: GameManager Ekle
1. Hierarchy'de sağ tıklayın → **Create Empty**
2. İsmi **"GameManager"** yapın
3. **Add Component** → **Game Manager** (yazdığımız script)
4. Inspector'da:
   - **Spawn Points**: 4 olarak ayarlayın
   - Herbir elemente SpawnPoint_1, SpawnPoint_2, vb. sürükleyin
   - **Spawn Delay**: 0.5
   - **Tank Prefab Name**: "Tank" (Resources klasöründeki prefab ismi)

### Adım 3.5: Oyun Alanı Sınırları (Opsiyonel)
Tankların harita dışına çıkmaması için:

1. Hierarchy'de sağ tıklayın → **2D Object** → **Sprites** → **Square**
   - İsmi **"WallTop"** yapın
   - Scale: (20, 1, 1)
   - Position: (0, 10, 0)
   - **Add Component** → **Box Collider 2D**
   - Sprite Renderer'ı kapatabilirsiniz (görünmez duvar için)

2. Aynısını **WallBottom**, **WallLeft**, **WallRight** için tekrarlayın:
   - WallBottom: Position (0, -10, 0), Scale (20, 1, 1)
   - WallLeft: Position (-10, 0, 0), Scale (1, 20, 1)
   - WallRight: Position (10, 0, 0), Scale (1, 20, 1)

### Adım 3.6: Zemin/Arkaplan (Opsiyonel)
1. Hierarchy'de sağ tıklayın → **2D Object** → **Sprites** → **Square**
   - İsmi **"Ground"** yapın
   - Scale: (20, 20, 1)
   - Position: (0, 0, 1) (z=1 tanklardan arkada olsun)
   - Sprite Renderer'da Color: Koyu yeşil veya kahverengi
   - Order in Layer: -1

**✅ GameScene Hazır!**

---

## 4. Build Settings

### Adım 4.1: Sahneleri Build'e Ekle
1. **File** → **Build Settings**
2. **Scenes in Build** listesine sahneleri ekleyin:
   - **Add Open Scenes** butonuna tıklayın VEYA
   - Her iki sahneyi (MenuScene ve GameScene) sürükleyip bırakın

3. Sıralama:
   - **0: MenuScene** (ilk sahne)
   - **1: GameScene** (ikinci sahne)

### Adım 4.2: Platform Ayarları
1. Platform olarak **PC, Mac & Linux Standalone** seçili olmalı
2. **Target Platform**: Windows (veya kullandığınız OS)

**✅ Build Settings Hazır!**

---

## 5. Test Etme

### Yöntem 1: İki Unity Editor ile Test (Önerilen - Geliştirme İçin)

#### Editor 1:
1. Unity'de **MenuScene**'i aç
2. Play butonuna tıkla
3. "BAĞLAN" butonuna tıkla
4. Konsolu izle: "Odaya katıldınız!" mesajını görmeli

#### Editor 2 (Aynı anda):
1. Unity projesini **farklı bir Unity Editor instance**'ında aç:
   - Windows: Unity Hub'dan projeyi yeniden aç
   - Veya: Projeyi kopyala ve başka klasörde aç

2. Play butonuna tıkla
3. "BAĞLAN" butonuna tıkla
4. Aynı odaya katılmalı (toplam 2 oyuncu)

5. GameScene'de:
   - Her Editor kendi tankını kontrol edebilmeli (WASD)
   - Diğer Editor'daki tankın hareketini görebilmeli
   - Tanklar farklı renkte olmalı (kendi tankınız beyaz, diğerleri açık kırmızı)

### Yöntem 2: Editor + Build ile Test (Gerçek Senaryoya Yakın)

#### Build Oluştur:
1. **File** → **Build Settings**
2. **Build** butonuna tıkla
3. Klasör seçin ve **Select Folder**
4. Build tamamlanınca .exe dosyası oluşacak

#### Test:
1. **Build'i çalıştır** (.exe dosyasını aç)
   - BAĞLAN butonuna tıkla
   - GameScene'e geçmeli

2. **Unity Editor'de Play** butonuna tıkla
   - MenuScene'de BAĞLAN butonuna tıkla
   - Aynı odaya katılmalı

3. Test:
   - Build'deki tankı hareket ettir (WASD)
   - Editor'deki tankı hareket ettir (WASD)
   - Her iki taraftan da diğer tankın hareketini görebilmelisin

### Beklenen Davranışlar ✅

**Bağlantı:**
- "Photon sunucusuna bağlanılıyor..." mesajı
- "Sunucuya bağlandı. Oda aranıyor..." mesajı
- "Odaya katıldınız! Oyuncu: 1/4" mesajı
- GameScene'e otomatik geçiş

**GameScene'de:**
- Kendi tankınız beyaz renkte spawn olmalı
- WASD veya Arrow keys ile hareket edebilmelisiniz
- Tank hareket yönüne doğru dönmeli
- Kamera tankınızı takip etmeli

**Multiplayer:**
- İkinci oyuncu katıldığında konsola log düşmeli
- Diğer oyuncunun tankı açık kırmızı renkte görünmeli
- Diğer tankın hareketi smooth şekilde görünmeli (lag olsa bile)
- Her oyuncu sadece kendi tankını kontrol edebilmeli

### Sorun Giderme 🔧

**"Tank prefab bulunamadı" hatası:**
- Tank.prefab'ın `Assets/Prefabs/Resources/Tank.prefab` yolunda olduğundan emin olun
- Resources klasörü ismini büyük harfle yazın: **Resources** (küçük harf çalışmaz!)

**"PhotonView bulunamadı" hatası:**
- Tank prefab'ına PhotonView bileşeni ekleyin
- Observed Components listesinde TankController ve PhotonTransformView olmalı

**Kamera hareket etmiyor:**
- Main Camera'ya CameraFollow script'i eklenmiş mi?
- TankController'da Start() metodu çalışıyor mu? (Debug.Log ekleyin)

**İkinci oyuncu aynı odaya katılmıyor:**
- PhotonServerSettings'de App ID ayarlı mı? (Assets/Photon/PhotonUnityNetworking/Resources/)
- Her iki client da aynı Game Version kullanıyor mu? (NetworkManager'da "1.0")
- İnternet bağlantısı var mı?

**Tanklar hareket ederken titriyor:**
- Rigidbody2D → Interpolate: Interpolate yapın
- TankController'daki lerpSpeed değerini artırın (örn: 15)

---

## 📚 Sonraki Adımlar

Temel altyapı çalışıyor! Şimdi şunları ekleyebilirsiniz:

1. **Ateş Etme Sistemi**
   - Bullet prefab
   - TankWeapon.cs script
   - RPC ile mermi senkronizasyonu

2. **Can Sistemi**
   - TankHealth.cs
   - HealthBar UI
   - Ölüm ve respawn

3. **Skor Sistemi**
   - Kill/Death sayacı
   - Scoreboard UI

4. **Oyun Kuralları**
   - Time limit
   - Kill limit
   - Team deathmatch

5. **Görsel İyileştirmeler**
   - Patlama efektleri
   - Ses efektleri
   - Particle sistemler
   - Minimap

İyi oyunlar! 🎮

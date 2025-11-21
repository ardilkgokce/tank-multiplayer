# Network Sistemi Unity Editor Setup Guide

Bu guide, yeni network sistemini Unity Editor'de kurmanız için gereken tüm adımları içerir.

## ⚠️ ÖNEMLİ: Bölge Yerleşimi

**Harita düzeni:**
- **Team A Areası:** Merkez bölge (y=0 civarı) - Kamera: (0, 0, -10)
- **Team B Areası:** 100 birim aşağıda (y=-100 civarı) - Kamera: (0, -100, -10)

İki takım aynı haritada farklı bölgelerde oynar ve birbirlerini görmezler.

---

## 📋 İçindekiler

1. [Unity Layers Ekleme](#1-unity-layers-ekleme)
2. [Physics2D Collision Matrix Ayarlama](#2-physics2d-collision-matrix-ayarlama)
3. [MenuScene Lobby UI Oluşturma](#3-menuscene-lobby-ui-oluşturma)
4. [GameScene Spawn Points Oluşturma](#4-gamescene-spawn-points-oluşturma)
5. [Script Referanslarını Atama](#5-script-referanslarını-atama)
6. [Test ve Doğrulama](#6-test-ve-doğrulama)

---

## 1. Unity Layers Ekleme

### Adım 1.1: Layer Settings'i Açın
1. Unity Editor'de **Edit → Project Settings** menüsüne gidin
2. Sol panelden **Tags and Layers** seçin

### Adım 1.2: Layer'ları Ekleyin
Boş layer slotlarına şu layer'ları ekleyin:

- **Layer 8:** `TeamA`
- **Layer 9:** `TeamB`
- **Layer 10:** `Spectator`

> **Not:** Layer numaraları önemlidir. Eğer farklı layer numaraları kullanırsanız, TeamManager.cs'de gerekli güncellemeleri yapmalısınız.

---

## 2. Physics2D Collision Matrix Ayarlama

### Adım 2.1: Physics2D Settings'i Açın
1. **Edit → Project Settings** menüsüne gidin
2. Sol panelden **Physics 2D** seçin

### Adım 2.2: Layer Collision Matrix'i Yapılandırın
Sayfanın altındaki **Layer Collision Matrix** bölümünde:

1. **TeamA** satırını bulun
2. **TeamB** sütunundaki checkbox'ı **KALDIR** (unchecked)
   - Bu, TeamA ve TeamB'nin birbirine çarpmamasını sağlar

3. **Spectator** satırını bulun
4. **TeamA** ve **TeamB** sütunlarındaki checkbox'ları **KALDIR**
   - Bu, spectator'ların hiçbir şeye çarpmamasını sağlar

> **TeamManager.cs** bu ayarları kod ile de yapacak, ama manuel ayarlamak garantili çalışma sağlar.

---

## 3. MenuScene Lobby UI Oluşturma

### Adım 3.1: MenuScene'i Açın
1. **Assets/Scenes/MenuScene.unity** dosyasını açın
2. Hierarchy'de mevcut UI elementlerini görün

### Adım 3.2: Connect Panel'i Organize Edin
Mevcut UI elementleri bir panel altında toplayın:

1. Hierarchy'de **sağ tık → UI → Panel** oluşturun
2. İsmini **"Panel_Connect"** yapın
3. Mevcut **StatusText** ve **ConnectButton**'u bu panel altına sürükleyin

### Adım 3.3: Lobby Panel Oluşturun
1. Hierarchy'de **sağ tık → UI → Panel** oluşturun
2. İsmini **"Panel_Lobby"** yapın

### Adım 3.4: Lobby UI Elementlerini Ekleyin

#### 3.4.1: Oyuncu Bilgileri Bölümü
Panel_Lobby içinde:

**A. İsim Girişi:**
1. **sağ tık → UI → Input Field - TextMeshPro** oluşturun
2. İsim: **"InputField_PlayerName"**
3. Placeholder text: "İsim Soyisim"
4. Position: Top-left area

**B. Rol Toggle:**
1. **sağ tık → UI → Toggle** oluşturun
2. İsim: **"Toggle_Role"**
3. Label text: "Oyuncu" (ToggleController script ile değişecek)
4. Default: Checked (true)
5. Position: Below name input

**C. Seçilen Takım Göstergesi:**
1. **sağ tık → UI → Text - TextMeshPro** oluşturun
2. İsim: **"Text_SelectedTeam"**
3. Text: "Takım seçilmedi"
4. Position: Below role toggle

#### 3.4.2: Takım Seçim Butonları
**A. Takım A Butonu:**
1. **sağ tık → UI → Button - TextMeshPro** oluşturun
2. İsim: **"Button_TeamA"**
3. Text: "TAKIM A"
4. Color: Green tint
5. Position: Left side

**B. Takım B Butonu:**
1. **sağ tık → UI → Button - TextMeshPro** oluşturun
2. İsim: **"Button_TeamB"**
3. Text: "TAKIM B"
4. Color: Blue tint
5. Position: Right side

#### 3.4.3: Hazır ve Başlat Butonları
**A. Hazır Butonu:**
1. **sağ tık → UI → Button - TextMeshPro** oluşturun
2. İsim: **"Button_Ready"**
3. Text: "Hazır Ol"
4. Color: Yellow
5. Position: Center bottom

**B. Oyunu Başlat Butonu (Master Only):**
1. **sağ tık → UI → Button - TextMeshPro** oluşturun
2. İsim: **"Button_StartGame"**
3. Text: "OYUNU BAŞLAT"
4. Color: Red
5. Position: Center bottom (below ready button)

#### 3.4.4: Status Text (Lobby için)
1. **sağ tık → UI → Text - TextMeshPro** oluşturun
2. İsim: **"Text_LobbyStatus"**
3. Text: "Lobby'ye hoş geldiniz!"
4. Position: Top center

#### 3.4.5: Oyuncu Listeleri
**A. Takım A Listesi Panel:**
1. **sağ tık → UI → Scroll View** oluşturun
2. İsim: **"ScrollView_TeamA"**
3. Içindeki **Content** GameObject'ini bulun
4. Content için **Vertical Layout Group** component ekleyin
5. Position: Left side, middle

**B. Takım B Listesi Panel:**
1. **sağ tık → UI → Scroll View** oluşturun
2. İsim: **"ScrollView_TeamB"**
3. Içindeki **Content** GameObject'ini bulun
4. Content için **Vertical Layout Group** component ekleyin
5. Position: Right side, middle

**C. Spectator Listesi Panel:**
1. **sağ tık → UI → Scroll View** oluşturun
2. İsim: **"ScrollView_Spectators"**
3. Içindeki **Content** GameObject'ini bulun
4. Content için **Vertical Layout Group** component ekleyin
5. Position: Bottom center

### Adım 3.5: Player List Item Prefab Oluşturun

1. **sağ tık → UI → Panel** (Hierarchy'de geçici olarak)
2. İsim: **"PlayerListItem"**
3. İçinde 3 Text element oluşturun:
   - **"NameText"** - Oyuncu ismi (örn: "Ahmet Yılmaz")
   - **"ColorText"** - Tank rengi (örn: "Green")
   - **"ReadyText"** - Hazır durumu (örn: "✓" veya "✗")
4. Bu panel'i **Assets/Prefabs/** klasörüne sürükleyip prefab yapın
5. Hierarchy'deki geçici instance'ı silin

### Adım 3.6: NetworkManager ve LobbyManager Script Referansları

**NetworkManager GameObject:**
1. Hierarchy'de **NetworkManager** GameObject'ini seçin
2. Inspector'da **NetworkManager** component'ini bulun
3. Referansları atayın:
   - **Connect Panel:** Panel_Connect
   - **Lobby Panel:** Panel_Lobby
   - **Status Text:** Text_LobbyStatus (Panel_Connect içindeki)
   - **Connect Button:** ConnectButton (Panel_Connect içindeki)

**LobbyManager GameObject Oluşturun:**
1. Hierarchy'de **sağ tık → Create Empty**
2. İsim: **"LobbyManager"**
3. **Add Component → LobbyManager** script'ini ekleyin
4. Referansları atayın:
   - **Player Name Input:** InputField_PlayerName
   - **Role Toggle:** Toggle_Role
   - **Role Toggle Label:** Toggle_Role'un içindeki Label Text component
   - **Team A Button:** Button_TeamA
   - **Team B Button:** Button_TeamB
   - **Ready Button:** Button_Ready
   - **Start Game Button:** Button_StartGame
   - **Selected Team Text:** Text_SelectedTeam
   - **Status Text:** Text_LobbyStatus
   - **Team A Player List Container:** ScrollView_TeamA → Viewport → Content
   - **Team B Player List Container:** ScrollView_TeamB → Viewport → Content
   - **Spectator List Container:** ScrollView_Spectators → Viewport → Content
   - **Player List Item Prefab:** PlayerListItem prefab (Assets/Prefabs/)

---

## 4. GameScene Spawn Points Oluşturma

### Adım 4.1: GameScene'i Açın
1. **Assets/Scenes/GameScene.unity** dosyasını açın

### Adım 4.2: Eski Spawn Points'leri Silin veya Değiştirin
Mevcut **SpawnPoint_1, SpawnPoint_2, SpawnPoint_3, SpawnPoint_4** GameObject'lerini silebilirsiniz.

### Adım 4.3: Takım A Spawn Points (Sol Bölge)

5 adet Empty GameObject oluşturun:

1. **SpawnPoint_TeamA_1**
   - Position: `(-10, 6, 0)`

2. **SpawnPoint_TeamA_2**
   - Position: `(-10, 3, 0)`

3. **SpawnPoint_TeamA_3**
   - Position: `(-10, 0, 0)`

4. **SpawnPoint_TeamA_4**
   - Position: `(-10, -3, 0)`

5. **SpawnPoint_TeamA_5**
   - Position: `(-10, -6, 0)`

### Adım 4.4: Takım B Spawn Points (100 birim aşağıda)

5 adet Empty GameObject oluşturun:

1. **SpawnPoint_TeamB_1**
   - Position: `(10, -94, 0)`

2. **SpawnPoint_TeamB_2**
   - Position: `(10, -97, 0)`

3. **SpawnPoint_TeamB_3**
   - Position: `(10, -100, 0)`

4. **SpawnPoint_TeamB_4**
   - Position: `(10, -103, 0)`

5. **SpawnPoint_TeamB_5**
   - Position: `(10, -106, 0)`

> **İpucu:** Gizmos sayesinde spawn point'leri Scene view'da görebilirsiniz (yeşil = TeamA, mavi = TeamB).

---

## 5. Script Referanslarını Atama

### Adım 5.1: GameScene - TankGameManager

1. Hierarchy'de **GameManager** GameObject'ini seçin
2. Inspector'da **TankGameManager** component'ini bulun
3. Referansları atayın:
   - **Team A Spawn Points:** (5 element array)
     - SpawnPoint_TeamA_1
     - SpawnPoint_TeamA_2
     - SpawnPoint_TeamA_3
     - SpawnPoint_TeamA_4
     - SpawnPoint_TeamA_5
   - **Team B Spawn Points:** (5 element array)
     - SpawnPoint_TeamB_1
     - SpawnPoint_TeamB_2
     - SpawnPoint_TeamB_3
     - SpawnPoint_TeamB_4
     - SpawnPoint_TeamB_5
   - **Spawn Delay:** 0.5
   - **Spectator Camera Prefab:** (Boş bırakabilirsiniz veya bir prefab oluşturun - opsiyonel)

### Adım 5.2: GameScene - TeamManager Oluşturun

1. Hierarchy'de **sağ tık → Create Empty**
2. İsim: **"TeamManager"**
3. **Add Component → TeamManager** script'ini ekleyin
4. Bu script herhangi bir referans gerektirmez, Awake'de otomatik çalışır

### Adım 5.3: Spectator Camera Prefab (Opsiyonel)

Eğer spectator desteği istiyorsanız:

1. Hierarchy'de **Main Camera**'yı kopyalayın (Ctrl+D)
2. İsmini **"SpectatorCamera"** yapın
3. **Add Component → SpectatorController** script'ini ekleyin
4. Bu GameObject'i **Assets/Prefabs/Resources/** klasörüne prefab yapın
5. TankGameManager'a bu prefab'ı atayın

---

## 6. Test ve Doğrulama

### Adım 6.1: Sahneleri Build Settings'e Ekleyin

1. **File → Build Settings** açın
2. **Scenes in Build** listesine şunları ekleyin (sırayla):
   - MenuScene
   - GameScene
3. MenuScene index 0, GameScene index 1 olmalı

### Adım 6.2: İlk Test (Editor'de)

1. MenuScene'i açın ve Play'e basın
2. Kontrol edilecekler:
   - ✅ "Bağlan" butonu çalışıyor mu?
   - ✅ Photon'a bağlanıyor mu?
   - ✅ Lobby UI gösteriliyor mu?
   - ✅ İsim girişi yapılabiliyor mu?
   - ✅ Takım seçimi yapılabiliyor mu?
   - ✅ Hazır butonu çalışıyor mu?
   - ✅ Player listesi güncellendiğinde gösteriliyor mu?

### Adım 6.3: Multiplayer Test (Build + Editor)

1. **File → Build Settings → Build** ile bir standalone build oluşturun
2. Build'i çalıştırın
3. Unity Editor'de Play'e basın
4. Her iki instance'da:
   - Isim girin
   - Farklı takımlar seçin (biri A, biri B)
   - Hazır olun
5. Master client "Oyunu Başlat" butonuna bassın

**Beklenen Sonuçlar:**
- ✅ Her iki client da GameScene'e geçmeli
- ✅ Her oyuncu kendi takımının spawn point'inde spawn olmalı
- ✅ Her oyuncu sadece kendi takımını görmeli
- ✅ Farklı tank renkleri (Green, Grey, Orange, Purple, Yellow) atanmalı
- ✅ Takımlar birbirine çarpmıyor olmalı (aynı yerde olsalar bile)

### Adım 6.4: Spectator Test (Opsiyonel)

1. 3. bir instance (build veya ParrelSync clone) açın
2. Lobby'de:
   - Role toggle'ı "İzleyici" yapın
   - Bir takım seçin
   - Hazır olun
3. Oyun başladığında:
   - ✅ Spectator olarak spawn olmalı
   - ✅ Seçtiği takımı görmeli
   - ✅ Tab ile oyuncular arası geçiş yapabilmeli

---

## 🎯 Tamamlama Checklist

- [ ] Unity Layers eklendi (TeamA, TeamB, Spectator)
- [ ] Physics2D Collision Matrix ayarlandı
- [ ] MenuScene Lobby UI oluşturuldu
- [ ] PlayerListItem prefab oluşturuldu
- [ ] NetworkManager referansları atandı
- [ ] LobbyManager GameObject oluşturuldu ve referansları atandı
- [ ] GameScene spawn points oluşturuldu (Team A: 5, Team B: 5)
- [ ] TankGameManager referansları atandı
- [ ] TeamManager GameObject oluşturuldu
- [ ] Sahneler Build Settings'e eklendi
- [ ] Editor'de test yapıldı
- [ ] Multiplayer test yapıldı (build + editor)
- [ ] Spectator test yapıldı (opsiyonel)

---

## ⚠️ Olası Sorunlar ve Çözümleri

### Sorun 1: "Layer 'TeamA' bulunamadı" hatası
**Çözüm:** Unity Editor'de Edit → Project Settings → Tags and Layers'dan layer'ları ekleyin.

### Sorun 2: Takımlar birbirini görüyor
**Çözüm:** Camera culling mask'ı kontrol edin. TeamManager.ConfigureCameraForTeam() fonksiyonu çalışıyor mu?

### Sorun 3: Lobby UI gösterilmiyor
**Çözüm:** NetworkManager'da connectPanel ve lobbyPanel referanslarını kontrol edin.

### Sorun 4: "Tank spawn edilemedi" hatası
**Çözüm:** Tank prefablarının **Assets/Prefabs/Resources/** klasöründe olduğundan emin olun.

### Sorun 5: Player list boş
**Çözüm:** LobbyManager'da playerListItemPrefab referansını kontrol edin. PlayerListItem prefab'ında NameText, ColorText, ReadyText GameObject'leri var mı?

### Sorun 6: Spectator kamera çalışmıyor
**Çözüm:** SpectatorCamera prefab Resources klasöründe olmalı ve TankGameManager'a atanmalı.

---

## 📚 Ek Notlar

- **Prefab isimlendirme:** Kodda "Tank_Grey" kullanılıyor (projedeki isim böyle)
- **Takım dengeleme:** Sistem otomatik dengeleme yapmaz, oyuncular manuel seçer
- **İzleyici limiti:** Max 2 izleyici (LobbyManager'da değiştirilebilir)
- **Tank renk sırası:** Green → Grey → Orange → Purple → Yellow (takıma katılım sırasına göre)

---

## ✅ Setup Tamamlandı!

Network sistemi artık hazır! Oyun mekanikleri (savaş, skor vb.) için sonraki aşamaya geçebilirsiniz.

Başarılar! 🎮

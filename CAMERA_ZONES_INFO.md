# Kamera ve Bölge Bilgileri

## 🎮 Harita Yerleşimi

### Team A Areası (Merkez)
- **Konum:** Y = 0 civarı (merkez bölge)
- **Kamera Pozisyonu:** (0, 0, -10)
- **Spawn Points:**
  - SpawnPoint_TeamA_1: (-10, 6, 0)
  - SpawnPoint_TeamA_2: (-10, 3, 0)
  - SpawnPoint_TeamA_3: (-10, 0, 0)
  - SpawnPoint_TeamA_4: (-10, -3, 0)
  - SpawnPoint_TeamA_5: (-10, -6, 0)

### Team B Areası (100 birim aşağı)
- **Konum:** Y = -100 civarı
- **Kamera Pozisyonu:** (0, -100, -10)
- **Spawn Points:**
  - SpawnPoint_TeamB_1: (10, -94, 0)
  - SpawnPoint_TeamB_2: (10, -97, 0)
  - SpawnPoint_TeamB_3: (10, -100, 0)
  - SpawnPoint_TeamB_4: (10, -103, 0)
  - SpawnPoint_TeamB_5: (10, -106, 0)

---

## 📷 Kamera Sistemi

### Otomatik Kamera Pozisyonlama

**TeamManager.GetTeamCameraPosition(teamID)** metodu otomatik olarak kamera pozisyonunu ayarlar:
- Team A için: `new Vector3(0, 0, -10)`
- Team B için: `new Vector3(0, -100, -10)`

### Oyuncu Kamerası

Her oyuncu spawn olduğunda:
1. `TankGameManager.SetupCameraForTank()` çağrılır
2. `TeamManager.ConfigureCameraForTeam()` kamera pozisyonunu ve culling mask'ı ayarlar
3. `CameraFollow` script'i tank'ı takip eder
4. Kamera **sadece** kendi takımını gösterir (Layer-based culling)

### İzleyici Kamerası

İzleyiciler için:
1. `SpectatorController` spawn olur
2. `TeamManager.ConfigureSpectatorCamera()` kamera pozisyonunu ayarlar
3. İzleyici seçtiği takımın bölgesinde (Team A: y=0, Team B: y=-100) başlar
4. Tab tuşu ile oyuncular arası geçiş yapabilir
5. Space tuşu ile manuel/otomatik kamera modları arası geçiş yapabilir

---

## 🎯 Görünürlük Sistemi

### Layer-Based Visibility

**Team A Oyuncuları:**
- Layer: `TeamA` (Layer 8)
- Kamera culling mask: `Default | TeamA`
- **Görebildikleri:** Sadece Team A oyuncuları + harita objeleri (Default layer)
- **Göremedikleri:** Team B oyuncuları

**Team B Oyuncuları:**
- Layer: `TeamB` (Layer 9)
- Kamera culling mask: `Default | TeamB`
- **Görebildikleri:** Sadece Team B oyuncuları + harita objeleri
- **Göremedikleri:** Team A oyuncuları

**İzleyiciler:**
- Layer: `Spectator` (Layer 10)
- Kamera culling mask: `Default | (TeamA veya TeamB)` (seçilen takıma göre)
- **Görebildikleri:** Seçtikleri takımın oyuncuları + harita objeleri

---

## 🔧 Teknik Detaylar

### Kod Referansları

**TeamManager.cs:**
```csharp
public static Vector3 GetTeamCameraPosition(int teamID)
{
    if (teamID == PlayerInfo.TEAM_A)
        return new Vector3(0, 0, -10);      // Team A - Merkez
    else
        return new Vector3(0, -100, -10);   // Team B - 100 birim aşağı
}
```

**TankGameManager.cs:**
```csharp
// Default spawn pozisyonları
Team A: new Vector3(-10, 0, 0)
Team B: new Vector3(10, -100, 0)
```

### Collision Matrix

**Physics2D Ayarları:**
- TeamA ↔ TeamB: **Collision DISABLED** (birbirlerine çarpmaz)
- TeamA ↔ TeamA: **Collision ENABLED** (takım arkadaşlarına çarpabilir)
- TeamB ↔ TeamB: **Collision ENABLED** (takım arkadaşlarına çarpabilir)
- Spectator ↔ All: **Collision DISABLED** (hiçbir şeye çarpmaz)

---

## 📊 Harita Görselleştirmesi

```
Y Axis
  |
  |  +---------------------+
  |  |   Team A Areası     |  y = 0 civarı
  |  |  (Merkez Bölge)     |  Camera: (0, 0, -10)
  |  |  5 Spawn Point      |
  |  +---------------------+
  |
  |  ... 94 birim boşluk ...
  |
  |  +---------------------+
  |  |   Team B Areası     |  y = -100 civarı
  |  |  (Aşağı Bölge)      |  Camera: (0, -100, -10)
  |  |  5 Spawn Point      |
  |  +---------------------+
  |
  v
```

---

## ✅ Kontrol Listesi

Unity Editor'de setup yaparken:
- [ ] Team A spawn points doğru pozisyonda (y = 0 civarı)
- [ ] Team B spawn points doğru pozisyonda (y = -100 civarı)
- [ ] Main Camera başlangıç pozisyonu (0, 0, -10)
- [ ] TeamManager GameObject eklendi
- [ ] Layers (TeamA, TeamB, Spectator) oluşturuldu
- [ ] Physics2D collision matrix ayarlandı
- [ ] Test: Team A oyuncusu merkez bölgede spawn oluyor
- [ ] Test: Team B oyuncusu 100 birim aşağıda spawn oluyor
- [ ] Test: Her takım sadece kendi üyelerini görüyor
- [ ] Test: İzleyici seçtiği takımı görebiliyor

---

## 🐛 Olası Sorunlar ve Çözümler

### Sorun 1: Team B oyuncuları görünmüyor
**Çözüm:** Team B spawn points pozisyonlarını kontrol edin. Y değeri -100 civarında olmalı.

### Sorun 2: Kamera yanlış pozisyonda başlıyor
**Çözüm:** `TeamManager.ConfigureCameraForTeam()` veya `ConfigureSpectatorCamera()` çağrıldığından emin olun.

### Sorun 3: İki takım birbirini görüyor
**Çözüm:**
1. Layer'lar doğru atanmış mı kontrol edin
2. Camera culling mask doğru mu kontrol edin
3. TeamManager.ConfigureCameraForTeam() çağrılıyor mu kontrol edin

### Sorun 4: Tank yanlış bölgede spawn oluyor
**Çözüm:**
1. TankGameManager'da spawn points referansları kontrol edin
2. Team A ve Team B spawn point array'leri doğru atanmış mı kontrol edin

---

**Son Güncelleme:** Kamera ve bölge sistemi implementasyonu tamamlandı.

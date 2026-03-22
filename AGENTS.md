# OVO — AI Destekli Kombin & Sanal Deneme Uygulaması
## Cursor Proje Prompt'u

---

## 🎯 Proje Özeti

**OVO**, kullanıcıların kıyafetlerini dijital dolaba ekleyebildiği, yapay zeka destekli kombin önerileri aldığı ve kıyafetleri kendi fotoğrafları üzerinde sanal olarak deneyebildiği bir mobil uygulamadır.

**Tech Stack:**
- **Backend:** ABP.io (ASP.NET Core) — REST API servisleri + Admin MVC paneli
- **Mobile:** React Native (Expo önerilir)
- **AI/ML Pipeline:** Banana Pro (NSFW kontrol, arka plan kaldırma, etiketleme, studio foto, try-on render)
- **SMS:** Twilio veya Netgsm
- **Hava Durumu:** OpenWeatherMap API (GPS bazlı)
- **Görsel Arama:** Google Lens API veya benzeri (Benzerini Bul özelliği için)
- **Depolama:** Azure Blob Storage veya AWS S3 veya Minio (keni serverimizde olan)

---

## 🏗️ Proje Yapısı
 backend tarafı abp.io ile tam uyumlu bir yapıda olmalıdır.
```
ovo/
├── backend/                    # ABP.io solution
│   ├── src/
│   │   ├── Ovo.Domain/         # Domain entities, interfaces
│   │   ├── Ovo.Application/    # Application services, DTOs
│   │   ├── Ovo.EntityFramework/ # EF Core, migrations
│   │   ├── Ovo.HttpApi/        # Controllers, REST endpoints
│   │   ├── Ovo.HttpApi.Host/   # API Host (startup)
│   │   └── Ovo.Web/            # Admin MVC panel
│   └── test/
├── mobile/                     # React Native app
│   ├── src/
│   │   ├── screens/
│   │   ├── components/
│   │   ├── services/           # API calls
│   │   ├── store/              # Zustand/Redux state
│   │   ├── hooks/
│   │   └── utils/
│   └── assets/
└── docs/
```

---

## 📱 Mobil Uygulama — Detaylı Özellikler

### Kimlik Doğrulama
- Kayıt: e-posta + telefon + şifre (min 8 karakter)
- SMS OTP: 6 haneli, 120 sn süre, 3 deneme hakkı, 15 dk kilit
- Giriş: e-posta+şifre VEYA telefon+SMS OTP
- Şifre sıfırlama akışı

### Alt Bar — 4 Sekme
1. **Kombinlerim** (Elbise ikonu) — Üye zorunlu
2. **Topluluk** (2 kişi ikonu) — Kısmi (yalnızca feed görüntüleme)
3. **Try-On** (Büyük siyah daire, insan ikonu) — Üye zorunlu
4. **Dolabım** (Dolap ikonu) — Üye zorunlu

Header her sayfada sabit: Sol = "kombin" logo | Sağ = Kalp ikonu (wishlist) + Profil ikonu

---

### SEKME 1: Kombinlerim

**Bileşenler (yukarıdan aşağı):**

1. **Hava Durumu Çubuğu**
   - GPS'ten şehir + sıcaklık + kısa öneri
   - Her 3 saatte güncellenir, izin yoksa İstanbul varsayılan
   - Örnek: "☁️ 12°C İstanbul, yağmurlu — trenchkot ve bot önerdik"

2. **Kombin Kartı (Ana Alan)**
   - Fotoğraf yüklüyse: studio fotoğraf üzerinde kıyafetler giydirilmiş
   - Fotoğraf yoksa: 4 kıyafetin görseli yan yana (gömlek, pantolon, ayakkabı, aksesuar)
   - Swipe ile 10 farklı kombin (nokta indikatör)
   - Sadece ana kombin otomatik gösterilir, diğerleri "Oluştur" butonuyla

3. **Kıyafet Parçaları** (küçük kutucuklar)
   - Her parçanın küçük görseli (Gömlek, Chino, Loafer, Saat...)
   - Parçaya tıkla → seçilir (kalın çerçeve)
   - Swap ikonu → aynı kategorideki diğer kıyafetler

4. **Trend Renkler**
   - Tüm kullanıcı dolaplarındaki en popüler renkler
   - Yuvarlak daireler + isim + yüzde (Lacivert %34, Beyaz %28...)

5. **En Çok Tercih Edilen Kombinler**
   - Yatay kaydırılan kartlar
   - Tıkla → Try-On'a gider
   - İleride: ürün satıcısına yönlendirme alanı (reklam/affiliate)

---

### SEKME 2: Topluluk

1. **Filtreler:** Yeni | Popüler | Takip
2. **Oylama Kartları:** "Hangisini giysem?" — 2-3 seçenek
   - Kullanıcı adına tıkla → profil (login gerekli)
   - Seçeneğe tıkla → oy verilir
   - Kalp → wishlist
   - "Bul" → Benzerini Bul paneli
   - "Dene" → Try-On
3. **Benzerini Bul Paneli** (alt panel)
   - Görsel arama (Google Lens benzeri), TR bazlı satıcılar
   - Sonuç: Zara 899TL %94 benzer, Trendyol 459TL %87...
   - Tıkla → siteye git VEYA Try-On'da dene
4. **Oylama Oluştur**
   - Dolabındaki kombinlerden 2-3 seç → soru yaz → AI içerik kontrolü → paylaş
5. **Profil Sayfası**
   - Görünür kombinler, takip et/engelle, kalp/dene/bul aksiyonları
   - Engelleme = karşılıklı görünmezlik

---

### SEKME 3: Try-On (Uygulamanın Kalbi)

**Adım 1: Fotoğraf Yükle**
- En az 3, en fazla 15 fotoğraf
- 3-5 fotoğraf → sarı uyarı "Çeşitlilik az ama sonuç üretilebilir"
- 6-10 → yeşil "İyi çeşitlilik, kaliteli sonuç"
- 11-15 → yeşil "Mükemmel, en iyi sonuç"
- Bulanık/yarım fotoğraf → uyarı + kılavuz görseli
- NSFW → reddedilir
- Referans UI: https://higgsfield.ai/character/upload

**Studio Foto Oluşturma (Banana Pro)**
- Beyaz arka plan, boydan, normal duruş
- BİR KERE oluşturulur, kaydedilir, cache'lenir
- Fotoğraflar değişirse yeniden oluşturulur

**Adım 2: Kıyafet Seç (3 sekme)**
- **Dolabım:** Kategori gruplu — Üst | Alt | Ayakkabı | Dış | Aksesuar
- **URL:** Link yapıştır → AI görsel çıkar + bg kaldır + etiketle (3-5 sn)
- **Foto:** Çek veya galeriden yükle → AI segmente eder

**Kural:** Her kategoriden yalnızca 1. Farklı kaynaklardan seçimler birleşir.

**Adım 3: Otomatik Tamamlama**
- 0 parça seçiliyken → popup (tamamlat veya kendin seç)
- 1-2 parça seçiliyken → eksik için popup
- 3 parça tamam → direkt render

**Adım 4: Render**
- Kombin hash oluştur → cache kontrol
- Cache'teyse: anında göster (hak harcanmaz)
- Cache'te yoksa: Banana Pro → 5-8 sn → 9:16 sonuç
- Başarısız: 10 sn sonra otomatik 1 retry, yine başarısızsa "Tekrar dene" butonu
- Limit dolarsa → "Yükselt!" popup

**Adım 5: Sonuç Aksiyonları**
- Kaydet → kombinler listesine ekle
- Paylaş → 9:16 görsel, Instagram/WhatsApp (watermark yok)
- Oyla → Topluluk'ta oylama olarak paylaş

---

### SEKME 4: Dolabım

1. **Kapsül Gardırop Insight Kartı**
   - "%45 siyah kıyafetin var. Camel ton eklersen kombin seçeneğin %40 artar"
   - "Öner" tıkla → markalardan listeleme

2. **Filtreler:** Tümü | Üst | Alt | Dış | Ayakkabı | Aksesuar

3. **Kıyafet Grid'i** (3 sütun)
   - Görsel + kategori etiketi
   - Son kart = "+" ile yeni ekleme

**Kıyafet Ekleme Akışı (kritik):**

Adım 1: Görsel al (Çek | Galeriden | URL | Screenshot)
Adım 2: AI işle (NSFW kontrol → bg kaldır → otomatik etiket)
Adım 3: Bilgi formu (AI doldurur, kullanıcı onaylar)

Form Alanları:
| Alan | Zorunlu | AI Doldurur |
|------|---------|-------------|
| Kıyafet tipi (Üst/Alt/Dış/Ayakkabı/Aksesuar) | ✅ | ✅ |
| Alt kategori (tip'e göre değişir) | ✅ | ✅ |
| Renk (16 seçenek + özel) | ✅ | ✅ |
| Desen (Düz/Çizgili/Ekose/Çiçekli...) | ✅ | ✅ |
| Mevsim (çoklu seçim) | ✅ | ✅ |
| Formalite (Casual/Smart Casual/Business/Elegant/Spor) | ✅ | ✅ |
| Beden | ❌ | ❌ |
| Ölçüler | ❌ | ❌ |
| Görünürlük (Gizli/Görünür) | ✅ | ❌ |
| Not (max 500 karakter) | ❌ | ❌ |

Adım 4: Ekle → "Kıyafet eklendi!" mesajı

---

### Wishlist (Favoriler)

- Header'daki kalp ikonundan açılır (sağdan kayan panel)
- Kaynak: Topluluk oylamaları, profil kombinleri, Try-On URL, trend kombinler
- 2 sütun grid: görsel + kalp + kaynak etiketi
- Tıkla → detay veya Try-On

---

### Profil & Ayarlar

- Sağdan kayan panel (profil ikonu)
- Studio fotoğraf yükleme (try-on ile senkron)
- Foto kalite göstergesi
- Premium kart + yükselt butonu
- Kaydedilen kombinler
- Vücut bilgileri (boy/kilo/vücut tipi)
- Hesap yönetimi: dondur / sil (30 gün geri dönüş) / veri export (JSON)
- KVKK/GDPR uyumlu

---

## 🗄️ Veritabanı Şeması (ABP.io Entity'leri)

```
users           → email, phone, cinsiyet, studyo_foto, paket, gunluk_render_sayisi
user_photos     → foto_url, kalite_skoru, yuz_var_mi, tam_vucut_mu
garments        → kategori, alt_kategori, renk, desen, mevsim, formalite, beden, gorunurluk, kaynak
outfits         → kiyafet_idleri[], kombin_hash, uyum_skoru, render_url, gorunurluk
polls           → soru, secenekler[], toplam_oy
votes           → oylama_id, kullanici_id, secenek (tekil: 1 oylama-1 kullanıcı)
wishlist        → icerik_tipi, icerik_id, kaynak_tipi, kaynak_etiketi
follows         → takip_eden, takip_edilen (tekil çift)
blocks          → engelleyen, engellenen
reports         → raporlayan, icerik_tipi, sebep, durum
render_cache    → kombin_hash (anahtar), render_url
```

---

## 🔌 API Endpoints (43 adet)

```
POST   /api/auth/register
POST   /api/auth/verify-otp
POST   /api/auth/login
POST   /api/auth/forgot-password

GET    /api/weather

GET    /api/outfits/daily
GET    /api/outfits/suggestions
POST   /api/outfits/save
POST   /api/outfits/auto-complete
POST   /api/outfits/:id/share

GET    /api/trends/colors
GET    /api/trends/outfits

GET    /api/community/polls
POST   /api/community/polls
POST   /api/community/polls/:id/vote

GET    /api/users/:id/profile
POST   /api/users/:id/follow
POST   /api/users/:id/block

POST   /api/tryon/photos
POST   /api/tryon/studio-photo
GET    /api/tryon/studio-photo
POST   /api/tryon/render
GET    /api/tryon/render/:hash

POST   /api/garments/from-url
POST   /api/garments/from-photo

GET    /api/wardrobe
POST   /api/wardrobe/upload
PUT    /api/wardrobe/:id
DELETE /api/wardrobe/:id
GET    /api/wardrobe/:id
GET    /api/wardrobe/insights
POST   /api/wardrobe/analyze-image

POST   /api/similar/search
GET    /api/similar/:id/results

GET    /api/wishlist
POST   /api/wishlist
DELETE /api/wishlist/:id

GET    /api/profile
PUT    /api/profile
POST   /api/profile/freeze
POST   /api/profile/delete
POST   /api/profile/delete/cancel
GET    /api/profile/export

POST   /api/reports
```
Api endponitleri projenin akışına ve işlevselliğine uygun olarak tasarlanmalıdır. Endpointlerde ekleme veya silme yapılabilir gereksiz endpointler olmamalıdır. 
---

## 🤖 AI Pipeline (Banana Pro)

| İşlem | Ne Zaman | Ne Yapar | Süre |
|-------|----------|----------|------|
| NSFW Kontrolü | Her görsel yüklemede | Uygunsuz içerik taraması | <1 sn |
| Arka Plan Kaldırma | Her kıyafet görselinde | Transparan PNG | 1-2 sn |
| Otomatik Etiketleme | Bg kaldırma sonrası | Kategori/renk/desen/mevsim/formalite → JSON | 1 sn |
| Studio Foto | İlk 3 foto yüklendikten sonra | Beyaz bg, boydan, BİR KERE | 10-15 sn |
| Try-On Render | 3 parça seçilince | Studio foto + kıyafetler → 9:16 görsel | 5-8 sn |
| Kombin Önerisi | Kombinlerim açılınca | 10 uyumlu kombin | <1 sn |
| Otomatik Tamamlama | "Otomatik" denince | Eksik parçaları eşleştir | <1 sn |
| URL Scraping | URL yapıştırılınca | Görsel çıkar + bg kaldır + etiketle | 3-5 sn |

---

## ⚠️ Hata Durumları

| Durum | Kullanıcıya Gösterilen | Sistem Aksiyonu |
|-------|----------------------|-----------------|
| Bulanık fotoğraf | "Fotoğrafınız net değil. Işıklı ortamda tekrar çekin." | Reddedilir + kılavuz |
| Yarım vücut | "Vücudunuzun tamamı görünmüyor" + örnek | Reddedilir |
| NSFW görsel | "Bu görsel uygun bulunmadı." | Reddedilir + log |
| URL çalışmadı | "Bu site desteklenmiyor. Screenshot yükleyin." | Foto sekmesi önerilir |
| Render >30sn | "Tamamlanamadı. Tekrar dene" | 1 otomatik retry |
| Günlük limit | "Limitine ulaştın. Yükselt!" | Paket ekranına yönlendir |
| İnternet yok | "Bağlantınızı kontrol edin" | Tekrar dene butonu |
| Dolap boş | "Önce dolabına kıyafet ekle!" | Dolabım'a yönlendir |
| SMS süresi doldu | "Kod süresi doldu. Yeni kod gönder" | Yeni kod butonu |
| 3x yanlış SMS | "Çok fazla deneme. 15 dk bekle." | 15dk sayaç |
| Studio foto hata | "Fotoğrafınız işlenemedi. Farklı fotoğraflar deneyin." | Log + tekrar dene |

---

## 🔐 Güvenlik & Uyumluluk

- SMS OTP: 3 deneme → 15 dk kilit
- KVKK / GDPR: hesap silme 30 gün geri dönüşlü, veri export JSON
- Engelleme: karşılıklı görünmezlik
- İçerik moderasyonu: AI otomatik + "Raporla" butonu
- Render cache: aynı kombin = hak harcanmaz

---

## 🛠️ Geliştirme Öncelikleri (Faz Sırası)
 Önce backend geliştirilir. Sonra mobile geliştirilir.

**Faz 1 — Core:**
1. Auth (kayıt/OTP/giriş)
2. Dolap (kıyafet ekleme + AI etiketleme)
3. Try-On (studio foto + render)

**Faz 2 — Social:**
4. Kombinlerim (öneriler + hava durumu)
5. Topluluk (oylamalar + profil)
6. Wishlist

**Faz 3 — Growth:**
7. Benzerini Bul
8. Premium/paket sistemi
9. Affiliate/satıcı yönlendirme
10. Bildirimler
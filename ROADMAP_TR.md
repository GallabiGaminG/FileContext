# FileContext Roadmap

## Amaç

FileContext dosyaları yönetmekten çok, dosyalar hakkındaki kararları yönetir.

- Explorer dosyayı taşır.
- Everything dosyayı bulur.
- Duplicate tool eşleşmeyi bulur.

FileContext ise şu soruların bağlamını tutar:

> Bu dosya neden burada?  
> Bununla ne yapacaktım?  
> Eski yeri neresiydi?  
> Bu kopya neden oluştu?  
> Güvenli karar ne?

---

## CURRENT / IMPLEMENTED

- .NET 8 WPF
- Local SQLite database
- Context kayıtları
- Başlık / Path / Açıklama
- Durum
- Sonraki işlem
- Etiketler
- Yeni kayıt
- Düzenleme
- Silme
- Dosya / klasör açma
- Native Windows dosya seçici
- Native Windows klasör seçici
- Path validation
- Everything entegrasyonu
- Bundled `tools\es.exe`
- Everything runtime dependency kontrolü
- Everything otomatik background başlatma
- Context + Everything birleşik arama
- Everything sonucundan “Bağlama Ekle”
- Search debounce
- Paged result loading
- UI virtualization / recycling
- Büyük sonuçlarda responsive arama
- “Everything aranıyor...” durumu
- Resizable Context / Everything panes
- Uzun path için ellipsis + tooltip
- Storage Overview temel disk görünümü
- Canlı disk Read / Write MB/s
- Active I/O process listesi
- PID + process bazında I/O
- ETW tabanlı Recent File I/O
- Gerçek READ / WRITE path takibi
- Muhtemel source → destination korelasyonu
- Confidence scoring
- `System / PID 4` write event’lerini korelasyonda değerlendirme
- Live telemetry Pause / Resume
- Teknik veriler için selectable / copyable alanların ilk uygulaması
- Runtime UI freeze sorunlarında background çalışma + overlap engelleme

---

# 01 — STORAGE OVERVIEW / STORAGE MAP

## Amaç

Bilgisayardaki diskleri teknik isimlerden çok anlamlı biçimde göstermek.

### Gösterilecek bilgiler

- Drive letter
- Volume label
- Fiziksel disk modeli
- Toplam kapasite
- Kullanılan alan
- Boş alan
- Kullanım yüzdesi
- USB / SATA / NVMe
- HDD / SSD
- Sistem diski / harici disk
- Steam benzeri görsel kapasite çubuğu

### Örnek

```text
22TB_DEPO (E:)
Toshiba MG10
Internal SATA HDD

Toplam: 20 TB
Kullanılan: 9.1 TB
Boş: 10.9 TB
```

## Geliştirme Aşamaları

### Storage Overview v1

- [x] Diskleri listeleme
- [x] Volume label
- [x] Toplam kapasite
- [x] Kullanılan alan
- [x] Boş alan
- [x] Kullanım yüzdesi
- [x] Görsel kullanım çubuğu

### Storage Overview v1.1

- [x] Canlı disk aktivitesi
- [x] Read MB/s
- [x] Write MB/s
- [x] Aktivite değerlerini periyodik yenileme
- [x] Ağır WMI sorgularını UI thread dışına alma
- [x] Recurring task overlap engelleme

### Storage Overview v1.2

- [x] Aktif I/O kullanan process
- [x] Process bazında disk kullanımı
- [x] ETW ile gerçek dosya READ / WRITE path takibi
- [x] Muhtemel source → destination ilişkisi
- [x] Confidence scoring
- [x] `System / PID 4` write event’lerini değerlendirme
- [x] Tahmin edilen ilişkileri kesin bilgi gibi göstermeme
- [x] Pause / Resume ile hızlı akan telemetry’yi inceleyebilme

### Storage Overview — kalan donanım kimliği katmanı

- [x] Fiziksel disk modeli
- [x] HDD / SSD tespiti
- [x] USB / SATA / NVMe tespiti
- [ ] Sistem diski / harici / removable ayrımı
- [ ] İnsan okunur cihaz kimliği

### Migration Assistant entegrasyonu

- [ ] Kesin source → destination
- [ ] Aktif dosya
- [ ] Progress
- [ ] Transfer speed
- [ ] ETA

---

# 02 — STORAGE CLASSIFICATION

Disk alanını anlamlı kategorilere ayır.

## Alan kategorileri

- Sistem / Dokunma
- Uygulamalar
- Kullanıcı verisi
- Temp
- Cache
- Temizlenebilir
- Arşiv
- Taşınacak
- Düzenlenecek
- Duplicate şüphesi
- Aktif proje
- Boş alan

## Güvenlik / karar sınıfları

- SYSTEM / DO NOT TOUCH
- APPLICATION FILE
- USER DATA
- CACHE
- TEMP
- LOG
- ARCHIVE
- LEFTOVER / ORPHAN CANDIDATE
- SAFE TO REVIEW
- SAFE TO CLEAN
- UNKNOWN / USER REVIEW REQUIRED

## Amaç

Yalnızca:

> “Disk dolu.”

demek değil; şunları cevaplamak:

- Disk neden dolu?
- Ne silinebilir?
- Ne taşınmalı?
- Neye dokunulmamalı?
- Neden bu öğe temizlenebilir / riskli / belirsiz olarak işaretlendi?

---

# 03 — PROGRAM-BASED STORAGE ANALYSIS

## Amaç

Bir uygulamanın gerçek disk etkisini göstermek.

## Program başına mümkün olduğunda

- Installation files
- Program Files
- Program Files (x86)
- ProgramData
- AppData Local
- AppData Roaming
- Cache
- Temp
- Logs
- User content
- Kullanıcının oluşturduğu proje / içerik klasörleri
- İlgili çalışma dizinleri
- Installer / update / download cache’leri
- Uninstall / registry metadata üzerinden ilişkili path’ler
- Kurulu uygulama tespiti
- Kaldırılmış uygulama kalıntıları
- Orphan / leftover candidate tespiti
- Programın toplam disk etkisi
- Bir dosyanın / klasörün neden ilişkili kabul edildiğini açıklama
- Kalıntıları sessizce otomatik silmeme

## Örnek

```text
DaVinci Resolve

Program: 4 GB
Cache: 310 GB
Projects: 18 GB
Diğer: 2 GB

Toplam disk etkisi: 334 GB
```

Program kaldırılmışsa örnek yaklaşım:

```text
Adobe Premiere Pro
Status: Not installed

Possible leftovers:
AppData / ProgramData / Cache / Logs ...

Classification:
LEFTOVER / ORPHAN CANDIDATE
```

Olası aksiyonlar:

- Review
- Open Location
- Mark as Keep
- Mark as Cleanable

---

# 04 — CONTEXT / PROVENANCE EXPANSION

Mevcut context çekirdeğini genişlet.

- Eski path
- Yeni path
- PreviousPath
- SourceDrive
- TargetDrive
- MovedAt
- MoveReason
- Provenance / taşıma geçmişi

Fiziksel dosya taşınsa bile mantıksal geçmiş korunmalı.

---

# 05 — DUPLICATE ANALYZER

Duplicate kontrolü aşamalı yapılmalı.

## Aşama 1 — hızlı aday bulma

- Dosya boyutu
- İsim
- Tarih
- Uzantı

## Aşama 2

- Gerekirse partial hash

## Aşama 3

- Full hash ile kesin doğrulama

## Duplicate grup aksiyonları

- Keep
- Delete
- Ignore
- Keep Both

Fiziksel duplicate silinse bile provenance kaydı korunmalı.

---

# 06 — MIGRATION / CONSOLIDATION ASSISTANT

## Gerçek kullanım senaryosu

```text
Eski diskler
→ 22TB_DEPO\_To_Sort
→ Duplicate kontrol
→ Sınıflandırma
→ Yeni kalıcı path
→ Eski path güncelleme
→ Provenance kaydı
```

## Takip edilecek alanlar

- Kaynak disk
- Eski path
- Geçici path
- Yeni path
- Durum
- Taşıma tarihi
- Taşıma nedeni
- Duplicate durumu
- Conflict durumu

FileContext tarafından başlatılan migration işlemlerinde ileride:

- Kesin source → destination
- Aktif dosya
- Progress
- Transfer speed
- ETA

gösterilebilmeli.

---

# 07 — CONFLICT INBOX

Aynı isimli ama farklı dosya bulunduğunda otomatik overwrite yapılmamalı.

## Seçenekler

- Şimdi karar ver
- Güvenli yedekle ve devam et
- Atla

Sonradan Conflict Inbox içinde analiz edilebilir.

## Text dosyada

- Diff

## Binary dosyada

- Boyut
- Tarih
- Hash

Kaynak ve hedef versiyon birlikte tutulabilmeli.

İkisini de koruma durumunda anlamsız `(1)` isimleri yerine provenance içeren isim tercih edilebilir.

---

# 08 — SAFE BACKUP POLICY

Conflict backup alanının kontrolsüz büyümesini engelle.

## Ayarlar

- Maksimum güvenli yedek boyutu
- Backup quota
- Kota yaklaşınca uyarı
- Retention süresi
- Eski conflict backup temizliği
- Büyük dosya yedekleme uyarısı

## Örnek seçenekler

- 50 GB
- 100 GB
- 500 GB
- Custom

---

# 09 — SAFE EJECT / DEVICE USAGE DIAGNOSTICS

Harici disk çıkarılmak istendiğinde kullanım durumunu analiz et.

Windows:

> Device is in use.

FileContext:

> Hangi işlem kullanıyor ve neden?

## Mümkün olduğunda göster

- Process adı
- PID
- İlgili path
- Açık file handle
- Açık directory handle
- Aktif disk I/O
- Okuma / yazma durumu

## Durum sınıfları

### KIRMIZI

- Aktif yazma
- Aktif kopyalama
- Migration devam ediyor
- Kesinlikle sökme

### SARI

- Açık handle
- Explorer / terminal / program diski tutuyor
- Şüpheli kullanım

### YEŞİL

- FileContext aktif kullanım tespit etmedi

Önemli:

> Yeşil durum bile “kesin güvenli” anlamına gelmez.  
> Nihai eject kararını Windows verir.

## Butonlar / aksiyonlar

- Kullanan işlemi göster
- Konumu göster
- Tekrar kontrol et
- Güvenli çıkarmayı tekrar dene

FileContext migration bilgisi varsa daha anlamlı mesaj:

```text
Steam Library Migration devam ediyor.
Kaynak: G:
Hedef: E:
Bu diski şimdi çıkarmayın.
```

Mevcut disk telemetry + ETW altyapısı bu modül için temel oluşturur.

---

# 10 — LINK / JUNCTION / SHORTCUT SUPPORT

Dosya taşındıktan sonra eski path kırılacaksa:

- Shortcut öner
- Junction öner
- Symlink öner

Ama şu konularda açık uyarı ver:

- Permission
- Compatibility
- Administrator requirement
- Risk

---

# 11 — POWER USER SETTINGS

Üst düzey kullanıcı için ayarlanabilir davranışlar.

## Search

- Everything result limit
- Page size
- Search debounce
- Scan scope
- Virtualization behavior

## Duplicate

- Hash strategy
- Duplicate depth
- Partial hash aç / kapat
- Full hash threshold

## Migration

- Conflict default behavior
- Safe backup quota
- Backup retention
- Default target rules
- Junction suggestion

## Safe Eject

- Handle scan aç / kapat
- Disk I/O gözlem süresi
- Safe eject kontrol derinliği
- Confirmation seviyesi

## Telemetry / Live Monitoring

- Refresh interval
- Pause / Resume davranışı
- Event history limiti
- Correlation confidence threshold
- ETW monitoring aç / kapat

## General

- Riskli işlemlerde confirmation seviyesi
- Log / diagnostic detay seviyesi

---

# 12 — MAIN NAVIGATION

Mevcut tek ekran her özelliği taşımaya zorlanmamalı.

## Planlanan ana modüller

- Context
- Storage
- Duplicates
- Migration
- Settings

## Gerekirse ayrıca

- Conflicts
- Devices

---

# 13 — PRIVILEGED HELPER / LEAST PRIVILEGE

Ana uygulama gereksiz yere Administrator çalışmamalı.

- `FileContext.exe` normal user olarak çalışır.
- `FileContext.ElevatedHelper.exe` yalnızca gerektiğinde UAC ister.
- ETW / privileged handle / device işlemleri helper üzerinden yürür.
- Helper küçük, denetlenebilir ve mümkün olduğunca kısa ömürlü olmalı.
- Normal Context / Search / Storage / Duplicate işlemleri elevated yetki istememeli.

---

# 14 — UI / VISUAL POLISH

Fonksiyonlar oturduktan sonra.

- Gumball mavisi → Darwin turuncusu gradient
- Steam benzeri disk kullanım çubukları
- Durum renkleri
- Risk / güvenli / temp görsel ayrımları
- Disk kartları
- Daha tutarlı spacing / typography
- App icon

## Selectable / Copyable Technical Text

Teknik ve kullanıcı tarafından tekrar kullanılabilecek bilgiler seçilebilir / kopyalanabilir olmalı:

- Path
- PID
- Process name
- Disk bilgileri
- Read / Write değerleri
- Hash / diagnostic detayları
- Context path

Hızlı akan telemetry için:

- Pause / Resume
- İleride gerekirse Snapshot / Freeze Selected Event

---

# DEVELOPMENT PRINCIPLES

- Local-first
- Runtime AI zorunluluğu yok
- Deterministic davranış öncelikli
- Sessiz overwrite yok
- Sessiz delete yok
- Belirsizlikte geri dönüş imkanı korunur
- Provenance mümkün olduğunca kaybolmaz
- Kullanıcı anlık karar vermeye zorlanmaz
- Risk açıkça gösterilir
- FileContext mümkün olduğunda açıklar, karar kullanıcıda kalır
- Heavy work → background
- UI update → UI thread
- Recurring task → overlap engelle
- Large result → pagination + virtualization
- Least privilege
- Tahmin edilen bilgi kesin gerçek gibi sunulmaz

FileContext Roadmap

Amaç:
FileContext dosyaları yönetmekten çok, dosyalar hakkındaki kararları yönetir.

Explorer dosyayı taşır.
Everything dosyayı bulur.
Duplicate tool eşleşmeyi bulur.

FileContext ise:
“Bu dosya neden burada, bununla ne yapacaktım, bunun eski yeri neresiydi,
bu kopya neden oluştu ve güvenli karar ne?” sorularının bağlamını tutar.



==================================================
CURRENT / IMPLEMENTED
===

* .NET 8 WPF
* Local SQLite database
* Context kayıtları
* Başlık / Path / Açıklama
* Durum
* Sonraki işlem
* Etiketler
* Yeni kayıt
* Düzenleme
* Silme
* Dosya / klasör açma
* Native Windows dosya seçici
* Native Windows klasör seçici
* Path validation
* Everything entegrasyonu
* Bundled tools\\es.exe
* Everything runtime dependency kontrolü
* Everything otomatik background başlatma
* Context + Everything birleşik arama
* Everything sonucundan “Bağlama Ekle”
* Search debounce
* Paged result loading
* UI virtualization / recycling
* Büyük sonuçlarda responsive arama
* “Everything aranıyor...” durumu
* Resizable Context / Everything panes
* Uzun path için ellipsis + tooltip



==================================================
01 — STORAGE OVERVIEW / STORAGE MAP
===

İlk sıradaki aktif geliştirme.

Amaç:
Bilgisayardaki diskleri teknik isimlerden çok anlamlı biçimde göstermek.

Gösterilecek bilgiler:

* Drive letter
* Volume label
* Fiziksel disk modeli
* Toplam kapasite
* Kullanılan alan
* Boş alan
* Kullanım yüzdesi
* USB / SATA / NVMe
* HDD / SSD
* Sistem diski / harici disk
* Steam benzeri görsel kapasite çubuğu

Örnek:

22TB\_DEPO (E:)
Toshiba MG10
Internal SATA HDD

Toplam: 20 TB
Kullanılan: 9.1 TB
Boş: 10.9 TB



==================================================
02 — STORAGE CLASSIFICATION
===

Disk alanını anlamlı kategorilere ayır.

* Sistem / Dokunma
* Uygulamalar
* Kullanıcı verisi
* Temp
* Cache
* Temizlenebilir
* Arşiv
* Taşınacak
* Düzenlenecek
* Duplicate şüphesi
* Aktif proje
* Boş alan

Amaç yalnızca “disk dolu” demek değil:

“Disk neden dolu?”
“Ne silinebilir?”
“Ne taşınmalı?”
“Neye dokunulmamalı?”



==================================================
03 — PROGRAM-BASED STORAGE ANALYSIS
===

Amaç:
Bir uygulamanın gerçek disk etkisini göstermek.

Program başına mümkün olduğunda:

* Installation files
* Program Files
* ProgramData
* AppData
* Cache
* Temp
* Logs
* User content
* İlgili çalışma dizinleri

Örnek:

DaVinci Resolve

Program: 4 GB
Cache: 310 GB
Projects: 18 GB
Diğer: 2 GB

Toplam disk etkisi: 334 GB



==================================================
04 — CONTEXT / PROVENANCE EXPANSION
===

Mevcut context çekirdeğini genişlet.

* Eski path
* Yeni path
* PreviousPath
* SourceDrive
* TargetDrive
* MovedAt
* MoveReason
* Provenance / taşıma geçmişi

Fiziksel dosya taşınsa bile mantıksal geçmiş korunmalı.



==================================================
05 — DUPLICATE ANALYZER
===

Duplicate kontrolü aşamalı yapılmalı.

Aşama 1:

* Dosya boyutu
* İsim
* Tarih
* Uzantı

Aşama 2:

* Gerekirse partial hash

Aşama 3:

* Full hash ile kesin doğrulama

Duplicate gruplarında:

* Keep
* Delete
* Ignore
* Keep Both

Fiziksel duplicate silinse bile provenance kaydı korunmalı.



==================================================
06 — MIGRATION / CONSOLIDATION ASSISTANT
===

Gerçek kullanım senaryosu:

Eski diskler
→ 22TB\_DEPO\_To\_Sort
→ Duplicate kontrol
→ Sınıflandırma
→ Yeni kalıcı path
→ Eski path güncelleme
→ Provenance kaydı

Takip edilecek alanlar:

* Kaynak disk
* Eski path
* Geçici path
* Yeni path
* Durum
* Taşıma tarihi
* Taşıma nedeni
* Duplicate durumu
* Conflict durumu



==================================================
07 — CONFLICT INBOX
===

Aynı isimli ama farklı dosya bulunduğunda otomatik overwrite yapılmamalı.

Seçenekler:

* Şimdi karar ver
* Güvenli yedekle ve devam et
* Atla

Sonradan Conflict Inbox içinde analiz edilebilir.

Text dosyada:

* Diff

Binary dosyada:

* Boyut
* Tarih
* Hash

Kaynak ve hedef versiyon birlikte tutulabilmeli.

İkisini de koruma durumunda anlamsız “(1)” isimleri yerine provenance içeren isim tercih edilebilir.



==================================================
08 — SAFE BACKUP POLICY
===

Conflict backup alanının kontrolsüz büyümesini engelle.

Ayarlar:

* Maksimum güvenli yedek boyutu
* Backup quota
* Kota yaklaşınca uyarı
* Retention süresi
* Eski conflict backup temizliği
* Büyük dosya yedekleme uyarısı

Örnek seçenekler:

50 GB
100 GB
500 GB
Custom



==================================================
09 — SAFE EJECT / DEVICE USAGE DIAGNOSTICS
===

Harici disk çıkarılmak istendiğinde kullanım durumunu analiz et.

Windows:
“Device is in use.”

FileContext:
“Hangi işlem kullanıyor ve neden?”

Mümkün olduğunda göster:

* Process adı
* PID
* İlgili path
* Açık file handle
* Açık directory handle
* Aktif disk I/O
* Okuma / yazma durumu

Durum sınıfları:

KIRMIZI

* Aktif yazma
* Aktif kopyalama
* Migration devam ediyor
* Kesinlikle sökme

SARI

* Açık handle
* Explorer / terminal / program diski tutuyor
* Şüpheli kullanım

YEŞİL

* FileContext aktif kullanım tespit etmedi

Önemli:
Yeşil durum bile “kesin güvenli” anlamına gelmez.
Nihai eject kararını Windows verir.

Butonlar:

* Kullanan işlemi göster
* Konumu göster
* Tekrar kontrol et
* Güvenli çıkarmayı tekrar dene

FileContext migration bilgisi varsa daha anlamlı mesaj:

“Steam Library Migration devam ediyor.
Kaynak: G:
Hedef: E:
Bu diski şimdi çıkarmayın.”



==================================================
10 — LINK / JUNCTION / SHORTCUT SUPPORT
===

Dosya taşındıktan sonra eski path kırılacaksa:

* Shortcut öner
* Junction öner
* Symlink öner

Ama:

* Permission
* Compatibility
* Administrator requirement
* Risk

konularında açık uyarı ver.



==================================================
11 — POWER USER SETTINGS
===

Üst düzey kullanıcı için ayarlanabilir davranışlar.

Search:

* Everything result limit
* Page size
* Search debounce
* Scan scope
* Virtualization behavior

Duplicate:

* Hash strategy
* Duplicate depth
* Partial hash aç/kapat
* Full hash threshold

Migration:

* Conflict default behavior
* Safe backup quota
* Backup retention
* Default target rules
* Junction suggestion

Safe Eject:

* Handle scan aç/kapat
* Disk I/O gözlem süresi
* Safe eject kontrol derinliği
* Confirmation seviyesi

General:

* Riskli işlemlerde confirmation seviyesi
* Log / diagnostic detay seviyesi



==================================================
12 — MAIN NAVIGATION
===

Mevcut tek ekran her özelliği taşımaya zorlanmamalı.

Planlanan ana modüller:

* Context
* Storage
* Duplicates
* Migration
* Settings

Gerekirse ayrıca:

* Conflicts
* Devices



==================================================
13 — UI / VISUAL POLISH
===

Fonksiyonlar oturduktan sonra.

* Gumball mavisi → Darwin turuncusu gradient
* Steam benzeri disk kullanım çubukları
* Durum renkleri
* Risk / güvenli / temp görsel ayrımları
* Disk kartları
* Daha tutarlı spacing / typography
* App icon



==================================================
DEVELOPMENT PRINCIPLES
===

* Local-first
* Runtime AI zorunluluğu yok
* Deterministic davranış öncelikli
* Sessiz overwrite yok
* Sessiz delete yok
* Belirsizlikte geri dönüş imkanı korunur
* Provenance mümkün olduğunca kaybolmaz
* Kullanıcı anlık karar vermeye zorlanmaz
* Risk açıkça gösterilir
* FileContext mümkün olduğunda açıklar, karar kullanıcıda kalır



\### Geliştirme Aşamaları



\#### Storage Overview v1

\- \[x] Diskleri listeleme

\- \[x] Volume label

\- \[x] Toplam kapasite

\- \[x] Kullanılan alan

\- \[x] Boş alan

\- \[x] Kullanım yüzdesi

\- \[x] Görsel kullanım çubuğu



\#### Storage Overview v1.1

\- \[ ] Canlı disk aktivitesi

\- \[ ] Read MB/s

\- \[ ] Write MB/s

\- \[ ] Aktivite değerlerini periyodik yenileme



\#### Storage Overview v1.2

\- \[ ] Aktif I/O kullanan process

\- \[ ] Process bazında disk kullanımı

\- \[ ] Muhtemel source → destination ilişkisi

\- \[ ] Tahmin edilen ilişkileri kesin bilgi gibi göstermeme



\#### Migration Assistant entegrasyonu

\- \[ ] Kesin source → destination

\- \[ ] Aktif dosya

\- \[ ] Progress

\- \[ ] Transfer speed

\- \[ ] ETA


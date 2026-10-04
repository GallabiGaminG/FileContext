# FileContext

FileContext, dosya ve klasörlere insan bağlamı eklemek için geliştirilmiş yerel bir Windows yardımcı uygulamasıdır.

Everything şu soruyu cevaplar:

> Bu dosya nerede?

FileContext ise şunu cevaplamayı amaçlar:

> Neden burada, hangi projeye ait ve bununla ne yapmayı planlıyordum?

## Özellikler

- Yerel SQLite veritabanı
- Dosya ve klasörler için bağlam notları
- Durum takibi
- Sonraki işlem takibi
- Etiketler
- Bağlam metadata'sı içinde arama
- Everything dosya sistemi arama entegrasyonu
- Everything arama sonuçlarını doğrudan FileContext'e ekleme
- Bağlam kayıtlarını açma, düzenleme ve silme
- Yerel Windows dosya ve klasör seçicileri
- Path doğrulama
- Sayfalı Everything sonuçları
- Sanallaştırılmış sonuç gösterimi
- Yeniden boyutlandırılabilir bağlam ve arama panelleri

## Gereksinimler

- Windows
- .NET 8
- voidtools Everything

FileContext, Everything indeksini sorgulamak için kullanılan Everything Command-line Interface (`es.exe`) aracını içerir.

Everything uygulamasının kendisi sistemde kurulu olmalıdır. Everything kurulu ancak çalışmıyorsa FileContext arka planda başlatmayı dener.

## Veri

FileContext yerel veritabanını şu konumda saklar:

`%LOCALAPPDATA%\FileContext\FileContext.db`

Herhangi bir bulut servisi veya çalışma zamanı AI servisi gerekli değildir.

## Üçüncü parti yazılımlar

Bkz. `THIRD_PARTY_NOTICES.txt`.

## Geliştirme notu

Bu proje, ChatGPT ile AI destekli kodlama ve tasarım görüşmeleri kullanılarak geliştirilmiştir.
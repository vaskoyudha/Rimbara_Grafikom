# Kandidat asset NPC gratis dan lisensi terbuka

Tanggal pemeriksaan: **16 November 2026**.
Scope: mencari kandidat untuk Tetua Desa, Petugas Hutan, koordinator, relawan,
dan warga RIMBARA. Kajian awal berikut dipertahankan sebagai sejarah pemilihan.
**Model MakeHuman/MPFB sudah terpasang dan regresi build/runtime lulus**, lihat
`Docs/Human-NPC-Implementation.md` untuk sumber paket aktual dan hasil validasi.

## Rekomendasi ringkas

Tidak ditemukan satu pack siap pakai yang sekaligus mempunyai semua peran,
proporsi cocok dengan Ch42, busana yang sesuai desa Indonesia, dan lisensi
terbuka yang konsisten. Ada tiga jalur realistis:

1. **MakeHuman / MPFB untuk NPC humanoid custom**: paling cocok jika ingin
   mempertahankan proporsi manusia seperti player sekarang, membuat Tetua
   benar-benar lebih tua, dan mengatur wajah/pakaian untuk tiap peran. Ini tool
   pembuatan karakter, bukan pack NPC sekali klik; butuh optimasi dan authoring.
2. **Kenney Mini Characters untuk satu gaya NPC sederhana yang konsisten**:
   CC0, variasi karakter dan aksesori, tetapi proporsi mini/chibi berbeda jauh
   dari player Ch42. Cocok jika tim menerima perubahan arah visual karakter.
3. **KayKit Adventurers untuk Petugas Hutan bergaya fantasy**: Ranger termasuk
   versi gratis. Hapus busur/quiver, sesuaikan warna/outfit. Bukan seragam petugas
   hutan Indonesia dan bukan solusi siap pakai untuk semua warga/Tetua.

## Kandidat dan kecocokan

| Kandidat | Lisensi yang terlihat | Isi gratis yang terkonfirmasi di halaman | Peran / batas |
| --- | --- | --- | --- |
| [MakeHuman](https://github.com/makehumancommunity/makehuman) / [MPFB](https://github.com/makehumancommunity/mpfb2) | Bundled graphical assets/output CC0; code tool AGPL / GPLv3 | Generator manusia dan asset bawaan; MPFB bekerja di Blender, membutuhkan Blender >= 4.2 menurut README | Tetua, Petugas Hutan, koordinator dan warga dengan proporsi humanoid. Perlu membuat/export model, pakaian, optimasi dan animasi; asset komunitas bukan otomatis CC0 |
| [Kenney Mini Characters](https://kenney.nl/assets/mini-characters) | CC0 | Halaman mencatat 40 files dan animation; preview menunjukkan pakaian/aksesori beragam | Warga/relawan; variasi topi, kacamata dan tubuh. Sangat chibi, tidak cocok langsung dengan Ch42 tanpa review skala/gaya |
| [Kenney Blocky Characters](https://kenney.nl/assets/blocky-characters) | CC0 | 20 files, animation; preview manusia, karakter berkacamata dan pakaian berbeda | Alternatif Tetua/warga sederhana; bentuk kotak/voxel berbeda dari environment dan player |
| [KayKit Adventurers](https://kaylousberg.itch.io/kaykit-adventurers) | CC0 pada description dan metadata itch.io | Free tier: 5 characters, movement animations, aksesori, FBX/GLTF; 12 MB pada saat kajian | Ranger sebagai basis petugas. Mage hanya basis modifikasi Tetua dan tampak muda; perlu rambut/janggut/busana. Engineer/Druid termasuk EXTRA, **bukan gratis**; .blend SOURCE juga berbayar |
| [Kenney Animated Characters Survivors](https://kenney.nl/assets/animated-characters-survivors) | CC0 | 8 files; preview 2 survivor manusia dan 2 zombie | Warga terdampak bencana memakai versi manusia, bukan zombie. Butuh variasi wajah/outfit dan material lebih bersih di Zona 3 |
| [Kenney Animated Characters Protagonists](https://kenney.nl/assets/animated-characters-protagonists) | CC0 | 8 files; preview pakaian kasual/karakter berkumis, juga cyborg | Warga/koordinator setelah recolor; hindari cyborg dan motif yang tidak sesuai cerita |

Angka file bukan klaim jumlah karakter unik/animasi. Label animation di halaman
belum menjamin rig Humanoid Unity cocok dengan Animator project; perlu import test.
CC0 mengizinkan modifikasi dan distribusi copyrighted asset tanpa attribution
wajib, tetapi tidak memberi izin trademark atau endorsement. Tetap simpan provenance.

## Usulan identitas per NPC

- **Tetua Desa:** manusia lansia, rambut abu/putih, pakaian sederhana bernuansa
  krem/cokelat, postur tenang. Jangan gunakan mahkota/armor atau topi penyihir
  sebagai penampilan final. Jalur MakeHuman/MPFB paling fleksibel untuk ini.
- **Petugas Hutan:** baju lapangan hijau/khaki, celana panjang, boots dan topi
  lapangan; tidak membawa busur/senjata. Ranger fantasy membutuhkan modifikasi.
- **Koordinator:** pakaian lapangan dengan rompi penanda sederhana, dibedakan
  dari relawan melalui warna/aksesori, bukan menyalin uniform/logo instansi.
- **Relawan:** outfit kerja praktis, rompi terang, beberapa variasi wajah/tubuh.
- **Warga:** pria/wanita beragam dengan pakaian sehari-hari, bukan semua Ch42
  berbaju merah. Zona 1/2/3 bisa memakai model identitas yang sama dengan variasi
  material/outfit yang sesuai, bukan berubah menjadi orang lain.

Tidak ada kandidat di atas yang terkonfirmasi menyediakan busana/etnis lokal
Indonesia spesifik siap pakai. Styling lokal perlu authoring dan review tim;
jangan mengklaim karakter fantasy Barat sebagai model warga Indonesia autentik.

## Temuan Quaternius — lisensi perlu klarifikasi

Secara visual pack berikut cocok:

- [Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html):
  11 characters, 24 animations, 4 bagian modular per karakter menurut halaman;
  preview menunjukkan backpacker, safety worker, pria berjanggut putih dan warga.
- [Ultimate Modular Women](https://quaternius.com/packs/ultimatemodularwomen.html):
  10 characters, 24 animations; outdoor/worker/casual variants.
- [RPG Character Pack](https://quaternius.com/packs/rpgcharacters.html):
  6 rigged/animated/textured fantasy characters; wizard berjanggut putih cocok
  sebagai basis Tetua, tetapi busana ornamental/magic harus disederhanakan.
- [Modular Character Outfits — Fantasy](https://quaternius.itch.io/modular-character-outfits-fantasy):
  Standard gratis hanya Ranger/Peasant; .blend/source dan semua outfit bukan gratis.

**Konflik sumber resmi ditemukan:** halaman pack/FAQ masih menampilkan CC0,
tetapi [halaman lisensi umum](https://quaternius.com/license.html) menampilkan
**Quaternius Asset License (QAL) v1.0**, diperbarui 28 Agustus 2026. QAL mengizinkan
pemakaian/modifikasi dalam produk, tetapi membatasi redistribusi raw asset termasuk
yang sudah dimodifikasi. Halaman itu juga mengatakan perubahan tidak retroaktif
terhadap asset yang sudah diperoleh dengan lisensi lama.

Karena itu **jangan menganggap download baru otomatis CC0** hanya dari badge
pack lama. Periksa LICENSE di arsip/release yang persis akan digunakan dan
konfirmasi dengan pencipta jika konflik tetap ada. Salinan CC0 yang telah sah
diperoleh sebelumnya berbeda dari asumsi hak untuk release baru. Kandidat ini
ditahan untuk publikasi raw source di GitHub sampai statusnya jelas; tidak ada
asset Quaternius baru yang diimpor dalam kajian ini.

## Animasi gratis pendukung

[KayKit Character Animations](https://kaylousberg.itch.io/kaykit-character-animations)
menyatakan CC0, 161 Humanoid animations, semua animasi gratis; FBX/GLTF tersedia,
sedangkan file sumber .blend ada pada tier berbayar. Mencakup idle, movement,
waving, cheering, sitting dan tool actions (digging/hammering/fishing).

Author menyatakan retargeting ke non-KayKit memungkinkan, tetapi proporsi dapat
membuat hasil tidak bagus. Periksa tangan/kaki, root motion dan scale dalam Unity;
jangan menyatakan kompatibel penuh sebelum diuji.

## Gerbang implementasi setelah kandidat dipilih

1. Unduh dari pencipta/distributor resmi, pilih versi gratis; jangan membeli tier
   atau mengirim pesan ke author tanpa persetujuan.
2. Simpan LICENSE, URL, tanggal, release/hash, attribution jika diperlukan.
3. Import satu model ke folder asset baru; jangan overwrite Ch42/prefab lama.
4. Uji rig, animasi, scale, material URP, silhouette dan collision/kamera.
5. Ganti **visual child** NPC sambil mempertahankan interaction root, ID, trigger,
   dialog, visibility chapter dan referensi. Jangan ikut mengubah player bila
   hanya NPC yang dipilih, kecuali keputusan gaya visual meminta itu.
6. Hapus overlay placeholder jubah/cap/badge hanya setelah visual baru sesuai;
   jangan menimpa perubahan user atau mengganti asset vendor source.
7. Jalankan scene/language audit, build dan runtime normal/skip/replay. Tinjau
   close-up Tetua, petugas, koordinator dan warga dari ketiga state lingkungan.
8. Publikasi source hanya sesudah lisensi arsip jelas. Mengganti NPC tidak
   mengubah seluruh project menjadi open-source/CC0: asset lain tetap lisensinya.

## Sumber lisensi yang diperiksa

- [CC0 summary](https://creativecommons.org/publicdomain/zero/1.0/).
- [MakeHuman license overview](https://github.com/makehumancommunity/makehuman/blob/master/LICENSE.md)
  dan [asset license](https://github.com/makehumancommunity/makehuman/blob/master/LICENSE.ASSETS.md).
- [MPFB license overview](https://github.com/makehumancommunity/mpfb2/blob/master/LICENSE.md).
- Halaman Kenney/KayKit masing-masing kandidat di atas.
- [Quaternius QAL](https://quaternius.com/license.html) dan
  [FAQ yang masih menyebut CC0](https://quaternius.com/faq.html).

Shortlist di atas bukan bukti kompatibilitas setiap kandidat. Hanya jalur
MakeHuman/MPFB yang dipilih untuk implementasi berproporsi manusia.

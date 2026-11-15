# PRD — Zona 3: Hutan Pulih

**Product Requirements Document / RIMBARA — Dari Abu Menuju Kehidupan**

| Atribut | Nilai |
| --- | --- |
| Versi | 0.1 — draft untuk review tim |
| Tanggal | 16 November 2026 |
| Status | Perencanaan; belum diimplementasikan atau disetujui sebagai desain final |
| Target | Zona 3 / M05 / penutup perjalanan lima misi |
| Bahasa pemain | Bahasa Indonesia (id-ID) |
| Unity | 6000.3.6f1 |
| Scene yang dilanjutkan | `Assets/Scenes/FromAshes.unity` |
| Acuan implementasi saat kajian | Working tree `feature/recovery-zone2`, baseline merge `f4ec160` |
| Penanggung jawab | Tim RIMBARA; pemilik tugas ditetapkan saat kickoff |

**Catatan pelaksanaan:** pengguna telah meminta pembangunan berdasarkan PRD ini.
Implementasi lokal selanjutnya dicatat dalam [Zone3-Implementation.md](Zone3-Implementation.md).
Dokumen PRD tetap menjadi acuan kebutuhan, bukan bukti seluruh kriteria sudah lolos.

## 1. Ringkasan produk

Zona 3 memberi pemain kesempatan **melihat dan merasakan hasil kerja sebelumnya**.
Setelah transisi waktu, kawasan bekas kebakaran berubah menjadi hutan hijau,
sungai kembali jernih, habitat dihuni lagi, dan warga beraktivitas di desa.
Tetua Desa mengakui kontribusi relawan dan memberikan misi terakhir:
**Janji untuk Hutan** — tanam satu bibit terakhir, siram, kunjungi titik pandang,
lalu saksikan ending yang memperlihatkan hutan dan desa yang pulih.

**Prinsip utama:** bukan map baru yang tidak berkaitan, bukan pergantian warna
langit saja, dan bukan hutan dewasa yang tumbuh seketika karena satu kali siram.
Landmark serta hasil pekerjaan Zona 1–2 harus tetap dikenali.

## 2. Dasar cerita dan batas interpretasi

Sumber utama: dokumen pengguna `Rimbaraku_Storyline_From_Ashes_to_Life.docx`,
dibaca langsung untuk kajian ini. Rujukan: bagian 6 (Zone Three — The Rebirth),
7 (Mission Flow), 8 (Perubahan Lingkungan), 9 (UI), 10 (Achievement), dan
11–12 (Rekomendasi Unity / Identitas Naratif).

| Kebutuhan sumber | Penerapan PRD |
| --- | --- |
| Langit cerah dan bersih | Hilangkan asap aktif, gunakan pencahayaan dan sky state Hutan Pulih |
| Pohon lebat, tanaman dan bunga | Komposisi hutan dewasa dan vegetasi tanah, tanpa menutup jalur |
| Sungai jernih | Lanjutkan sungai asli 27 m dan dua jembatan Zona 2; tampilkan state akhir |
| Satwa kembali dan warga aktif | Tanda kehidupan yang terlihat/bergerak serta ambience lokal |
| Village Elder / Tetua Desa | Percakapan bertahap sebelum M05 |
| Tanam dan siram pohon terakhir | Satu target final di area pemulihan desa |
| Kunjungi viewpoint dan saksikan transformasi | Tujuan fisik yang memperlihatkan landmark hutan, sungai dan desa |
| Kamera menjauh, musik emosional, narasi penutup | Ending sinematik yang dapat diselesaikan atau dilewati secara sah |
| Area terhubung dan perubahan karena progres | State presentasi pada map asli, tidak menyalin demo vendor |

Transisi waktu eksplisit, pemberian bibit oleh Tetua, bentuk titik pandang,
durasi sinematik, jumlah NPC/satwa, dan tata letak vegetasi adalah **usulan desain
PRD**, bukan detail yang diwajibkan dokumen sumber. Daftar achievement ada dalam
dokumen, tetapi integrasi platform dan penyimpanan permanennya tidak ditetapkan.
Brand teknis tetap RIMBARA; penggunaan nama RIMBARAKU di ending menunggu keputusan tim.

## 3. Tujuan dan indikator keberhasilan

### Tujuan

1. Memberikan perubahan visual yang jelas dibanding Zona 1 dan Zona 2.
2. Menyelesaikan urutan M01–M05 tanpa kehilangan progres atau salah memicu ending.
3. Menghubungkan kontribusi relawan dengan pulihnya hutan, air, habitat dan desa.
4. Menawarkan penutup yang tenang, terbaca, dan dapat diakses tanpa tantangan baru.
5. Menjaga map asli, source scene tim/vendor, referensi Unity dan workflow kolaborasi.

### Indikator penerimaan produk

- Dalam review screenshot sudut kamera yang sama, tim dapat membedakan ketiga
  state lingkungan tanpa bergantung pada judul HUD atau penjelasan developer.
- Playtester dapat mengikuti M05 dari briefing hingga ending memakai petunjuk
  game, tanpa console command, teleport, atau bantuan developer.
- Tidak ada jalur sah untuk mendapat hasil final tanpa menanam, menyiram, dan
  mengonfirmasi titik pandang; pengulangan interaksi tidak memberi progres ganda.
- Seluruh tes regresi yang masih relevan lolos; laporan tes baru mencatat build,
  scene, baseline commit/working tree dan batas cakupannya.
- Layar final tidak lagi menyebut Zona 3 sedang dikembangkan, tetapi tidak
  mengklaim fitur prologue/cinematic/achievement yang belum dibuat sudah tersedia.

## 4. Kondisi awal dan dependensi

### Sudah tersedia pada working tree lokal

- M01 pemadaman dan M02 pembersihan; penyelesaian Zona 1 mengaktifkan Recovery.
- M03: 4 bibit diambil, digali/ditanam, dan disiram satu per satu.
- M04: 10 titik sampah sungai dan 2 habitat; sungai asli dengan 2 jembatan,
  shader riak, audio aliran dan perubahan tepian berdasarkan progres.
- Model progres chapter, interaksi E, held-mouse spraying, dialog bertahap,
  HUD, pause, replay dan scene authoring/verifikasi.
- Verifikasi terakhir Zona 1–2: **77 EditMode / 181 runtime checks**, build berhasil.
  Ini baseline regresi, **bukan** bukti Zona 3 sudah berjalan.

### Belum tersedia

- Chapter Hutan Pulih, bibit/target final, Tetua Desa, viewpoint, transisi waktu,
  komposisi hutan matang/desa aktif, ending cinematic, achievement storyline.
- Opening cinematic dan villager escort masih pekerjaan terpisah yang belum selesai.
- Save progres lintas peluncuran belum ada; hanya master volume yang persist.
- Zona 2 dan river upgrade belum di-commit/push/check-in pada saat PRD disusun.

### Gerbang sebelum implementasi

1. Review PRD dan keputusan pada bagian 15.
2. Simpan/publikasikan milestone Zona 2 secara sengaja bila diminta; pastikan
   workspace tim mempunyai source, `.meta`, dan object LFS yang sama.
3. Verifikasi rute dan stabilitas baseline sebelum menambah collision vegetasi.
4. Tentukan satu pemilik scene/terrain, gunakan task branch dan koordinasi lock.
   Jangan berpindah workspace atau update menimpa pekerjaan lokal yang belum disimpan.

## 5. Scope dan prioritas

| Prioritas | Fitur | Definisi selesai |
| --- | --- | --- |
| P0 | Transisi Zona 2 → Zona 3 | M04 + debrief selesai; tidak reset/reload ke menu; waktu cerita dijelaskan |
| P0 | Transformasi lingkungan | Hutan, tanah, langit, sungai, habitat dan desa berubah nyata |
| P0 | Tetua Desa | NPC beridentitas jelas, dialog pembuka bertahap, pengarahan M05 |
| P0 | Bibit final terpisah | Satu bibit tersedia khusus M05; tidak pernah dikonsumsi M03 |
| P0 | Tanam dan siram final | Range, target dan pause gates; bibit final tetap muda |
| P0 | Titik pandang | Rute fisik aman; konfirmasi setelah penyiraman; pemandangan representatif |
| P0 | Ending cinematic | Kamera, teks Indonesia dan musik; skip aman; lalu hasil final |
| P0 | HUD/reset/regresi | Tujuan, status M05 dan replay konsisten di semua state |
| P0 | Bukti perubahan visual | Capture tiga fase dari sudut yang sebanding + review manusia |
| P1 | Achievement lokal | Reward per misi dan final, idempotent; persistence diputuskan terpisah |
| P1 | Polish NPC/satwa | Model/animasi lebih khas, variasi ambient path dan audio |
| P1 | Viewpoint alternatif | Sudut panorama tambahan bila biaya/hasil playtest mendukung |

### Di luar scope milestone Zona 3

Multiplayer gameplay, save/load penuh, cloud saves, achievement Steam/platform,
AI ekologi kompleks, kawanan satwa dinamis, combat/damage, api menyebar,
crafting, refill air, berenang, fluid physics, siklus musim/cuaca, dan
penggantian seluruh environment library. Voice-over tidak wajib; subtitle wajib.
Opening cinematic/escort tidak otomatis dianggap selesai karena M05 sudah selesai.

## 6. Alur pengalaman pemain

### A. Selesaikan Pemulihan

Pemain menyelesaikan M04 dan membaca debrief Petugas Hutan. Keempat bibit sudah
disiram, 10 sampah hilang, dan 2 habitat pulih. Bukan lagi layar akhir dua zona:
Petugas menjelaskan bahwa pemulihan berlangsung melalui perawatan dan waktu.

### B. Transisi waktu

- Fade pendek, teks **“Waktu berlalu. Hutan dirawat, musim demi musim.”**
- Di balik fade, apply state Restored secara atomik; tidak memainkan pertumbuhan
  dewasa dalam beberapa detik di depan pemain.
- Jika dibutuhkan tanggal yang spesifik, usulan **“Beberapa tahun kemudian”**
  harus direview tim; jangan menyebut semua ekosistem nyata pulih penuh dalam
  periode pasti atau hanya karena empat bibit.
- Pemain kembali pada lokasi aman yang sama setelah transisi; jika collision
  baru berpotensi mengenai pemain, gunakan safe anchor terdekat dengan fade,
  bukan memindahkan ke area asing tanpa alasan.
- Narasi pendek menandai relawan kembali melihat hasil pemulihan.

### C. Hutan Bangkit — Tetua Desa

Petunjuk mengarahkan pemain kembali ke desa. Tetua berdiri di area komunal
yang pulih, dengan warga melakukan aktivitas sederhana di sekitarnya.
Dialog merujuk pada api/abu dahulu dan kehidupan yang kembali sekarang.
Setelah seluruh briefing dibaca, Tetua memberikan **satu bibit final**.

### D. M05 — Janji untuk Hutan

1. Datangi satu lahan tanam final di area pemulihan desa.
2. Tekan **E** untuk menanam bibit final pada lahan yang sudah siap.
   Menggali dua tahap seperti M03 boleh sebagai polish; bukan kebutuhan wajib M05.
3. Bidik bibit lalu **tahan klik kiri** sampai progres siram selesai.
4. Bibit terakhir tetap berupa pohon muda; hutan dewasa di sekelilingnya berasal
   dari transisi waktu dan usaha bersama, bukan pohon final itu sendiri.
5. Ikuti petunjuk menuju titik pandang dan tekan **E — Saksikan hutan pulih**.
   Mengunjungi viewpoint lebih awal boleh, tetapi tidak memicu ending.

### E. Ending dan hasil final

Kamera memperlihatkan bibit/pemain, hutan, sungai dan desa; bergerak menjauh
sesuai sumber. Musik lebih menenangkan dan narasi Indonesia tampil bertahap.
Setelah cinematic selesai atau skip dikonfirmasi, game membuka hasil final.
Tersedia **MAIN LAGI**, **MENU**, dan **KELUAR**. Replay memulai ulang seluruh
perjalanan; tidak membawa hutan hijau ke awal Zona Bencana.

## 7. Kebutuhan fungsional dan penerimaan

| ID | Requirement P0 | Kriteria penerimaan |
| --- | --- | --- |
| Z3-01 | Gerbang Zona 3 | Tidak aktif sebelum Recovery `Complete` dari debrief M04; pemanggilan ulang tidak mengulang transisi |
| Z3-02 | Progres lintas chapter | Api tetap padam, puing/sampah tidak kembali, 4 bibit dan 2 habitat tetap tercatat |
| Z3-03 | Transisi waktu | Teks waktu terbaca; transformasi saat tertutup fade; tidak menghapus data chapter |
| Z3-04 | Lingkungan Restored | Acceptance visual pada bagian 8 terpenuhi, bukan sky tint saja |
| Z3-05 | Tetua dan dialog | Identitas Tetua jelas, E dalam jangkauan; Escape menutup tanpa menerima briefing sebagian |
| Z3-06 | Supply final | Briefing memberi 1 bibit khusus sekali; menutup/membuka ulang tidak menggandakan inventory |
| Z3-07 | Penanaman final | Target terdaftar, briefing selesai, inventory 1 dan jarak sah; transaksi mengonsumsi tepat 1 bibit |
| Z3-08 | Penyiraman final | Tanam lebih dahulu; target raycast benar, range/nozzle obstruction/pause gate; progres selesai sekali |
| Z3-09 | Viewpoint | Akses jalan aman; E hanya diterima setelah siram; tidak dapat selesai dari jauh atau melalui trigger saja |
| Z3-10 | Cinematic | Baru dimulai setelah viewpoint sah; movement/interaksi/spray terkunci; kamera game tidak melawan kamera cinematic |
| Z3-11 | Skip/pause | Escape membuka jeda cinematic; pilihan lanjutkan atau lewati; skip hanya boleh ketika cinematic sudah sah dimulai |
| Z3-12 | Hasil final | Hanya muncul sesudah cinematic complete/skip; identitas cerita dan ringkasan progres benar |
| Z3-13 | Replay/menu | Reset seluruh chapter, final inventory, watering, kamera, material/vegetasi/audio, dialog dan flags cinematic |
| Z3-14 | Bahasa/fit UI | Semua teks pemain Indonesia; font, button dan subtitle tidak overflow pada resolusi uji |
| Z3-15 | Idempotensi | E berulang, double click, re-entry, resume dan repeated callback tidak menggandakan progres/reward |

**Default tuning usulan:** satu bibit final, satu bed, satu viewpoint,
2 detik kontak siram mengikuti Zona 2. Durasi tidak ditetapkan sumber dan dapat
dituning tanpa mengubah gate misi.

## 8. PRD lingkungan — perubahan wajib yang terlihat

Semua perubahan dilakukan pada **map asli FromAshes** menggunakan model/material
EmaceArt individual dan asset turunan project. Tidak membuka atau menyalin map
demo; tidak overwrite scene Main/Arunika/VillageRestoration atau source vendor.

| Elemen | Zona 2 sekarang | Target Zona 3 | Bukti review |
| --- | --- | --- | --- |
| Langit/asap | Lebih terang, haze masih ada | Langit bersih, smoke emission nol dan sisa particle selesai/bersih | Capture horizon dari jalur masuk |
| Hutan | Banyak pohon hangus, 4 pohon muda | Dominan pohon hijau berukuran dewasa dan understory; sedikit bekas hangus sebagai memori | Panorama hutan dan perbandingan bed lama |
| Tanah | Mayoritas abu/gersang | Patch tanah pulih, grass/ground cover dan bunga; jalan tetap terbaca | Jalur kaki dan clearings |
| Sungai | Lebih bersih setelah 10 sampah | Jernih/berwarna alami, riak halus dan tepi hijau; bentuk alur/jembatan dipertahankan | Capture sebelum/sesudah dari sudut sama |
| Habitat/satwa | Dua tanda burung statis | Beberapa satwa terlihat dan gerakan ambient sederhana, tanpa tuntutan AI ekologi | Capture/rekaman gerakan di lokasi habitat |
| Desa | Posko darurat, beberapa placeholder | Warga beraktivitas: berjalan pendek, bekerja di meja/kebun atau berkumpul; tanaman desa pulih | Panorama desa, bukan hanya forest |
| Bed final | Belum ada | Lahan terawat dekat desa dan satu bibit muda setelah ditanam | Close-up final tree |
| Audio | Aliran air dan bird ambience ringan | Api aktif hening, alam/desa terdengar; musik ending mengikuti master volume | Pemeriksaan dengar + mode pause |

### Aturan komposisi dan navigation

- Hutan dewasa muncul hanya setelah transisi; tidak terlihat dari spawn Zona 1.
- Pohon muda M03 boleh memiliki visual dewasa padanan pada posisi terdaftar;
  state penanaman/penyiraman lama tetap benar, tanpa menghasilkan target E baru.
- Tambahkan vegetasi dengan ruang kosong untuk motor, kamera, jembatan dan
  pendekatan semua objective. Collision baru tidak boleh dinonaktifkan hanya
  untuk membuat tes rute lulus. Foliage dekoratif boleh non-solid secara desain.
- Penduduk tidak memakai collider yang membuat jalan atau final bed terblokir.
- Point of interest tetap saling terhubung: **Tetua → final bed → viewpoint**.
- Usulan viewpoint: tonjolan/titik pandang rendah di sisi yang memungkinkan
  kamera menangkap hutan, sungai dan desa. Koordinat final ditentukan lewat
  blockout, bukan angka tetap tanpa review terrain/camera.
- Papan permanen menempel pada papan/tiang. Hanya objective beacon yang melayang.
- Jangan memenuhi seluruh terrain dengan prefab; pilih komposisi yang terlihat
  dari jalur dan panorama, dengan batas render/collision yang terukur.

## 9. Dialog, UI, narasi dan cinematic

### Tetua Desa — draft adaptasi sumber

**Halaman 1:**
> Lihat apa yang telah kita capai. Dulu, hutan ini dipenuhi api dan abu.
> Desa kami diliputi ketakutan, dan kami khawatir akan kehilangan rumah ini.

**Halaman 2:**
> Kini pepohonan tumbuh kembali, burung-burung kembali, dan desa kami hidup lagi.
> Terima kasih. Kamu tidak hanya menyelamatkan hutan, tetapi membantu
> menghidupkan kembali rumah kami.

**Halaman 3 — usulan pengarahan gameplay:**
> Bawalah bibit ini ke lahan pemulihan desa. Tanam dan siram sebagai janji
> untuk terus menjaga hutan. Setelah itu, datangi titik pandang dan lihat
> apa yang telah kita pulihkan bersama.

### Teks pemain

| Situasi | Teks usulan |
| --- | --- |
| Memasuki Zona 3 | KEHIDUPAN KEMBALI KE HUTAN |
| Header misi | ZONA HUTAN PULIH / MISI 05 / 05 |
| Nama misi | JANJI UNTUK HUTAN |
| Setelah briefing | BIBIT TERAKHIR / DIBAWA 1 |
| Bed siap | [E] Tanam bibit terakhir |
| Setelah tanam | Siram bibit terakhir sebelum menuju titik pandang. |
| Siram | TAHAN KLIK KIRI / Siram bibit terakhir |
| Viewpoint belum sah | Tanam dan siram bibit terakhir terlebih dahulu. |
| Viewpoint sah | [E] Saksikan hutan pulih |
| Jeda cinematic | LANJUTKAN / LEWATI SINEMATIK / MENU |
| Identitas ending | DARI ABU MENUJU KEHIDUPAN — Setiap Pohon Menyimpan Cerita. |
| Penutup | TERIMA KASIH, RELAWAN. |

### Draft urutan ending

Durasi usulan **30–45 detik**, subtitle beberapa halaman (bukan satu paragraf
panjang), musik original/licensed yang haknya jelas. Tanpa voice-over pun sah.

| Shot | Tujuan visual | Draft narasi |
| --- | --- | --- |
| 1 — Relawan dan bibit | Dekat pemain/final sapling, menandai janji masa depan | Dulu, tempat ini diliputi api. Hutan terdiam, satwa menjauh, dan desa hidup dalam ketakutan. |
| 2 — Sungai dan habitat | Gerak tenang mengikuti sungai, tepian dan satwa | Melalui kepedulian dan banyak langkah kecil, kehidupan menemukan jalan untuk kembali. |
| 3 — Hutan dan desa | Kamera mundur/naik; desa dan hutan harus terlihat bersama | Pepohonan tumbuh, sungai kembali jernih, dan desa menemukan harapan. Kamu datang sebagai relawan, dan kini menjadi bagian dari cerita hutan ini. |
| 4 — Identitas/penutup | Fade/title, kemudian layar hasil | Menyelamatkan hutan bukan sekadar menanam pohon. Ini tentang melindungi kehidupan dan memberi alam kesempatan untuk memulai lagi. |

Semua draft copy perlu review keterbacaan dan kesetiaan cerita. Shot tidak boleh
menembus terrain/rumah/pohon; objek yang disebut narasi harus terlihat pada hasil
yang memang dibuat. Skip menuju state hasil yang sama, bukan melewatkan misi.

## 10. Rancangan state dan integrasi teknis

Nama berikut adalah **usulan teknis**, bukan file yang sudah ada.

```text
Dormant
  -- M04 dan debrief Recovery selesai --> TimeTransition
  -- fade + apply Restored selesai ----> ElderBriefing
  -- briefing lengkap + supply 1 ------> FinalPlanting
  -- tanam target final sah -----------> FinalWatering
  -- kontak siram final lengkap -------> Viewpoint
  -- E di titik pandang sah -----------> Ending
  -- cinematic selesai / skip sah -----> Complete
```

### Progres/domain

- `RestoredProgress`: state machine murni, target IDs final bed/viewpoint,
  bool supply granted/carried/planted/watered, cinematic started/completed.
- Satu sumber kebenaran; HUD hanya membaca. Semua transition mengembalikan
  berhasil/gagal, menguji state, session permission, registered IDs dan duplikasi.
- Final inventory berdiri sendiri. Jangan memperbesar `RecoveryProgress` dari
  4 bibit menjadi 5: constructor sekarang mengikat supply M03 tepat per bed.
- Completion akhir membutuhkan M01–M04 lengkap + M05 planted/watered/viewpoint
  dan ending. Persentase chapter tidak boleh memicu final victory sendiri.

### Adapter scene dan interaksi

- `RestoredChapter`: mengikat Recovery, Tetua, final bed, viewpoint dan cinematic.
- `ElderDialogue`, `FinalPlantingPoint`, `RestoredViewpoint`: pakai pola
  `InteractableBehaviour`, layer 10, range dan obstruction sama seperti sekarang.
- Target siram final memakai layer 11 dan kontrak target air yang jelas. Ekstrak
  bagian reusable dari `WaterSprayer` bila diperlukan tanpa merusak pemadaman
  `FirePoint` atau `RecoveryPlantingPoint`.
- `GameFlow.CompleteRecoveryChapter()` saat ini langsung membuka Victory:
  ubah menjadi routing ke Restored apabila chapter tersedia; standalone milestone
  tanpa Restored masih boleh memakai hasil dua zona untuk pengembangan.
- Pisahkan identitas hasil chapter dari hasil final; `DisasterHud.RefreshMode()`
  sekarang menulis teks dua zona dan harus tidak menimpa hasil final M05.

### Pemilik presentasi dunia

`RecoveryEnvironment.LateUpdate()` saat ini terus mengatur fog, sun dan material
air; `RecoveryRiver` juga mengatur property block air/vegetasi. Jika hanya menambah
script Zona 3, keduanya bisa mengembalikan state Zona 2 setiap frame.

Requirement: tetapkan **satu pemilik state presentasi aktif** atau gate writer
chapter lama ketika Restored mengontrol dunia. Sky, fog, lights, water, emission,
vegetation dan ambience diterapkan satu kali/perubahan state, dengan restoration
replay yang jelas. Material instance/property block dibersihkan saat reload;
jangan mengganti material vendor bersama secara global.

### Kamera, input, pause

- `EndingDirector` mengelola Cinemachine camera/sequence dan subtitle.
- Tambahkan mode transisi/cinematic eksplisit pada `GameFlow`, atau izin input
  ekuivalen yang terdokumentasi; jangan memakai `Playing` untuk cinematic sambil
  membiarkan gameplay action aktif.
- Gameplay clock tidak menghitung pause/cinematic jika hasil menampilkan waktu
  bermain; gunakan clock cinematic terpisah yang dapat pause/resume.
- Fade dan cinematic memakai timing yang konsisten saat `Time.timeScale` berubah.
  Resume harus kembali ke mode sebelum pause, bukan selalu ke Playing.
- Fokus window hilang → jeda aman; menu/replay/quit membatalkan coroutine,
  melepas camera override dan input lock. Hindari callback akhir pada scene lama.
- Fallback: konfigurasi kamera/clip wajib divalidasi di Editor. Runtime failure
  tidak meninggalkan layar hitam atau state terkunci; tawarkan jalur lanjut/skip
  hanya jika gate M05 telah sah dicapai, dan catat masalah diagnostik.

### Authoring/preservasi

- Root/state visual baru di FromAshes; authoring satu kali dengan backup terarah.
  Untuk scene yang sudah dibuat, edit biasa atau migration terarah yang idempotent.
- Tidak menjalankan `create`, `author-map`, atau `river` pada map existing.
- Pertahankan IDs, `.meta`, reference, terrain/river geometry kecuali ada perubahan
  spesifik hasil blockout yang disetujui. Model matang adalah visual state, bukan
  overwrite permanent terhadap scene sumber.
- Usulan folder: `Assets/Settings/RestoredMap`, `Assets/Prefabs/Storyline`,
  `Docs/Evidence/Restored`, dan `Docs/Captures/Restored`.

## 11. Non-functional requirements

- Tidak ada exception/error runtime pada alur normal, skip, pause, reload dan replay.
- Target review awal: 1600×900 seperti baseline; cek UI pada 1280×720 dan 1920×1080.
  Button, subtitle dan mission text tidak terpotong; mengubah resolusi bukan tes
  kinerja lintas hardware.
- Target performa usulan: 60 FPS pada mesin baseline RX 6600 dalam jalur normal;
  lakukan pengukuran stabil dengan setting/capture overhead disebutkan. Angka
  laporan smoke test lama bukan sertifikasi. Investigasi penurunan relatif
  baseline sebelum menambah dekorasi lebih banyak.
- Scene reload berulang tidak meninggalkan material instance, audio source,
  camera override atau subscription event dari session sebelumnya.
- Semua source binary baru memiliki metadata dan kebijakan Git LFS yang sesuai;
  cache, builds, credentials, `.plastic`, `.git` dan Backups tidak dibagikan.
- Notice lisensi tetap utuh; tidak membeli/download library atau mempublikasikan
  raw asset/repository tanpa izin terpisah.

## 12. Rencana implementasi dan deliverable

| Tahap | Pekerjaan | Deliverable / exit gate |
| --- | --- | --- |
| 0 — Baseline dan blockout | Review PRD, snapshot Zona 2, tandai Tetua/bed/viewpoint, rute dan framing | Keputusan desain, layout blockout, tidak ada area inaccessible |
| 1 — Progres dan transisi | RestoredProgress, final inventory, routing debrief, mode transition/cinematic | Tes gates/duplikasi/reset; tidak final victory saat M04 |
| 2 — Dunia Restored | Visual mature variants, soil/vegetation, clear sky/river, desa/satwa; presentation ownership | Perbandingan 3 fase dari sudut sama, review tim |
| 3 — M05 interaktif | Tetua, supply, final planting/watering, viewpoint | Jalur M05 dapat dimainkan fisik; tidak bisa bypass |
| 4 — Ending | Shots, subtitle, musik, pause/skip, hasil final | Cinematic normal/skip/resume berhasil, kamera tidak clip |
| 5 — Regresi dan polish | Runtime semua chapter, visual review, human playtest, performance/reload | Bukti baru dan checklist bagian 13–14 lengkap |
| 6 — Publikasi terkontrol | Review diff/licence/LFS, check-in/commit dan push jika diminta | Milestone jelas di UVCS/GitHub, tanpa merge otomatis |

Dependensi: 0 → 1 → 3 → 4; tahap 2 mengikuti 1 dan harus selesai sebelum final
review 5. Tidak ada janji tanggal selesai sebelum tim/asset/kamera blockout jelas.
Rincian ownership disepakati: gameplay/state, map/presentation, UI/narrative,
camera/audio, dan reviewer/tester. Satu pemilik memegang scene utama pada satu waktu.

## 13. Strategi pengujian

### EditMode/domain

- Tidak boleh memulai Restored sebelum Recovery lengkap/debrief sah.
- Grant supply sekali; bed ID salah, tanpa bibit, atau sebelum briefing ditolak.
- Water sebelum plant/duplikasi/invalid delta/pause ditolak.
- Viewpoint sebelum plant/water, ID salah dan out-of-range tidak menyelesaikan.
- Ending start/complete/skip mematuhi state; repeated callback idempotent.
- Reset semua flags/inventory; chapter lama tetap tidak berubah karena M05.
- Achievement (bila P1 masuk) tidak diberikan dari counter saja atau dua kali.

### Scene/language verification

- Tepat satu Tetua, bibit supply final, bed dan viewpoint terdaftar; reference
  kamera/sequence/clip/subtitle lengkap; tidak ada missing scripts/GUID.
- Restored vegetation inactive sebelum transisi; river/bridge source preserved.
- Semua teks Indonesia dan UI sizing terdaftar; nama file/hierarchy teknis stabil.
- Authoring menolak regenerasi destructive; build tidak membutuhkan folder Backups.

### Standalone runtime

- Jalankan M01–M04 lewat input yang sudah diuji, lalu masuk Zona 3 tanpa reset.
- Uji datang ke Tetua/viewpoint lebih awal, dismiss dialog, pause dan resume.
- Jalan Tetua → final bed → viewpoint **tanpa teleport** sebagai acceptance rute M05.
- Tanam dan siram menggunakan E/mouse sungguhan; uji jarak serta nozzle obstruction.
- Uji cinematic normal, skip, focus loss, menu dan replay. Fixtures terisolasi
  boleh teleport untuk negative case, tetapi tidak disebut sebagai coverage rute penuh.
- Smoke normal tidak aktif tanpa opt-in; tidak mengisi misi otomatis pada game biasa.
- Perbarui assertion hasil dua zona yang sekarang mengharapkan Victory setelah
  M04. Jumlah total tes harus berasal dari run aktual, bukan target buatan PRD.

### Review visual/manusia

- Capture perbandingan Zona 1/2/3 dari jalur desa, bed M03, sungai, dan viewpoint.
- Review ending sebagai video/urutan frame: terrain/camera clipping, subtitle
  pace, landmark yang terlihat, musik/master volume, dan transisi waktu.
- Human tester menjalankan seluruh perjalanan, termasuk menyeberang jembatan,
  mengecek collision vegetasi baru dan memahami bahwa pohon final masih muda.
- Simpan laporan, screenshots, setting dan limitations dalam bukti Restored.

## 14. Definition of Done

- [ ] Seluruh requirement Z3-01–Z3-15 terpenuhi.
- [ ] Perubahan hutan **dan desa** terlihat jelas; sungai dan habitat konsisten.
- [ ] Waktu naratif jelas; final tree tidak menjadi pohon dewasa seketika.
- [ ] M01–M05 sah sebelum final result, termasuk watering dan viewpoint.
- [ ] Cinematic + skip + pause + focus loss + replay diuji.
- [ ] Semua tes regresi/baru yang relevan lolos; build Windows sukses.
- [ ] Rute M05 fisik serta human playtest lengkap direview.
- [ ] Bukti visual dan coverage limits disimpan, tidak memakai laporan usang.
- [ ] Original team/vendor scenes dan referensi terjaga.
- [ ] Final UI tidak mencampur chapter completion dengan whole-story victory.
- [ ] Pekerjaan prologue/achievement yang belum masuk tetap dinyatakan belum ada.
- [ ] Diff/asset rights/LFS diverifikasi sebelum publikasi yang diotorisasi.

## 15. Keputusan terbuka dan risiko

### Keputusan review tim (default usulan, belum final)

| Keputusan | Default usulan |
| --- | --- |
| Struktur map | Scene sama + Restored presentation state, bukan map demo/peta terpisah |
| Waktu | Teks umum perawatan bertahun/musim; periode spesifik direview sebelum produksi |
| Bibit final | Diberikan Tetua setelah briefing, inventory terpisah |
| Viewpoint | Satu titik rendah/aman yang memperlihatkan hutan–sungai–desa |
| Cinematic | 30–45 detik, subtitle + musik, tanpa voice-over wajib |
| Satwa/warga | Ambient motion sederhana; asset existing bila sesuai dan haknya jelas |
| Achievement | P1 lokal session-only dahulu; persistence/platform di luar P0 |
| Branding | RIMBARA teknis; konfirmasi RIMBARAKU sebelum title final |

### Risiko dan mitigasi

- **Perubahan visual terlalu kecil:** wajib blockout Restored dan capture tiga
  fase sebelum menambah mekanik detail. Sky tint saja tidak lulus.
- **Writer Zona 2 menimpa Zona 3:** presentation ownership/gating wajib sejak tahap 1.
- **Final supply habis di M03:** gunakan inventory dedicated dan registered final ID.
- **Ending bisa dibypass:** state gates sampai cinematic completion, bukan progress 100%.
- **Pertumbuhan tidak masuk akal:** explicit time transition, final sapling tetap muda.
- **Vegetasi menutup jalan/kamera:** reserved corridors dan rute fisik diuji dengan collision.
- **Cinematic softlock/pause salah:** mode/clock terpisah, cancel on reload, skip sah dan uji fokus.
- **Asset/story identity tidak sesuai:** review model Tetua/satwa, terjemahan dan branding;
  placeholder yang belum diganti tidak disamarkan sebagai karakter final.
- **Performa menurun:** batasi dekorasi, gunakan visual yang relevan panorama,
  ukur sebelum/after; tidak menambah sistem AI besar tanpa keputusan baru.
- **Snapshot tim berbeda:** checkpoint Zona 2, single source of truth UVCS,
  mirror GitHub manual; tidak menimpa perubahan rekan atau auto-push/merge.

## 16. Dokumen terkait

- [Storyline-Plan.md](Storyline-Plan.md) — kontrak lima misi.
- [Zone2-Concept.md](Zone2-Concept.md) — kebutuhan sumber Zona 2.
- [Zone2-Implementation.md](Zone2-Implementation.md) — implementasi termasuk sungai/10 sampah.
- [Bahasa-Game.md](Bahasa-Game.md) — Bahasa Indonesia dan identitas naratif.
- [Slavic-Base.md](Slavic-Base.md) — resource library, bukan template layout.
- [Unity-Version-Control.md](Unity-Version-Control.md) — kolaborasi dan mirror GitHub.

**Dokumen ini adalah rencana pembangunan Zona 3, bukan laporan fitur yang sudah
dibuat. Implementasi, pembelian asset, check-in, commit, push dan merge tidak
dijalankan sebagai bagian dari penyusunan PRD.**

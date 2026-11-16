# Zona 2 — Konsep Pemulihan

Status: **kajian konsep sebelum implementasi**, 16 November 2026.
Implementasi berikutnya dicatat terpisah dalam [Zone2-Implementation.md](Zone2-Implementation.md);
dokumen ini tetap membedakan kebutuhan sumber dari usulan desain.
Sumber utama: `Rimbaraku_Storyline_From_Ashes_to_Life.docx` yang dikirim tim,
bagian 5 (Zone Two — The Recovery), 7 (Mission Flow), 8 (Perubahan Lingkungan),
dan 11 (Rekomendasi Implementasi). Bagian 4 dan 6 menentukan batas transisi.
Dokumen asli di Downloads sudah dibaca langsung, bukan hanya ringkasan repo.

## 1. Tujuan cerita

Zona 1 menyelamatkan kawasan dari api dan membersihkan lahan. Zona 2 memulai
pemulihan: **hutan adalah rumah bagi makhluk hidup, bukan sekadar kumpulan pohon**.
Pemain menanam, merawat bibit, dan memperbaiki air serta habitat.

Zona 2 bukan hutan matang yang langsung hijau. Asap berkurang dan langit mulai
cerah; area bersih terlihat, tetapi banyak pohon masih mati dan tanah belum pulih.
Pesan sumber: pemulihan membutuhkan kesabaran, keberanian, dan kepedulian.

## 2. Kebutuhan dari dokumen sumber

| Bagian | Kebutuhan |
| --- | --- |
| Pembuka | Bertemu **Forest Ranger / Petugas Hutan**, yang menjelaskan pentingnya pemulihan kehidupan |
| M03 — Tanam Masa Depan | Ambil bibit di persemaian, cari lokasi tanam bertanda, gali lubang, tanam bibit, siram pohon yang baru ditanam |
| Selesai M03 — Benih Harapan | Narasi: setiap bibit adalah janji bagi masa depan; rawat dan beri waktu untuk tumbuh |
| M04 — Kembalikan Kehidupan | Bersihkan sumber air terdekat, singkirkan sampah tersisa, pulihkan habitat alami yang rusak |
| Opsional M04 | Sediakan makanan/air pada lokasi satwa **jika gameplay mendukung** |
| Akhir Zona 2 | Pohon muda tumbuh, air lebih bersih, tanda kehidupan satwa mulai muncul; bukan akhir seluruh cerita |

Dokumen **tidak menetapkan jumlah bibit, titik air, sampah, habitat, durasi menyiram,
atau spesies pohon/satwa**. Semua angka/spesies nanti merupakan keputusan desain
dan tuning, bukan ketentuan cerita. Tidak ada keharusan AI satwa, lapar/haus,
crafting, refill air, damage, timer, atau simulasi ekologis.

## 3. Usulan alur gameplay

Usulan berikut menerjemahkan kebutuhan cerita menjadi interaksi; belum diterapkan.

1. Setelah M02, koordinator mengarahkan pemain menuju Petugas Hutan. Api tetap
   padam dan puing yang sudah dibersihkan tidak muncul kembali.
2. Dialog Petugas Hutan dibaca bertahap; M03 aktif setelah briefing selesai.
3. **E** di persemaian mengambil bibit. Inventory menjelaskan bibit yang dibawa.
4. **E** pada tanah bertanda menyiapkan/menggali lubang; interaksi berikutnya
   menanam bibit dan mengonsumsi satu bibit. Efek sederhana cukup untuk versi
   awal; jangan menyatakan ada animasi sekop khusus bila belum dibuat.
5. Bidik bibit yang sudah ditanam dan **tahan klik kiri** untuk menyiram.
   Progres siram dicatat **per pohon**, bukan satu counter global tanpa target.
6. M03 selesai hanya jika semua target sudah ditanam **dan disiram**. Tampilkan
   narasi Benih Harapan, kemudian briefing M04.
7. M04 mencakup sampah pada sumber air/sekitar kawasan dan target habitat yang
   dapat dipulihkan secara konkret. Pengambilan sampah dan perubahan visual air
   harus terkait progres, bukan air langsung bersih saat memasuki zona.
8. Setelah semua target M04 selesai, tampilkan ringkasan pemulihan dan arahan
   berikutnya. Penyelesaian Zona 2 tidak memicu ending M05.

Petugas Hutan harus punya identitas tampilan/dialog sendiri. Model placeholder
boleh digunakan secara eksplisit, tetapi bukan sekadar koordinator bernama lama.

## 4. Usulan map dan perubahan lingkungan

- Lanjutkan **map asli FromAshes** dengan area saling terhubung; jangan buat tiga
  peta terpisah atau menyalin komposisi demo Slavic. Jangan regenerate Zona 1.
- Area utama: persemaian sederhana, lahan tanam yang disiapkan, sumber air
  terdampak abu/sampah, dan habitat kecil yang perlu dipulihkan.
- Lahan bersih Zona 1 dapat menjadi lokasi penanaman bila sesuai jalur/kamera.
  Empat tumpukan puing sebelumnya tidak otomatis berarti empat bibit wajib.
- Gunakan prefab/material EmaceArt secara individual. Pohon, semak, rumput,
  bebatuan, dan modul sungai yang tersedia perlu diperiksa ukuran/collisionnya.
  Air, bibit, atau penanda habitat yang belum sesuai dapat dibuat sebagai asset
  turunan milik project; jangan ubah sumber vendor.
- Awal: pohon hangus tetap terlihat, tanah sebagian besar gersang, air masih
  terdampak, desa masih masa pemulihan.
- Setelah menanam: muncul **bibit/pohon muda**, bukan pohon dewasa.
- Setelah menyiram: indikator perawatan dan pertumbuhan muda yang terbatas;
  tidak ada hutan lebat dalam hitungan detik.
- Setelah pemulihan habitat/air: tanda satwa dan ambience alam perlahan kembali.
  Efek lokal harus mewakili tindakan yang selesai, bukan hanya persentase umum.
- Vegetasi matang, sungai sepenuhnya jernih, dan desa aktif adalah tujuan Zona 3,
  dengan transisi waktu naratif yang masuk akal.

## 5. Contoh teks pemain — Bahasa Indonesia

Pembuka Petugas Hutan, mengikuti terjemahan sumber:

> Kamu sudah melakukan pekerjaan luar biasa. Api sudah padam dan kawasan ini
> bersih. Namun, hutan bukan sekadar kumpulan pohon; hutan adalah rumah bagi banyak
> makhluk hidup. Kita harus mengembalikan kehidupan itu. Siap menanam benih
> pertama pemulihan hutan?

Usulan objective/UI:

- **M03 / TANAM MASA DEPAN** — Ambil bibit di persemaian. Siapkan lahan, tanam,
  lalu siram setiap bibit.
- **[E] Ambil bibit** / **[E] Gali lubang tanam** / **[E] Tanam bibit**.
- **TAHAN KLIK KIRI / Siram bibit**; status **Belum disiram / Sudah disiram**.
- **M04 / KEMBALIKAN KEHIDUPAN** — Bersihkan sumber air dan pulihkan habitat
  yang ditandai.
- **ZONA PEMULIHAN SELESAI** — Upayamu memberi hutan ini kesempatan kedua.

Label permanen persemaian/papan ditempel pada objeknya. Beacon objective boleh
melayang sebagai petunjuk, tetapi jangan menggantikan tulisan fisik pada papan.

## 6. Temuan kode saat ini dan konsekuensinya

- `DisasterHud` mengakhiri bab setelah dialog koordinator pada `RecoveryReady`.
  Perlu transisi ke Pemulihan dengan mempertahankan progres, bukan replay/reset.
- `MissionSystem.CanCollectOrPlant` selalu false pada storyline Disaster.
  Membuka gate saja tidak cukup: perlu status M03/M04 yang terpisah dari urutan
  MVP lama (yang menanam sebelum memadamkan api).
- `PlantingPoint` lama langsung menanam dan membesarkan pohon dalam 1,8 detik;
  belum ada state menggali atau menyiram. Jangan gunakan pertumbuhan dewasa ini
  untuk mengklaim sesuai konsep Zona 2.
- `WaterSprayer` hanya mengenali `FirePoint`; perlu target penyiraman bibit,
  gate misi, range/obstruction, dan progres siram per target.
- `GameState`/progres lama belum menyimpan pohon disiram, air bersih, sampah,
  atau habitat. HUD bukan pemilik state progres.
- `GameAudio` storyline sengaja belum meningkatkan suara burung setelah
  pemadaman. Kembalinya satwa nanti harus terkait pemulihan habitat.
- Inventory/progres M05 harus dapat dipisahkan: jangan menghabiskan bibit terakhir
  yang nanti dibutuhkan untuk misi final. Mekanisme reserve/supply akan ditentukan
  saat desain Zona 3, bukan dianggap sudah ada.

## 7. Kriteria penerimaan implementasi berikutnya

- M03 tidak dapat dimulai sebelum pemadaman dan pembersihan Zona 1 selesai.
- Progres Zona 1 dipertahankan saat memasuki Zona 2.
- Bibit tidak bisa ditanam tanpa inventory atau pada target yang belum disiapkan.
- Menyiram hanya dihitung untuk bibit yang sudah ditanam, dalam jangkauan, tanpa
  penghalang; pause/double interaction tidak memberi progres tambahan.
- Semua target M03 harus disiram sebelum M04 terbuka.
- M04 memeriksa air/sampah/habitat; kegiatan satwa opsional tidak diam-diam wajib.
- Setiap perubahan visual mengikuti tindakan dan tetap menampilkan pemulihan
  bertahap, bukan kondisi Zona 3 sejak awal.
- Selesai Zona 2 bukan akhir game. Replay mengembalikan semua target dengan benar.
- Teks pemain Indonesia, referensi scene/.meta stabil, dan scene asli/vendor utuh.
- Tes model, scene verification, build, runtime serta rute fisik diperbarui untuk
  fitur yang benar-benar diimplementasikan; test Zona 1 bukan bukti Zona 2 lolos.

Urutan kerja yang disarankan: **transisi Zona 1 + state Pemulihan → persemaian,
gali/tanam/siram → sumber air/sampah/habitat → visual/audio → pengujian**.

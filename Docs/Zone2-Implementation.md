# Zona 2 — implementasi pemulihan

**Milestone historis dua zona:** laporan 77/181 di bawah berlaku sebelum Zona 3
ditambahkan. Perubahan tiga zona dan bukti baru dicatat dalam
[Zone3-Implementation.md](Zone3-Implementation.md); ending setelah M04 kini
diganti transisi ke Hutan Pulih saat chapter tersebut tersedia.

Branch kerja: `feature/recovery-zone2`, berdasarkan hasil merge tim `f4ec160`.
Scene: `Assets/Scenes/FromAshes.unity`. Bahasa pemain: Bahasa Indonesia.
Panduan konsep: [Zone2-Concept.md](Zone2-Concept.md).

## Gameplay yang ditambahkan

1. Selesaikan pemadaman/pembersihan Zona 1 dan dialog koordinator. Pemain
   diarahkan menuju Petugas Hutan; tidak kembali ke menu atau reset progress.
2. Petugas Hutan di persemaian menjelaskan pentingnya memulihkan kehidupan.
3. M03 **Tanam Masa Depan**: E mengambil bibit; E menggali target; E lagi menanam
   satu bibit; bidik bibit dan tahan klik kiri untuk menyiram.
4. Semua target harus ditanam dan disiram sebelum dialog **Benih Harapan** membuka
   M04. Inventory/progres Recovery terpisah dari MVP lama.
5. M04 **Kembalikan Kehidupan**: E membersihkan sampah tepi sumber air dan
   memulihkan susunan ranting/tanaman penutup sebagai habitat satwa kecil.
6. Air sungai membaik bertahap selama sampah diambil dan menjadi lebih bersih
   setelah seluruh 10 titik disingkirkan. Tanda
   satwa dan suara burung meningkat hanya setelah air dan semua habitat pulih.
7. Dialog akhir Petugas Hutan membuka hasil **Zona 1 dan Zona 2 selesai**. Zona 3
    pada milestone historis belum tersedia. Snapshot terkini melanjutkan ke Zona 3
    dan akhir lima misi; lihat `Zone3-Implementation.md`.

Tuning saat ini: **4 bibit/lahan tanam, 10 titik sampah sungai, 2 habitat**, penyiraman
**2 detik kontak per bibit**. Jumlah/durasi bukan ketentuan dokumen storyline.
Air tidak perlu refill, tidak ada timer/damage/decay. Progres hanya satu sesi.
Progres menyiram bertambah selama kontak air valid; melepas klik atau berhenti
membidik tidak menghapus kontak yang sudah didapat. Menyiram bibit selesai tidak
memberi poin kedua. Penyiraman terlalu jauh atau terhalang tidak memberi progres.

## Map dan presentasi

Authoring Recovery hanya menambah root/object/komponen baru; tidak meregenerasi
Zona 1, terrain, scene asli tim atau template vendor. Keempat lahan bersih yang
sudah disiapkan dipakai kembali. Persemaian berada di kiri posko; sumber air
berupa sungai kecil berkelok di tengah bagian utara hutan. Lima titik sampah berada
di tiap tepian; setiap titik berisi botol plastik, kantong dan kaleng. Dua jembatan
kayu menghubungkan kedua tepian. Habitat Recovery dipindahkan ke tepi sungai,
tanpa memindahkan bibit atau objective Zona 1.

Model vegetasi, ranting dan batu memakai prefab EmaceArt individual. Pot, papan,
tanah, mesh sungai, sampah dan tanda satwa adalah bentuk/material turunan project.
Sungai memakai alur/mesh asli, shader riak bergerak dan audio aliran prosedural.
Kejernihan meningkat sesuai jumlah sampah diambil; vegetasi tepian aktif setelah
semua 10 titik selesai. Bersihnya air tidak otomatis menyelesaikan kedua habitat.
Pohon yang ditanam berukuran muda (sekitar 1,25 m), tanpa coroutine pertumbuhan
dewasa. Pohon hangus dan tanah gersang tetap ada. Tulisan persemaian menempel
pada papan, sedangkan objective memakai penanda dunia.

Audit sebelum perubahan sungai: **0 object Zona 1 dihapus dan 0
Transform/RectTransform lama dipindahkan**. Perubahan sungai mengganti genangan,
membentuk cekungan terrain lokal, serta memindahkan hanya task/habitat Recovery.
Origin terrain diturunkan 0,5 m dengan kompensasi heightmap agar elevasi dunia
di luar alur sungai tidak berubah. Scene/terrain sebelum penggantian disalin ke
`Backups`. Semua 4 bibit memiliki trigger layer
WaterTarget terpisah yang aktif hanya setelah ditanam dan selama perlu disiram.

**Batas visual yang disengaja:** sungai bergaya stylized dengan riak shader,
bukan simulasi arus, berenang atau fluid physics. Jembatan memiliki collision
deck; air belum memiliki damage/arus yang mendorong karakter. Aksi menggali berupa
interaksi/penampakan tanah, belum animasi
sekop khusus. Pada milestone historis petugas memakai placeholder Ch42; snapshot
terkini memakai model petugas unik berproporsi manusia, tanpa badge lama.
Dua tanda burung statis bukan satwa AI, animasi terbang atau spesies
ekologis yang terverifikasi. Tidak ada opsi memberi makan satwa dalam versi ini.

## File utama

- `Assets/Scripts/Mission/RecoveryProgress.cs`: registered target IDs, inventory,
  digging, planting, watering, ecosystem gates, percentage and reset.
- `Assets/Scripts/Mission/RecoveryChapter.cs`: scene bindings, destination and copy.
- `Assets/Scripts/Interaction/RecoverySeedling.cs`, `RecoveryPlantingPoint.cs`,
  `RecoveryTask.cs`, `RangerDialogue.cs`: physical interactions.
- `WaterSprayer.cs`: target api/bibit, range/nozzle obstruction and held input.
- `DisasterHud.cs`: staged coordinator/ranger dialogue, Recovery HUD and result.
- `RecoveryEnvironment.cs`, `GameAudio.cs`: action-driven water, haze and wildlife.
- `Assets/Editor/RecoveryMapBuilder.cs`: one-time additive authoring and verification.
- `Assets/Editor/RecoveryRiverBuilder.cs`: penggantian genangan, sculpt alur,
  10 target sampah, jembatan dan scene audit.
- `Assets/Scripts/World/RecoveryRiver.cs`, `RecoveryRiverLayout.cs` dan
  `Assets/Art/Shaders/RecoveryRiver.shader`: flow presentation dan layout asli.

`Tools/Invoke-Storyline.ps1 -Command recovery` hanya untuk penambahan pertama.
Scene yang sudah punya Recovery menolak regenerate. Gunakan `verify` / `build`
untuk pekerjaan rutin dan edit scene secara normal.
`-Command river` hanya untuk penggantian genangan pertama; river yang sudah
ditambahkan menolak regenerasi. Jangan menjalankan create/author-map kembali.

## Verifikasi

Upgrade sungai: **77/77 EditMode tests** (56 baseline + 18 Recovery + 3 river),
**181/181 runtime checks**, scene river/supply audit dan Windows build
**0 error / 0 warning build** lolos. Tes runtime lengkap diulang setelah
cekungan diperbaiki; bukti final disimpan di `Docs/Evidence/Recovery`
dan `Docs/Captures/Recovery`. Laporan sebelum perubahan tidak dipakai sebagai
bukti sungai/10 sampah. Bahasa: 53 label UI, 44 penanda dunia, 0 belum diterjemahkan.

Run runtime pertama Zona 2 menemukan raycast bibit tidak termasuk mask WaterTarget.
Trigger penyiraman layer 11 ditambahkan; E tetap memakai trigger layer 10.
Capture persemaian juga menunjukkan palette atlas kit tidak cocok pada cube,
sehingga meja memakai material kayu turunan dan papan mendapat tiang fisik.

Runtime diagnostik melewati Zona 1 terlebih dahulu, lalu menguji pengarahan,
pickup, digging/planting, aim/watering, pause, obstruction, misi ekosistem,
milestone dan replay. Fixture lain masih menggunakan teleport dan pendekatan
fisik lokal; itu bukan bukti playtest manusia atau rute penuh tanpa teleport.
Rute fisik yang diuji tanpa teleport: spawn → posko, posko → api pertama,
dan posko → Petugas Hutan. Scene/map dan kamera tetap memakai collision.
Range, partial watering, obstruction, pause, duplicate IDs/actions, gate M03/M04,
material air, sky change, kembalinya satwa bersyarat, dan replay tercakup.

Visual yang sudah ditinjau: dialog Petugas Hutan, persemaian/papan berkaki,
penyiraman bibit, sumber air, habitat kecil dan layar selesai dua zona. Detail
sumber air/habitat diperiksa melalui kamera close-up khusus diagnostics.

Upgrade sungai juga menguji seluruh 10 sampah dalam satu rute fisik dari
Petugas Hutan: tepi kiri → jembatan utara → tepi kanan, tanpa teleport di antara
sampah. Tiga target lama mempertahankan ID-nya; tujuh target baru ditambahkan.
Semua sepuluh wajib selesai; sembilan saja belum membersihkan sungai.
Terrain-channel audit memeriksa permukaan air tidak tertutup tanah sepanjang alur.
Audit perbaikan cekungan: 139 titik heightmap berubah di alur, perubahan elevasi
di luar alur tercatat **0 m**. Terrain origin berubah dengan kompensasi heightmap;
semua objective lama dan scene sumber tim/vendor tetap utuh.

Pekerjaan ini belum di-commit, push GitHub, merge, atau check-in UVCS otomatis.

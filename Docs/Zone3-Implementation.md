# Zona 3 — Hutan Pulih dan penutup cerita

Status: implementasi tiga zona dan penggantian NPC sudah diuji; publikasi dicatat
di `Docs/Publication-Status.md`.
Branch: `feature/restored-zone3`; seluruh progres Zona 2/river lokal dipertahankan.
Scene: `Assets/Scenes/FromAshes.unity`. Sumber kebutuhan: [Zone3-PRD.md](Zone3-PRD.md).

## Alur

1. Selesaikan M01–M04 dan debrief Petugas Hutan.
2. Fade/transisi **Waktu Berlalu** menjelaskan perawatan musim demi musim.
3. State Hutan Pulih diterapkan tanpa reset progres chapter sebelumnya.
4. Temui **Tetua Desa**, baca dialog lengkap, dapatkan satu bibit final khusus.
5. **E** menanam bibit terakhir di lahan desa; bidik dan tahan klik kiri untuk
   menyiram (2 detik kontak valid). Inventory final terpisah dari 4 bibit M03.
6. Datangi titik pandang dan **E** untuk melihat hasil pemulihan. Kunjungan awal
   tidak memicu ending.
7. Kamera bergerak selama 36 detik, narasi Indonesia dan musik prosedural original.
   Escape menjeda; lanjutkan kembali ke cinematic, bukan mengaktifkan movement.
   Pilihan **LEWATI SINEMATIK** tersedia ketika cinematic yang sah dijeda.
   Transisi waktu berlangsung 6 detik; clock transisi/cinematic terpisah dari
   waktu gameplay dan berhenti saat jeda. Fokus window hilang memakai pause
   `GameFlow`; behavior platform tetap memerlukan review manusia.
8. Hasil akhir mengakui M01–M05; replay/menu reload membersihkan semua state.

## Presentasi

- Pohon hijau dewasa pada lokasi hutan lama dan 4 bed M03, understory, bunga,
  tanah lebih hijau, sky bersih, smoke di-clear, sungai state akhir.
- Variasi visual hanya aktif sesudah transisi. Model/source vendor dan scene tim
  tidak ditulis ulang. Terrain Restored memakai instance runtime; terrain authoring
  Zona 2/river tidak diubah menjadi hijau secara permanen.
- Tajuk/understory tambahan memperjelas pemulihan hutan. Collision lama tetap
  ada; pohon tumbang yang memiliki collision tetap tampak sebagai pengingat
  bencana, bukan penghalang tak terlihat. Dekorasi baru tidak menutup rute M05.
- Pemilik presentasi Restored mengunci writer sky/fog/river Zona 2 agar tidak
  saling menimpa. Bibit final tetap muda setelah disiram.
- Desa mempunyai 5 penduduk decorative ambient motion; 6 tanda burung bergerak
  sederhana. Ini bukan navigation/animal AI atau simulasi ekologi.
- Tetua dan warga kini memakai model human-proportioned MakeHuman/MPFB; overlay
  jubah/cap lama dihapus. Lihat `Human-NPC-Implementation.md`. Frame kamera dan
  full-map collision tetap perlu review manusia.
- Viewpoint berada pada jalur terbuka dekat desa, bukan tebing/tantangan lompat.

## File

- `RestoredProgress.cs`, `RestoredChapter.cs`: registered IDs, gate misi final.
- `FinalPlantingPoint.cs`, `ElderDialogue.cs`, `RestoredViewpoint.cs`: interaksi.
- `RestoredEnvironment.cs`: perubahan presentasi dan terrain runtime.
- `EndingDirector.cs`: fade, cinematic/subtitle/music, pause/skip, kamera cleanup.
- `RestoredAmbientMotion.cs`, `RestoredRoleVisibility.cs`: NPC/ambient sederhana.
- `RestoredMapBuilder.cs`: authoring satu kali dan verifikasi referensi.

`Tools/Invoke-Storyline.ps1 -Command restored` hanya untuk penambahan pertama;
root yang sudah ada menolak regenerate. Edit scene biasa, gunakan verify/build.

## Verifikasi

**Milestone historis sebelum penggantian NPC:** 95/95 EditMode (77 baseline + 18 Restored) lolos.
**207/207 standalone runtime checks** jalur normal sampai ending dan
**206/206 checks** jalur pause + explicit skip lolos pada build terkini.
Windows build berhasil dengan **0 error / 0 warning build**.
Audit bahasa mencatat **55 label UI, 48 penanda dunia, 0 belum diterjemahkan**.
Laporan Zona 1/2 tidak membuktikan Zona 3 selesai. Bukti baru disimpan
di `Docs/Evidence/Restored` dan `Docs/Captures/Restored` dari run final.
**Milestone terkini dengan NPC:** 105/105 EditMode; 238/238 runtime normal;
237/237 runtime skip; 0 runtime errors; build terakhir 0 error / 0 warning.
Evidence terkini ada di `Docs/Evidence/HumanNPCs/`, bukan folder historis Restored.
Audit preservasi terhadap backup pra-Zona 3: tidak ada object sebelumnya
dihapus atau Transform/RectTransform lama dipindahkan. Canvas hanya mendapat
child UI cinematic baru; river/terrain authoring tetap sama seperti Zona 2.

## Batas

Belum menambahkan opening cinematic/escort, voice-over, achievement, full save,
AI NPC kompleks atau pembelian asset. Tujuan milestone adalah akhir lima misi;
itu tidak menyatakan semua polish prologue dan fitur optional sudah tersedia.
Progres tetap per sesi, master volume persist. Human playtest lengkap dan
benchmark lintas hardware belum diklaim dari diagnostic fixtures.

## Acceptance yang masih memerlukan review manusia

- Viewpoint/ending framing sudah punya rute aman dan panorama, tetapi human
  reviewer perlu menilai apakah desa dan sungai terbaca jelas sepanjang cinematic.
- NPC manusia sudah berbeda dan terintegrasi; satwa tetap decorative sederhana,
  bukan bukti navigation/ekologi/AI lengkap.
- Uji UI multi-resolusi, focus-loss platform, performa stabil dan rute seluruh
  perjalanan tanpa fixture teleport belum selesai hanya dari smoke tests.
- Achievement P1 belum dibuat. Semua P0 PRD tidak otomatis dinyatakan tuntas
  hanya karena urutan lima misi mencapai hasil final.

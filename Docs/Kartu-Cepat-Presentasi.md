# Kartu cepat — buka ini saat presentasi

## Satu kalimat

**RIMBARA sudah menjalankan lima misi dalam tiga fase restorasi; fondasi mini-loop
berjalan, polish/playtest full-map dan fitur opsional masih dilanjutkan.**

## Tugas A/B/C (isi nama dan kontribusi nyata sebelum presentasi)

- **A:** player/input/kamera, terrain/layout/collision, foundation/setup.
- **B:** E/detector, pickup, gali/tanam, target/NPC/world interaction.
- **C:** state/event/urutan misi, HUD/dialog, api/air/pause/ending.
- **Integrasi bersama:** satu fitur memakai input A, target B, state/UI C.
- Jangan menyamakan tanggung jawab dengan klaim semua file ditulis sendiri.
  Sebut package/bantuan AI dan kontribusi nyata secara jujur.

## Data progres

**2 api → 4 puing → 4 bibit tanam/siram → 10 sampah sungai + 2 habitat →
1 bibit final + viewpoint → ending.**

- 10 NPC, 8 model manusia; player Ch42 tetap.
- 105/105 EditMode; normal 238/238; skip 237/237; 0 runtime errors.
- Build terakhir 0 error / 0 warning. UI Bahasa Indonesia.
- Belum: opening escort/cinematic, achievement, full save, voice-over, AI kompleks,
  sertifikasi semua resolusi/perangkat dan full-map human playtest.

## Unity: jangan tersesat

1. Unity **6000.3.6f1** → `Assets/Scenes/FromAshes.unity`.
2. Pilih `Volunteer` → Inspector: controller/input/Animator/detector.
3. Play → menu/briefing → WASD/mouse; E dialog; perlengkapan; bidik+tahan klik.
4. Stop Play → cari `Recovery planting bed 1` → `RecoveryPlantingPoint`.
5. Jika ditanya NPC: root = dialog/interaction; child = model/Animator.
6. Jika demo panjang: gunakan bukti gambar/report dan katakan itu hasil pengujian,
   bukan berpura-pura baru menyelesaikan seluruh game secara live.
7. **Jangan regenerate map / Create / Add Recovery / River / Restored.**

## Empat kata kunci untuk semua fitur

**INPUT → VALIDASI → STATE → OUTPUT**

- E: `InteractionDetector.Scan` → jarak/layer/penghalang → target `Interact` →
  `RecoveryProgress` → event `Changed` → HUD/visual.
- Air: `WaterSprayer.UpdateAim` → raycast crosshair/range/linecast/gate →
  `ReceiveWater` → waktu kontak/counter → api padam atau bibit disiram.
- Misi: `GameFlow` + chapter/progress menentukan tahap dan controls.
- Hutan pulih: transisi waktu dahulu, `RestoredEnvironment` mengubah visual.
- NPC: rig Humanoid/retarget, bukan AI kompleks.

## Kalimat penyelamat

> “Sebentar Pak/Bu, saya tunjukkan komponen yang menangani fitur ini supaya
> penjelasannya tepat.”

> “Ini tanggung jawab integrasi kami bersama; detail bagian ini akan dijelaskan
> teman yang mengerjakannya.”

> “Fitur tersebut belum tersedia; progres saat ini per sesi, bukan save/load penuh.”

> “Saya belum dapat memastikan detail itu. Kami catat dan periksa, bukan menebak.”

Pegangan minimal per anggota: **1 fitur, 3 file, 1 validasi, 1 keterbatasan, 1 bukti**.
Panduan lengkap: `Docs/Panduan-Presentasi-Dosen.md`.

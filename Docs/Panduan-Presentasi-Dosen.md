# Panduan presentasi RIMBARA — progres dan demo Unity

Versi ringkas satu halaman: `Docs/Kartu-Cepat-Presentasi.md`.

## 1. Kalimat pembuka (30–45 detik)

> “RIMBARA adalah game restorasi hutan berbasis Unity. Target awal kami adalah
> mini-loop: bergerak, mengambil bibit, menanam, memadamkan api, dan memperbarui
> HUD. Fondasi itu sudah berjalan. Sekarang versi storyline sudah mencakup tiga
> fase: bencana, pemulihan, dan hutan pulih, dengan lima misi berurutan. Kami masih
> menyelesaikan polish dan playtest seluruh map; kami tidak mengklaim semua
> fitur rencana sudah selesai.”

Jangan menyebut “100% selesai” atau persentase proyek yang tidak punya ukuran.
Counter/progress bar di game adalah **progres misi pemain**, bukan persentase
selesainya tugas pengembangan tim.

## 2. Apakah pembagian awal A/B/C masih relevan?

**Ya, sebagai pembagian bidang dan tanggung jawab.** Bukan bukti bahwa setiap
orang menulis semua file di bidang itu sendiri. Mini-loop minggu pertama tetap
baseline; implementasi sekarang sudah berkembang jauh melampaui baseline.

| Bidang awal | Penjelasan tanggung jawab sekarang | Contoh yang bisa diperlihatkan |
|---|---|---|
| A — fondasi, player, kamera, terrain | Integrasi controller/input/kamera, collision dan layout level, setup version control serta scene | `Volunteer` di Hierarchy; `VolunteerRig.cs`; `VolunteerInput.cs`; `ThirdPersonController`; Cinemachine; Terrain |
| B — interaksi, bibit, lahan tanam | Deteksi E, validasi jarak/penghalang, pickup/gali/tanam, interaksi NPC dan target dunia | `InteractionDetector.cs`; `IInteractable.cs`; `RecoverySeedling.cs`; `RecoveryPlantingPoint.cs`; `RecoveryTask.cs` |
| C — state, event, HUD, api | Penyimpanan progres per sesi, urutan misi, update HUD/dialog, pemadaman/penyiraman, pause/ending | `GameState.cs`; `RecoveryProgress.cs`; `DisasterHud.cs`; `FirePoint.cs`; `WaterSprayer.cs`; `GameFlow.cs`; `EndingDirector.cs` |

Beberapa fitur memang **lintas bidang**: penyiraman menggunakan input A, target
interaksi B, state/UI C. Boleh menjelaskan integrasi sebagai kerja bersama.

### Isi kontribusi aktual bersama tim, bukan menghafal klaim yang belum benar

| Anggota / peran yang disepakati | Yang benar-benar dikerjakan | Bukti: commit/file/demo | Yang masih perlu dipelajari |
|---|---|---|---|
| Nama anggota A: ____ | ____ | ____ | ____ |
| Nama anggota B: ____ | ____ | ____ | ____ |
| Nama anggota C: ____ | ____ | ____ | ____ |

Riwayat Git yang diperiksa saat persiapan menunjukkan akun `vaskoyudha` melakukan
perubahan input player, HUD/Ch42, scene Arunika dan versi Unity; `bilaaah` melakukan
upload/update project, instruksi clone dan perbaikan overlap desa;
`fahrurrozi487` melakukan integrasi storyline Zona 1/merge, dan sesi pengembangan
berbantuan AI ini menambahkan Zona 2/3, river, NPC dan pengujian. Ini bukti aktivitas
akun/commit, **bukan kepastian identitas A/B/C atau seluruh kerja offline**. Verifikasi
dengan anggota tim sebelum dipakai sebagai atribusi individu.

Jika ditanya penggunaan AI/package, jawab jujur sesuai kebijakan mata kuliah:

> “Kami memakai package untuk fondasi dan bantuan AI untuk implementasi serta
> debugging. Kami membedakan source package dari kode integrasi game, dan
> meninjau/menguji hasilnya. Bagian yang saya tanggung jawabkan adalah …; saya
> dapat menunjukkan input, state, output dan file terkaitnya.”

Jangan mengaku membuat controller, model, atau semua kode dari nol jika tidak.
Pahami dan tinjau hasil sebelum mengklaim telah menguasainya. Tool otomatis
tidak menggantikan pertanggungjawaban akademik tiap anggota.

## 3. Progres yang bisa dipertanggungjawabkan

### Sudah bekerja pada build storyline

- Movement berjalan/lari/lompat dan kamera third-person dengan Unity Starter
  Assets + Cinemachine; input game dihubungkan oleh adapter RIMBARA.
- Map original RIMBARA dirakit dari resource environment; bukan menyalin layout
  scene demo vendor. Scene tim/original tidak diganti.
- **Zona 1 / M01–M02:** dialog koordinator, ambil perlengkapan, padamkan **2 api**,
  bersihkan **4 puing** bertanda, kembali melapor.
- **Zona 2 / M03:** pengarahan petugas hutan, **4 bibit**, gali → tanam → siram tiap
  bibit. Membawa bibit dan menyiram adalah state berbeda.
- **Zona 2 / M04:** bersihkan **10 target sampah sungai** dan pulihkan **2 habitat**;
  air/bank presentation berubah mengikuti progres. Sumber air adalah sungai.
- **Zona 3 / M05:** transisi waktu **6 detik**, tetua memberi **1 bibit terakhir**,
  tanam/siram, kunjungi viewpoint, ending kamera/narasi/musik **36 detik**.
- Pause/resume, explicit skip cinematic, result akhir dan replay/reset tiga zona.
- **10 NPC / 8 identitas berproporsi manusia**; player Ch42 tetap. NPC decorative
  bergerak sederhana, bukan navigation AI/dialog generatif.
- Presentasi pemain menggunakan Bahasa Indonesia.

### Bukti pengujian terakhir sebelum publikasi

| Pemeriksaan | Hasil |
|---|---|
| EditMode | **105/105 lulus** |
| Runtime Windows, normal ending + replay | **238/238 lulus**, 0 runtime errors |
| Runtime Windows, skip ending + replay | **237/237 lulus**, 0 runtime errors |
| Build Windows terakhir | Succeeded, **0 error / 0 warning** |
| Audit label Indonesia | **55 UI / 48 world labels**, 0 untranslated |

File bukti: `Docs/Evidence/HumanNPCs/README.md`, `RuntimeNormal/report.json`,
`RuntimeSkip/report.json`, `editmode-tests.xml`, `windows-build.json`.
Test runtime menggabungkan input nyata dan fixture terisolasi/teleport untuk
beberapa interaksi. **Bukan** bukti playtest manual seluruh perjalanan atau
benchmark semua perangkat. Versi diuji pada Windows / RX 6600 / D3D11 / 1600×900.

### Belum selesai / tidak tersedia

- Opening cinematic/escort sesuai prolog belum diimplementasikan.
- Achievement, full save/load progres, voice-over dan AI NPC kompleks belum ada.
- Progres misi hanya per sesi; yang persist antar launch adalah volume master.
- Human playtest full-map, berbagai resolusi/projector, focus-loss dan performance
  lintas perangkat belum disertifikasi.
- Revisi busana/visual lokal, framing cinematic dan polish artistic masih dapat
  ditingkatkan; model NPC bukan klaim rekonstruksi budaya Indonesia autentik.
- Paket yang awalnya direncanakan tidak semuanya dipakai: particle/audio yang
  tampil dirakit/dibuat dalam project. Jangan mengatakan menggunakan Unity
  Particle Pack atau Synty bila belum benar-benar diimpor.

## 4. Demo aman 5–7 menit (bukan main semua misi dari awal)

### Sebelum hari presentasi

1. Pakai **Unity 6000.3.6f1**. Tunggu import/compile selesai sebelum dosen hadir.
2. Buka **`Assets/Scenes/FromAshes.unity`**, bukan `Main.unity`. Main adalah
   baseline/scene tim dan tidak memuat seluruh storyline terbaru.
3. Game view di 16:9. Coba keyboard/mouse, suara dan projector pada laptop presentasi.
4. Siapkan executable `Builds/Storyline-Windows/RIMBARA-FromAshes.exe` beserta
   folder `_Data`, `UnityPlayer.dll` dan file pendukung. Jangan memindahkan EXE saja.
   Build lokal di-ignore version control: anggota lain harus build/download
   paket lengkap yang memang dibagikan, bukan berharap EXE ada dari clone.
5. Siapkan gambar `Docs/Captures/HumanNPCs/unity-lineup.png` dan evidence ending/
   sungai sebagai backup. Screenshot bukan live demo; sebutkan secara jelas.
6. Bagi peran: satu orang operator mouse/keyboard, satu menjelaskan state, satu
   membuka file/bukti jika diminta. Jangan ketiganya mengoperasikan komputer.
7. Rehearsal sekali dengan pertanyaan dadakan; siapkan catatan halaman ini offline.
8. Jangan update Unity/package, switch branch, merge scene atau regenerate map
   tepat sebelum presentasi. Download/sinkronkan snapshot lebih dulu.

### Urutan ketika dosen meminta Unity dibuka

1. **Project panel:** tunjukkan `Assets/Scenes/FromAshes.unity` dan folder
   `Scripts/Player`, `Scripts/Interaction`, `Scripts/Mission`, `Scripts/UI`.
2. **Hierarchy:** pilih `Volunteer`; Inspector menunjukkan controller, input,
   Animator dan detector. Jelaskan bahwa GameObject adalah wadah komponen.
3. **Play:** mulai dari menu Indonesia → briefing → game. Klik Game view agar
   input diterima; WASD/mouse, Shift lari, Spasi lompat, Esc pause.
4. Dekati koordinator. E membuka dialog, BERIKUTNYA/LANJUTKAN melanjutkan;
   terima pengarahan sebelum mengambil perlengkapan.
5. Tunjukkan satu contoh pemadaman: bidik api lalu tahan klik kiri. Tunjukkan
   counter/prompt; dari sini sudah terbukti jalur input → logic → state → visual.
6. **Stop Play** sebelum pemeriksaan struktur scene. Di Hierarchy cari
   `Recovery planting bed 1`, pilih komponen `RecoveryPlantingPoint`; tunjukkan
   reference chapter, soil/tree/water target dan Water Seconds = 2.
7. Pilih `Forest ranger - Recovery guide` atau `Village elder - restored chapter`:
   root memiliki logic interaksi; child memiliki model/Animator. Identitas visual
   tidak menggantikan state/dialog.
8. Untuk fase akhir yang membutuhkan seluruh misi, gunakan evidence hasil test
   atau build yang telah dimainkan dalam rehearsal. Jangan mengaku screenshot/
   fixture diagnostik sebagai progres live yang baru saja diselesaikan.

**Jangan tekan** menu authoring Create/Add Disaster/Recovery/River/Restored untuk
demo. Menu itu membuat scene/extension awal dan bukan tombol “jalankan game”.
Jangan jalankan `create`, `author-map`, `recovery`, `river`, `restored` pada map
yang sudah dibuat. Jangan edit script ketika Play masih aktif.

Perubahan gameplay saat Play umumnya tidak tersimpan; setelah Stop state reset.
Untuk pindah tampilan scene, gunakan Scene tab dan F (Frame Selected), bukan
memindahkan player/target permanen secara sembarangan.

## 5. Cara menjelaskan fitur tanpa hafal setiap baris

Pakai empat kalimat: **input → validasi → state → output**.

### Mengambil / menggali / menanam

> “Player menekan E. InteractionDetector mencari target dalam radius, memilih
> target yang available dan tidak terhalang. Target melakukan Interact dan
> meminta perubahan ke model progres. Bila valid, state dan visual/HUD berubah.”

`InteractionDetector.Scan()` memakai `Physics.OverlapSphereNonAlloc`, layer
interaction, jarak dan `Physics.Linecast`. `RecoveryPlantingPoint.Interact()`
memilih gali/tanam. `RecoveryProgress.Plant()` mensyaratkan misi Planting, lubang
yang sudah digali dan `Carried >= 1`. `HashSet` ID mencegah hitungan ganda.

### Memadamkan / menyiram

> “Klik kiri harus ditahan dengan target pada crosshair. Raycast kamera menentukan
> target. Sistem memeriksa tahap misi, jarak dan penghalang, lalu mengakumulasi
> waktu kontak. Particle air memberi feedback; bukan particle itu sendiri yang
> menghitung progres.”

`WaterSprayer.UpdateAim()`/`ApplySpray()`, `FirePoint.ReceiveWater()` dan
`RecoveryPlantingPoint.ReceiveWater()` adalah tempat pemeriksaan. Bibit memakai
2 detik kontak valid; jangan mengklaim simulasi air/fluid physics.

### Counter HUD / misi terkunci

> “State adalah sumber data, bukan angka di teks HUD. Setelah aksi sah, event
> Changed/ProgressChanged memberi tahu UI. Chapter menentukan langkah berikutnya,
> sehingga player tidak dapat menanam atau mengakhiri cerita sebelum syaratnya.”

`GameState` untuk baseline/state sesi; storyline memakai `DisasterProgress`,
`RecoveryProgress`, `RestoredProgress` terpisah. `DisasterHud` subscribe chapter
`Changed` dan `GameFlow.ModeChanged`. `GameFlow` mengatur menu/playing/paused/
transition/cinematic/victory dan controls player.

### Hutan menjadi hijau / NPC

> “Visual lingkungan berubah mengikuti fase/progres. Pohon dewasa ditampilkan
> setelah transisi waktu naratif, bukan seketika setelah bibit disiram. NPC
> memakai rig Humanoid dengan retarget animasi; logic dialog tetap di root.”

`RecoveryEnvironment`, `RecoveryRiver`, `RestoredEnvironment`, `EndingDirector`;
`HumanNpcVisual` hanya identitas model, `RestoredAmbientMotion` pergerakan sederhana.

## 6. Pertanyaan yang sering membuat panik

| Pertanyaan dosen | Jawaban singkat yang benar |
|---|---|
| “Kalian buat semua dari nol?” | Tidak. Controller/camera/UI library/model/environment memakai sumber berlisensi; tim mengintegrasikan mekanik, state, scene dan presentasi game. |
| “Kalau E ditekan dua kali?” | Target memiliki ID dan model progres menolak duplicate/syarat yang tidak valid; bukan menambah counter terus. |
| “Mengapa bibit belum bisa ditanam?” | Ada mission gate, inventory dan syarat menggali dahulu. Bukan bug kalau dilakukan sebelum tahapnya. |
| “Apakah hutan tumbuh alami?” | Ini perubahan presentasi berbasis aksi dan chapter, bukan simulasi biologis. Ada transisi waktu sebelum mature forest. |
| “Apakah progres tersimpan?” | Belum save/load penuh. Progres per sesi; replay/menu reload scene. Volume disimpan. |
| “Apa yang diuji?” | State/gates/IDs pada EditMode; build serta runtime normal/skip/pause/replay. Full-map human test masih pekerjaan berikutnya. |
| “Kenapa Main berbeda?” | Main dipertahankan sebagai baseline/scene tim; storyline aktif ada di FromAshes. |
| “Apa kontribusi kamu?” | Sebut pekerjaan aktual dan bukti, bukan seluruh daftar tanggung jawab atau klaim semua source package. |
| “Di mana script fitur ini?” | Pilih object di Hierarchy, lihat component Inspector, buka script terkait; mulai dari input → validasi → state → output. |

## 7. Kalau blank: prosedur 20 detik

1. Berhenti klik. Tarik napas, baca pertanyaan kembali.
2. Katakan: **“Sebentar Pak/Bu, saya tunjukkan komponen yang menangani fitur ini
   supaya penjelasannya tepat.”** Ini lebih baik daripada menebak.
3. Temukan object → Inspector → script. Jelaskan satu alur, bukan semua class.
4. Jika bagian anggota lain: **“Untuk integrasi keseluruhan saya jelaskan alurnya;
   detail bagian ini akan dilanjutkan teman yang bertanggung jawab.”**
5. Jika benar-benar belum paham: **“Saya belum bisa memastikan detail itu sekarang.
   Kami catat untuk diperiksa.”** Jangan membuat jawaban palsu.
6. Bila demo gagal: Esc, cek fokus Game view/urutan misi/jarak, Stop Play lalu ulang
   jika perlu. Jika tetap gagal, tunjukkan build/evidence dan akui perbedaannya.

Tidak perlu menguasai seluruh repository sekaligus. Setiap orang minimal harus
bisa menjelaskan **satu fitur miliknya, tiga file terkait, satu contoh validasi,
satu keterbatasan, dan satu bukti pengujian**. Itu target rehearsal yang realistis.

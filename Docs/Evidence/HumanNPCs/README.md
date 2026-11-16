# Evidence NPC manusia

Folder ini khusus hasil setelah penggantian NPC. Evidence Zona 3 sebelum NPC
tetap berada pada folder historis, tidak dipakai untuk menyatakan model baru lulus.

- `npc-import.log`: sepuluh pemasangan visual dan validasi delapan avatar human.
- `editmode-tests.xml`: **105/105 lulus**, termasuk 10 test NPC baru.
- `generated-file-hashes.json`: SHA-256 binary/model/texture/manifest terpasang.
- `source-file-hashes.json`, `mpfb-provenance.json`: source `.blend` dan graphical
  input MPFB dari commit pinned. `tested-build-hashes.json`: binary/assembly build.
- Render authoring: `Docs/Captures/HumanNPCs/blender-lineup.png`.
- `windows-build.json`: build terakhir **Succeeded / 0 errors / 0 warnings**.
  Build awal setelah recompile mencatat warning field vendor `CustomButton.isHeld`
  yang tidak dipakai; vendor script tidak diubah.
- `indonesian-verification.json`: **55 UI / 48 world labels / 0 untranslated**.
- `RuntimeSkip/report.json`: fit final **237/237 lulus / 0 runtime errors**,
  termasuk skip ending, replay, final-tree gates dan delapan animated human portraits.
- `RuntimeNormal/report.json`: fit final **238/238 lulus / 0 runtime errors**,
  normal 36-second ending, pause/resume, replay dan lima misi terkonfirmasi.
- `runtime-summary.json`: ringkasan dua route, perangkat dan ukuran layar aktual.
- Review delapan portrait Unity selesai; gambar gabungan
  `Docs/Captures/HumanNPCs/unity-lineup.png`. Portrait memakai clone opt-in di
  tempat terisolasi berlampu, bukan screenshot posisi NPC yang dipindah di map.
- `InitialRuntime/`: hasil percobaan pertama disimpan, bukan evidence lulus.
  Alur mencapai ending dan avatar runtime valid, tetapi assertion replay gagal.
  Fixture diperbarui untuk menunggu scene handle baru dan dua frame `Start`,
  serta portrait terisolasi berlampu untuk menghindari occlusion oleh posko.
- `InitialSkipRuntime/`: percobaan gagal pada klik tombol mulai. Input driver
  kini menonaktifkan device mouse/keyboard native dalam proses opt-in smoke saja
  agar event pointer asli tidak menimpa pointer virtual. Input OS dan game normal
  tidak diubah; device dipulihkan ketika pengujian selesai.
- `PreFitSkipRuntime/`: **237/237 lulus / 0 runtime errors** sebelum clearance
  garment terakhir. Disimpan sebagai hasil antara, tidak menggantikan pengujian
  final setelah perubahan fit/mesh compression.
- `PreFitNormalRuntime/`: **238/238 lulus / 0 runtime errors** pada model sebelum
  fit akhir; normal ending dan replay terkonfirmasi, tetap bukan report fit final.
- `container-preservation.json`: sepuluh container visual asli tetap memakai
  fileID, parent, posisi, rotasi dan skala yang sama dengan scene backup.
- `script-collider-preservation.json`: **339 existing script/collider blocks**
  sama persis dengan backup; tidak ada block lama yang diubah atau dihapus.
- `retained-transform-preservation.json`: **360 retained existing Transform
  fileIDs** termasuk map/interaksi/player mempertahankan posisi/rotasi/skala/parent.
  Descendant visual Ch42 yang memang diganti tidak termasuk hitungan ini.
- `player-preservation.json`: actual serialized player PrefabInstance
  `1819549308`, source GUID dan avatar override tetap sama persis dengan backup.
- `publication-uvcs.json`: implementasi remote UVCS cs:2 complete/readback.
- `publication-github.json`: push branch + fresh clone/LFS fsck + binary hashes.
  Status/changeset dokumentasi selanjutnya: `Docs/Publication-Status.md`.

Runtime menggunakan fixture untuk interaksi dan rute tertentu; ini bukan
sertifikasi human playtest seluruh map, performance lintas perangkat, atau
kesesuaian pakaian/karakter lokal Indonesia secara artistik.

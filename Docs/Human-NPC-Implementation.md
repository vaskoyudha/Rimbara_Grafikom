# NPC berproporsi manusia

## Ruang lingkup

Penggantian visual saja pada `Assets/Scenes/FromAshes.unity`: koordinator,
petugas hutan, tetua, relawan, warga posko, dan lima warga ambient Zona 3.
Player Ch42, prefab vendor, scene tim/original, interaksi, ID target, trigger,
dialog dan progres tiga zona tidak diganti.

## Sumber dan hak distribusi

- Blender portable **4.5.16**, checksum diverifikasi terhadap daftar resmi Blender.
- MPFB **v2.0.18**, commit `d0a32e57a7f915cb2f2b95410e2117648c7bbb7e`.
- MakeHuman **1.3.0+20240427162744**, distribusi source PPA resmi tim
  `makehuman-official`, bukan arsip dari pengguna lain.
- URL arsip:
  `https://ppa.launchpadcontent.net/makehuman-official/makehuman-community/ubuntu/pool/main/m/makehuman-community/makehuman-community_1.3.0+20240427162744.orig.tar.gz`
- SHA-256 arsip:
  `f9b8c4b3167b49fbeb3f231ca3c77e88b8e379bb72ffd0d5f30002e84b9490ae`.
  Cocok dengan `Checksums-Sha256` dalam `.dsc` publisher; verifikasi signature
  GPG terpisah tidak dilakukan.
- Graphical assets bundled dan output MakeHuman/MPFB: **CC0 1.0**.
  Lisensi dalam distribusi, komentar material dan metadata binary proxy diperiksa.
  Tool MakeHuman AGPL dan MPFB GPL terpisah dari data/model; tool tidak diimpor
  ke `Assets` atau disertakan dalam executable.
- Dua mirror system-pack gagal DNS. Arsip system-pack tersebut **tidak** dipakai.
- Tidak menggunakan asset Quaternius/QAL, tier berbayar, model orang nyata,
  atau logo/seragam instansi.

Notice sumber dan teks CC0 tersimpan di `Docs/Licenses/HumanNPCs/`.
Hash file graphical data yang benar-benar dipakai akan tersedia bersama
`Assets/Art/Characters/HumanNPCs/source-hashes.json`.

## Pipeline

`Tools/Generate-HumanNPCs.py` dipanggil dengan Blender headless. Paket PPA
menyimpan proxy/mesh bundled sebagai `.mhpxy`/`.npz`; script mengonversi
array graphical data menjadi MHCLO/OBJ di folder authoring eksternal, dengan
gerbang metadata CC0 sebelum digunakan.

Delapan phenotype memakai tinggi/umur/gender/bentuk tubuh berbeda. Mata,
alis, rambut, pakaian dan sepatu dipasang serta diberi rig `mixamo_unity`.
Nama rig merupakan konvensi MPFB, bukan download Mixamo baru. Game memakai
controller/animasi Humanoid yang sudah ada; lisensi animasinya tetap Adobe.

Export membersihkan helper mesh, membakar target/mask pakaian, tanpa subdiv,
mempertahankan skin weights, bind T-pose dan tekstur diffuse maksimal 1024 px.
Source `.blend` disimpan di `SourceArt/HumanNPCs`, terpisah dari `Assets` agar Unity tidak memerlukan
Blender terpasang global untuk import; tekstur source dipack saat pengemasan.

Tinggi game model: koordinator 1,75 m; petugas 1,82 m; tetua 1,68 m;
relawan 1,66 m; warga wanita 1,60/1,64 m; warga pria 1,76 m; warga senior 1,69 m.
Mesh tanpa subdiv sekitar 27.644–39.534 triangles per model. Rambut/alis memakai
alpha cutout; dua karakter senior memakai rambut abu terang. Pakaian wanita
memakai warna fabric original dengan bagian celana terpisah, tanpa logo bawaan.
Untuk fit animation, garment mendapat clearance normal 3 mm dan Unity mesh
compression dinonaktifkan agar skin/garment boundaries tidak bergeser berbeda.

Review Blender menemukan skin bleed-through pada export awal; parser binary-to-
MHCLO diperbaiki untuk delimiter blok `verts`/`delete_verts`, dan delete groups
ditambahkan eksplisit. Export terbaru menyembunyikan kulit di bawah pakaian
serta membuang geometry tertutup. Tinjauan Unity/animasi tetap gerbang terpisah.

`Assets/Editor/HumanNpcBuilder.cs` memasang FBX dalam container visual lama,
tanpa rename root serialized. Ambient scripts tetap pada root. Avatar diperiksa
valid/Humanoid, material memakai URP Lit dan alpha clip untuk rambut/alis.
Overlay jubah/cap/badge buatan awal dihapus hanya setelah pengganti terpasang.
Scene backup dibuat sebelum pemasangan.
Perbandingan YAML backup/scene terpasang mengonfirmasi sepuluh container visual
mempertahankan fileID/parent/transform dan **339 existing script/collider blocks**
tidak diubah atau dihapus.

## Status pengujian

Authoring dan import selesai: 10 visual / 8 avatar Humanoid valid.
Review lineup Blender tidak lagi menunjukkan skin bleed-through seperti export
awal. EditMode **105/105** lulus. Build Windows terakhir **Succeeded / 0 errors /
0 warnings**. Build awal recompile mencatat field vendor `CustomButton.isHeld`
tidak dipakai; vendor script tidak dimodifikasi.
Audit Indonesia **55 UI / 48 world labels / 0 untranslated**.
Runtime fit final: **normal 238/238**, **skip 237/237**, keduanya **0 runtime errors**
dan replay lulus. Normal ending 36 detik, pause/resume, skip confirmation dan reset
semua tiga zona diuji melalui executable Windows pada 1600×900 / RX 6600 / D3D11.
Delapan portrait animasi Unity ditinjau; celah garment yang terlihat pada export
terkompresi awal tidak tampak pada portrait final setelah clearance/compression off.
Gambar gabungan: `Docs/Captures/HumanNPCs/unity-lineup.png`. Portrait merupakan
clone diagnostik terisolasi, tidak mengubah posisi NPC di map. Evidence final ada
di `Docs/Evidence/HumanNPCs/RuntimeNormal` dan `RuntimeSkip`; hasil percobaan awal
dan sebelum fit final tetap disimpan terpisah.
Pengujian pertama berhasil menyelesaikan lima misi/ending tanpa error runtime,
tetapi klik replay gagal. Percobaan skip awal gagal pada klik mulai. Evidence
gagal dipertahankan; driver opt-in mengisolasi pointer virtual dari input native
serta menunggu scene handle baru untuk pemeriksaan replay. Ini perubahan harness
diagnostik, bukan bypass interaksi atau perubahan mekanik game normal.

Publikasi GitHub/UVCS diotorisasi pemilik pada 16 November 2026; status/identitas
snapshot dicatat di `Docs/Publication-Status.md`. Review manusia terhadap styling
lokal dan rute full-map tetap terpisah dari fixture runtime otomatis.

## Menjalankan dan memelihara

- Buka `Assets/Scenes/FromAshes.unity` di Unity 6000.3.6f1 atau jalankan
  `Builds/Storyline-Windows/RIMBARA-FromAshes.exe` bersama folder `_Data`/DLL.
- `Tools/Invoke-Storyline.ps1 -Command verify-npcs` untuk avatar/scene checks;
  `-Command build` untuk build storyline, bukan build Main legacy.
- Untuk re-export model, jalankan script Blender dengan `--mpfb`, `--assets`,
  `--output`, `--sources` dan opsional `--preview`. Gunakan source library pinned
  eksternal. `--preview-only` hanya merender export yang sudah ada.
- Sesudah menyalin FBX/texture/manifest yang diubah, gunakan `-Command npc-imports`.
  Jangan regenerasi map/Recovery/river/Restored untuk memperbarui NPC.
- Semua file FBX/PNG/BLEND mengikuti Git LFS yang sudah dikonfigurasi. Publikasi
  atau check-in tetap operasi terpisah yang membutuhkan arahan tim.

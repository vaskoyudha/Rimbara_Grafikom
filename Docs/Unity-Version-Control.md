# Kolaborasi tim: Unity Version Control + GitHub

## Keputusan dan status

- **Unity Version Control (UVCS) adalah sumber utama kolaborasi** setelah upload
  awal dan pengujian workspace rekan selesai. GitHub tetap dipertahankan sebagai
  mirror manual; kedua sistem tidak otomatis sinkron.
- Unity Editor: **6000.3.6f1**. Paket integrasi: `com.unity.collab-proxy` **2.14.0**.
- Unity Cloud organization: **RIMBARA Team**, ID `18968548628243`.
- Cloud project: **Rimbara_Grafikom**, ID `39c9aaf1-2979-4d0c-bac6-c108fa2cc535`.
- Region UVCS: **Asia South East — Singapore**; sudah dipilih oleh pemilik dan
  tidak dapat diubah setelah dikonfigurasi menurut dashboard Unity.
- Repository dibuat melalui Cloud project; nama tampilan **Rimbara_Grafikom**.
  GUID repository: `eb57d8c0-fb52-410b-8878-f4c159210160`.
  Slug UVCS yang ditampilkan dashboard: `rimbara-team`.
  Gunakan selector repository resmi saat membuat workspace; jangan menebak alamat
  server dari nama organization.
- Upload awal telah terkonfirmasi di Cloud sebagai **changeset `cs:1`** pada
  `/main` (komentar: `Add project assets and settings to Unity Version Control.`).
  Workspace lokal **Rimbara_Grafikom-Rombel2** menggunakan mode Standard dan
  repository `Rimbara_Grafikom/Rimbara_Grafikom`. Akses/download serta pertukaran
  perubahan dengan workspace rekan belum terverifikasi.
- Snapshot tiga zona/NPC dan panduan presentasi sedang dipublikasikan dengan
  otorisasi pemilik; lihat `Publication-Status.md` untuk hasil server terkini.
- Paket sudah di-resolve oleh Unity dan **56/56 EditMode tests lolos** setelah
  pemasangan. Editor telah dibuka, tetapi koneksi tool Editor tidak terautentikasi;
  pemilik menyelesaikan setup workspace melalui Window > Unity Version Control.
  Jangan menyamakan login browser dengan login Hub/Editor.
- Perkiraan source lokal saat setup: `Assets` **0,444 GiB**, `Docs` **0,014 GiB**,
  di luar cache/build/Git. Ukuran riwayat server dan penggunaan egress akan berbeda
  dan bertambah sesuai changeset/download.
- Agent tidak mengaktifkan paket berbayar atau mengirim undangan. Status billing
  maupun undangan yang dilakukan sendiri oleh pemilik perlu diperiksa di dashboard.
- Dashboard pada saat setup menampilkan Free tier: **25 GB Standard Storage** dan
  **100 GB Standard Egress**, dibagi dengan layanan DevOps lainnya. Periksa kembali
  batas pengguna, biaya, storage/egress dan ketentuan terbaru sebelum mengundang
  seluruh anggota atau mengaktifkan upgrade; jangan menganggap pengguna tak terbatas.

Dashboard repository:
https://cloud.unity.com/organizations/18968548628243/projects/39c9aaf1-2979-4d0c-bac6-c108fa2cc535/plastic-scm/organizations/18968548628243/repositories/Rimbara_Grafikom%2FRimbara_Grafikom

## Menghubungkan dan mengunggah project lokal

1. Tutup proses Unity lain yang membuka project ini. Buka folder **Rimbara_Grafikom**
   yang berisi `Assets`, `Packages`, `ProjectSettings` menggunakan Unity 6000.3.6f1.
   Jangan memilih project lama `RIMBARA` atau menjalankan generator scene/map.
2. Pastikan Unity Hub/Editor login menggunakan akun pemilik, lalu buka
   **Window > Unity Version Control**. Paket tersedia setelah Package Manager
   selesai resolve dan compile.
3. Gunakan repository **yang sudah dibuat** di organization **RIMBARA Team**.
   Jika dialog hanya menawarkan repository baru, gunakan client UVCS Desktop
   untuk membuat workspace pada repository existing; jangan membuat duplikat.
4. **Jangan Update/Switch workspace kosong ke atas folder lokal yang berisi
   perubahan belum disimpan.** Tujuannya adalah memasukkan snapshot lokal ke
   repository kosong, bukan menggantinya dengan isi repository kosong.
5. Periksa `ignore.conf` sebelum setup/check-in: `Library`, `Temp`, `Builds`,
   `Artifacts`, `.git`, `.plastic`, `Backups`, dan credential lokal tidak boleh
   masuk upload. Semua file source di `Assets`, termasuk `.meta`, harus masuk.
   `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings`,
   `Docs`, `Tools`, README dan notice lisensi juga dibagikan.
6. Tinjau **Pending Changes** dan initial check-in. Snapshot harus menyertakan
   perubahan lokal **FromAshes** dan resource EmaceArt yang lengkap. Jangan
   melakukan upload berdasarkan `git archive HEAD` sebelum seluruh perubahan
   storyline masuk commit Git: file lokal yang belum di-commit tidak ada di archive.
7. **Peringatan:** Create Workspace dapat melakukan automatic initial check-in
   pada repository kosong, termasuk repository existing. Jangan menyelesaikan
   wizard sebelum file, izin asset, kuota, dan upload awal disetujui. Gunakan
   repository existing agar tidak membuat repository duplikat tanpa sengaja.
8. UVCS menyimpan binary asset secara native. Upload file binary asli, bukan
   pointer Git LFS. Masalah object Git LFS historis telah diperbaiki dan fresh clone
   berhasil; upload UVCS sendiri tetap tidak memperbaiki storage GitHub.
9. Tinjau license resource pihak ketiga dan batasi akses pada anggota tim yang
   berhak. Jangan menjadikan raw asset/repository publik. UVCS untuk kolaborasi
   bukan izin redistribusi asset.

## Mengundang dan membuka project oleh rekan

1. Pemilik: buka organization **RIMBARA Team > Members** dan invite alamat email
   yang digunakan masing-masing rekan untuk Unity ID. Jangan berbagi akun/password.
2. Berikan akses project **Rimbara_Grafikom** dan permission UVCS read/write yang
   diperlukan. Periksa user/seat entitlement: anggota organization belum tentu
   otomatis mendapat semua permission repository atau seat yang dibutuhkan.
   Gunakan akses anggota biasa; admin/owner hanya bila diperlukan.
3. Rekan menerima invite, login Hub/UVCS Desktop, lalu pilih remote repository dan
   buat **workspace lokal sendiri**. Gunakan folder non-OneDrive/non-Dropbox bila
   memungkinkan. Sinkronisasi folder cloud bukan pengganti version control.
4. Setelah snapshot project tersedia di server, gunakan Hub **Add/Open remote
   project** jika tersedia, atau checkout melalui UVCS Desktop lalu Add from disk.
5. Gunakan Unity **6000.3.6f1** yang sama. Tunggu import dan package resolution,
   buka `Assets/Scenes/FromAshes.unity`, lalu Play. Jangan regenerate Main/map.
6. Validasi dengan satu rekan: download workspace baru, compile, Play, buat perubahan
   kecil pada branch tugas, check-in, lalu pastikan pemilik dapat menerima perubahan.
   Baru setelah itu nyatakan kolaborasi aktif.

## Cara kerja harian

### Batch publisher untuk snapshot presentasi

`Assets/Editor/UvcsSyncBridge.cs` adalah utility **batch-only** untuk paket UVCS
yang sudah terpasang, bukan gameplay atau tombol demo. Ia membaca allow-list
source lokal dari `Artifacts/uvcs-approved-files.txt`, menolak cache/credential/
path di luar project, dan Stage hanya menambahkan file yang ditinjau tanpa
workspace Update/Switch. Publish menolak pending file di luar allow-list lalu
memakai check-in API paket. Comment/pending/result lokal tidak masuk repository.
Tool mengandalkan API internal paket 2.14.0; jangan dianggap CLI lintas versi
yang stabil. Workflow harian tim tetap UI Pending Changes/check-in/review.

- Update workspace sebelum mulai, setelah menyimpan/check-in/shelve pekerjaan sendiri.
  Gunakan satu branch per tugas, misalnya `/main/feature/zona2-penanaman`.
- Simpan scene/prefab dan review Pending Changes beserta `.meta` pasangannya.
- Coordinate pemilik scene: jangan mengedit `FromAshes.unity` bersama-sama tanpa
  koordinasi. Konfigurasi lock server untuk scene, prefab, terrain data dan binary
  asset bersama bila diperlukan; `ignore.conf` tidak memasang lock otomatis.
- C# dapat di-merge; konflik scene/terrain tidak boleh diselesaikan dengan memilih
  semua perubahan salah satu pihak tanpa pemeriksaan. Pisahkan kerja menjadi prefab
  atau scene additive ketika masuk akal.
- Check-in kecil dengan deskripsi jelas, review, lalu merge ke `/main`.
- Semua teks yang terlihat pemain tetap **Bahasa Indonesia**.

## Mirror GitHub: satu arah dan terkontrol

Tetapkan satu maintainer mirror. Setelah perubahan disetujui di UVCS:

1. Ambil changeset UVCS yang disetujui. Catat changeset/label di pesan commit Git.
2. Gunakan clone GitHub terpisah/bersih untuk menyalin snapshot source yang sama.
   Pertahankan `.git` clone tersebut; jangan menyalin `.git` atau `.plastic` antar
   workspace. Tinjau rename/delete secara eksplisit, tanpa broad recursive delete.
3. Inspect diff, jalankan verifikasi, commit pada branch mirror dan upload seluruh
   binary Git LFS yang dibutuhkan sebelum push/PR yang memang diotorisasi.
4. Jangan mengembangkan perubahan berbeda di kedua repository. Jika ada hotfix
   GitHub, integrasikan dulu ke UVCS dan review sebelum mirror berikutnya.

`.git` project saat ini dan seluruh history tetap dipertahankan. Aktivasi UVCS
tidak menjalankan git reset, commit, push atau merge otomatis.

## Dokumentasi resmi

- https://docs.unity3d.com/Packages/com.unity.collab-proxy@2.14/manual/index.html
- https://docs.unity3d.com/Packages/com.unity.collab-proxy@2.14/manual/GetStartedNewRepository.html
- https://docs.unity3d.com/Packages/com.unity.collab-proxy@2.14/manual/GetStartedExistingRepository.html
- https://docs.unity.com/en-us/unity-version-control/ignore-files
- https://docs.unity.com/en-us/unity-version-control/config-files/filter-pattern

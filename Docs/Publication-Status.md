# Publikasi snapshot tiga zona dan NPC

Pemilik mengotorisasi push GitHub dan check-in Unity Version Control pada
**16 November 2026**. GitHub adalah mirror manual; UVCS adalah kolaborasi utama.
Keduanya tidak tersinkron otomatis.

Snapshot mencakup Zona 2, sungai 10 sampah, Zona 3/ending, delapan identitas NPC
untuk sepuluh instance, source art/provenance, tests dan panduan presentasi dosen.
Baseline: Git `f4ec160` / upload UVCS awal `cs:1`. Build/evidence akhir:
105/105 EditMode, 238/238 normal runtime, 237/237 skip runtime, 0 runtime errors,
Windows build 0 error / 0 warning.

## Identitas dan hasil

- Commit implementasi Git: **`30a0e1329b54cb0403f8dade2d9013ee5904ad69`**.
- UVCS snapshot implementasi: **`cs:2`**, branch `/main`, dikonfirmasi oleh
  check-in API. **`cs:3`** menyinkronkan panduan/dokumentasi dan verifier;
  remote readback complete, parent `cs:2`, GUID
  `1d02eb5a-5d20-4c15-b0db-84b26033e22c`. Pending Changes kosong sesudah check-in.
- GitHub implementasi berhasil push ke **`feature/restored-zone3`**, termasuk
  **75 object LFS / sekitar 345 MB**. Branch tidak auto-merge ke main.
- Readback UVCS mengonfirmasi `cs:2` complete, parent `cs:1`, GUID
  `5a19da90-a630-4215-b0d7-0aa73c3f0a30`, 442 remote diff entries.
  Bukti: `Docs/Evidence/HumanNPCs/publication-uvcs.json`.
- Commit dokumentasi GitHub yang diverifikasi: **`055da6d5f0ed46bcf77e63be1301730e52a9ff58`**.
- **Fresh clone + Git LFS fsck OK**; 27 binary NPC file hashes cocok, dua file
  JSON sama setelah normalisasi CRLF/LF Git. Tidak ada upstream LFS hilang.
  Bukti: `Docs/Evidence/HumanNPCs/publication-github.json`.
- EditMode lokal setelah utility publikasi ditambahkan kembali lulus **105/105**.
  Unity import pada clone baru belum dijalankan; jangan menyatakan telah diuji.
- Catatan verifikasi ini dibagikan sebagai dokumentasi lanjutan; tidak ada
  perubahan gameplay atau merge ke GitHub main. History server bisa memiliki
  commit/changeset pencatatan setelah identitas snapshot yang disebut di atas.

Jangan menyamakan file lokal, commit lokal, upload LFS, push branch, merge main
dan check-in UVCS. Catatan akhir memakai changeset/commit server aktual.

GitHub repository: `https://github.com/vaskoyudha/Rimbara_Grafikom`.
Branch publikasi: `feature/restored-zone3` (tidak auto-merge ke `main`).
UVCS workspace: `Rimbara_Grafikom-Rombel2`; branch `/main`;
repository selector: `Rimbara_Grafikom/Rimbara_Grafikom@18968548628243@cloud`.

Cache `Library`, local `Artifacts`/`Backups`, build executable, Git/UVCS credential
dan state workspace tidak dipublikasikan. Source `.fbx`/`.png`/`.blend` memakai
Git LFS pada GitHub dan binary asli pada UVCS. Source license notices dipertahankan.
Tidak ada upgrade subscription, perubahan akses/public visibility, atau merge
riwayat tim yang dilakukan sebagai bagian push ini.

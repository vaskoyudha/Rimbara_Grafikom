# Bahasa game: Indonesia

Bahasa pemain untuk storyline RIMBARA ditetapkan **Bahasa Indonesia (id-ID)**.
Nama game/Arunika tetap sama. Menu, kontrol, HUD, dialog, penanda dunia, tujuan
misi, feedback interaksi, jeda, dan hasil akhir bab menggunakan Bahasa Indonesia.

Nama teknis file, hierarchy, prefab, ID objek, enum, Animator parameter dan binding
input tidak diterjemahkan agar referensi Unity tetap stabil. Penanda tujuan HUD
memakai nama tampilan Indonesia, bukan menampilkan nama GameObject berbahasa Inggris.

Judul cerita: **Dari Abu Menuju Kehidupan**.
Tagline: **Setiap Pohon Menyimpan Cerita.**

| Tahap | Nama Indonesia |
| --- | --- |
| Arrival | Kedatangan |
| Disaster Zone | Zona Bencana |
| Stop the Flames | Padamkan Api |
| Clean the Wasteland | Bersihkan Lahan |
| Recovery Zone | Zona Pemulihan |
| Plant the Future | Tanam Masa Depan |
| Bring Life Back | Kembalikan Kehidupan |
| Restored Zone | Zona Hutan Pulih |
| A Promise to the Forest | Janji untuk Hutan |

`Assets/Scripts/UI/IndonesianText.cs` menyimpan teks UI statis/penanda dan konstanta
yang dipakai pengujian. Dialog dan feedback dinamis juga diterjemahkan pada script
gameplay. Scene yang sudah diedit menerapkan bahasa melalui
`Tools/Invoke-Storyline.ps1 -Command indonesian` tanpa regenerasi map.

Pemeriksaan bahasa memastikan setiap label UI nonkosong terdaftar dan cocok,
serta penanda lama telah diterjemahkan. Pengujian runtime memeriksa menu, kontrol,
dialog, HUD dan feedback Indonesia. Laporan teknis/diagnostik boleh berbahasa Inggris;
itu bukan teks yang ditampilkan kepada pemain.

Scene asli tim/vendor tidak ditulis ulang. Penerapan penuh UI statis ini berlaku
pada scene aktif storyline `Assets/Scenes/FromAshes.unity` dan build storyline.
Seluruh konten pemain baru untuk zona selanjutnya harus mengikuti bahasa ini.

Bahasa UI bukan pilihan yang berubah mengikuti bahasa Windows: build ini secara
eksplisit menggunakan Bahasa Indonesia. Tidak ada perubahan gameplay, ID progress,
kontrol, tata letak map, atau format save yang diperlukan untuk penerjemahan.

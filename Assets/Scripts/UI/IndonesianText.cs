using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rimbara.UI
{
    // Presentation only. Hierarchy names, object IDs, input bindings and serialized
    // references stay stable; all future player-facing copy should be Indonesian.
    public static class IndonesianText
    {
        public const string WaterAction = "TAHAN KLIK KIRI  |  Padamkan api";
        public const string SeedlingWaterAction = "TAHAN KLIK KIRI  |  Siram bibit";
        public const string ChapterPending = "masih dalam pengembangan";
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { "Menu eyebrow", "ARUNIKA / PROGRAM RELAWAN PEMULIHAN HUTAN" },
            { "RIMBARA title", "RIMBARA" },
            { "Game subtitle", "DARI ABU MENUJU KEHIDUPAN" },
            { "Menu story", "Kamu tiba di kawasan yang dilanda kebakaran.\nPadamkan api. Bersihkan lahan. Beri kehidupan kesempatan untuk kembali." },
            { "Menu scope", "BENCANA → PEMULIHAN → HUTAN PULIH / MISI 01–05" },
            { "Menu footer", "Setiap Pohon Menyimpan Cerita.\nVERSI PENGEMBANGAN TIM / TIGA ZONA" },
            { "Start restoration label", "MULAI PEMULIHAN" },
            { "Controls button label", "CARA BERMAIN" },
            { "Menu quit label", "KELUAR" },
            { "Briefing eyebrow", "POSKO RELAWAN / HUTAN ARUNIKA" },
            { "Briefing title", "HARI PERTAMAMU" },
            { "Briefing story", "Hutan masih terbakar. Datangi posko darurat dan temui koordinator. Ambil perlengkapan, padamkan titik api yang ditandai, lalu bersihkan puing sisa kebakaran." },
            { "Briefing steps", "01–02 / Padamkan api dan bersihkan lahan\n03 / Ambil bibit, gali, tanam, dan siram\n04 / Bersihkan sungai dan pulihkan habitat\n05 / Janji untuk Hutan: bibit terakhir dan titik pandang" },
            { "No fail promise", "Tanpa batas waktu atau batas air. Karakter tidak menerima cedera." },
            { "Enter Arunika label", "MASUK KE ARUNIKA" },
            { "Briefing back label", "KEMBALI" },
            { "Help eyebrow", "PANDUAN LAPANGAN" },
            { "Help title", "CARA BERMAIN" },
            { "Control list", "WASD / Bergerak    MOUSE / Putar kamera\nSHIFT / Berlari    SPASI / Melompat\nE / Bicara, ambil, gali, tanam, dan bersihkan\nKLIK KIRI / Tahan untuk padamkan api atau siram bibit\nESC / Jeda, lanjutkan, atau tutup dialog" },
            { "Help tip", "Ikuti penanda dan peta. Dekati objek untuk berinteraksi. Bidik api atau bibit sebelum menyemprot; penghalang dapat menghalangi air. Setiap bibit harus disiram." },
            { "Help back label", "KEMBALI KE MENU" },
            { "Controls text", "WASD gerak / SHIFT lari / SPASI lompat / E interaksi / KLIK KIRI air / ESC jeda" },
            { "Map title", "ARUNIKA / PETA" },
            { "Pause eyebrow", "TARIK NAPAS SEJENAK" },
            { "Pause title", "PERMAINAN DIJEDA" },
            { "Pause description", "Progresmu tetap tersimpan selama jeda. Lanjutkan saat siap." },
            { "Volume label", "VOLUME UTAMA" },
            { "Resume mission label", "LANJUTKAN MISI" },
            { "Pause return menu label", "KEMBALI KE MENU" },
            { "Pause quit label", "KELUAR" },
            { "Victory eyebrow", "ZONA BENCANA / BAB SELESAI" },
            { "Victory title", "AWAL YANG BERSIH" },
            { "Victory story", "Api telah padam. Lahan telah dibersihkan.\nBencana berhasil diatasi. Perjalanan pemulihan dimulai." },
            { "Play again label", "MAIN LAGI" },
            { "Victory return menu label", "MENU" },
            { "Victory quit label", "KELUAR" },
            { "Dialogue next label", "BERIKUTNYA" },
            { "Mission number", "PROLOG / POSKO DARURAT" },
            { "Mission title", "KEDATANGAN" },
            { "Mission description", "Datangi posko darurat. Tekan E untuk berbicara dengan koordinator." },
            { "Seedling counter", "PERLENGKAPAN   BELUM DIAMBIL" },
            { "Tree counter", "PUING DIBERSIHKAN   0 / 4" },
            { "Fire counter", "API PADAM   0 / 10" },
            { "Restoration percent", "0% BAB BENCANA" },
            { "Briefing targets", "10 TITIK API / 4 TUMPUKAN PUING / ZONA 1" },
            { "Waypoint instruction", "POSKO DARURAT" },
            { "Interaction text", "[E]  Berinteraksi" },
            { "Feedback text", "" },
            { "Mission results", "Zona Hutan Pulih masih dalam pengembangan." },
            { "Dialogue text", "KOORDINATOR MISI" },
            { "Ending subtitle", "" },
            { "Skip cinematic label", "LEWATI SINEMATIK" }
        };

        private static readonly Dictionary<string, string> WorldLabels = new Dictionary<string, string>
        {
            { "MISSION COORDINATOR", "KOORDINATOR MISI" },
            { "COLLECT EQUIPMENT", "AMBIL PERLENGKAPAN" },
            { "DESIGNATED DEBRIS", "PUING BERTANDA" },
            { "REMAINING FIRE", "TITIK API" },
            { "EMERGENCY MISSION POST", "POSKO DARURAT" },
            { "INSPECT THE FOREST", "PERIKSA HUTAN" },
            { "RETURN TO VOLUNTEER POST", "KEMBALI KE POSKO" },
            { "COLLECT SEEDLING", "AMBIL BIBIT" },
            { "PLANT A TREE", "TANAM POHON" },
            { "A / DAMAGED FOREST", "A / HUTAN TERDAMPAK" },
            { "A / BURNED FOREST", "A / HUTAN TERBAKAR" },
            { "B / NEW ROOTS", "B / PEMULIHAN" },
            { "C / LAST FLAMES", "C / TITIK API" }
        };

        public static void ApplyStaticLabels(Transform canvas)
        {
            foreach (TMP_Text label in canvas.GetComponentsInChildren<TMP_Text>(true))
                if (Labels.TryGetValue(label.name, out string text))
                {
                    label.text = text;
                    label.enableAutoSizing = true;
                    label.fontSizeMax = Mathf.Max(label.fontSizeMax, label.fontSize);
                    label.fontSizeMin = Mathf.Min(label.fontSizeMax, 12f);
                    label.textWrappingMode = TextWrappingModes.Normal;
                }
        }

        public static string WorldLabel(string text) => WorldLabels.TryGetValue(text, out string translated) ? translated : text;
    }
}

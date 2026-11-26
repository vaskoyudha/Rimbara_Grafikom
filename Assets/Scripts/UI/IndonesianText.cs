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
            { "Menu story", "Hutan ini pernah terbakar. Tanahnya kini dingin, tetapi belum hidup.\nKamu datang bukan untuk berperang, melainkan untuk menanam.\n\nPadamkan api terakhir, tanam akar baru, dan beri Arunika alasan untuk bernapas lagi." },
            { "Menu scope", "5 BAB  /  3 ZONA  /  TANPA BATAS WAKTU" },
            { "Menu footer", "Setiap Pohon Menyimpan Cerita.\nRIMBARA  /  VERSI TIGA ZONA" },
            { "Start restoration label", "MULAI PEMULIHAN" },
            { "Controls button label", "CARA BERMAIN" },
            { "Menu quit label", "KELUAR" },
            { "Briefing eyebrow", "POSKO RELAWAN / HUTAN ARUNIKA" },
            { "Briefing title", "HARI PERTAMAMU" },
            { "Briefing story", "Api sudah lewat, tetapi hutan belum pulih. Dari posko ini, ikuti penanda amber untuk memeriksa kerusakan. Lalu bawa bibit asli kembali ke lahan tanam dan padamkan sisa api." },
            { "Briefing steps", "01  KEDATANGAN  /  Periksa hutan\n02  TANAM MASA DEPAN  /  Kumpulkan dan tanam bibit\n03  PADAMKAN API  /  Bidik dan tahan klik kiri\n04  KEMBALIKAN KEHIDUPAN  /  Bersihkan sungai dan habitat\n05  JANJI UNTUK HUTAN  /  Bibit terakhir dan titik pandang" },
            { "Briefing targets", "5 BIBIT ASLI  /  5 POHON  /  10 TITIK API" },
            { "No fail promise", "Tanpa batas waktu. Tanpa cedera. Tanpa batas air. Lakukan dengan tenang." },
            { "Enter Arunika label", "MASUK KE ARUNIKA" },
            { "Briefing back label", "KEMBALI" },
            { "Help eyebrow", "PANDUAN LAPANGAN" },
            { "Help title", "CARA BERMAIN" },
            { "Control list", "WASD  Bergerak\nMOUSE  Putar kamera\nSHIFT  Berlari\nSPASI  Melompat\nE  Bicara, ambil, gali, tanam, bersihkan\nKLIK KIRI  Tahan untuk menyiram api atau bibit\nESC  Jeda atau tutup dialog" },
            { "Help tip", "Ikuti penanda amber dan peta. Dekati objek untuk berinteraksi. Bidik api atau bibit sebelum menyiram; penghalang bisa menahan air." },
            { "Help back label", "KEMBALI KE MENU" },
            { "Controls text", "WASD gerak  /  SHIFT lari  /  SPASI lompat  /  E interaksi  /  KLIK KIRI air  /  ESC jeda" },
            { "Map title", "ARUNIKA / PETA" },
            { "Pause eyebrow", "TARIK NAPAS SEJENAK" },
            { "Pause title", "PERMAINAN DIJEDA" },
            { "Pause description", "Progresmu aman. Lanjutkan saat kamu siap." },
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

        public static List<string> Validate()
        {
            var problems = new List<string>();
            foreach (KeyValuePair<string, string> pair in Labels)
                if (string.IsNullOrWhiteSpace(pair.Value) && pair.Key != "Feedback text" && pair.Key != "Ending subtitle")
                    problems.Add(pair.Key + " is empty");
            if (!Labels["Menu scope"].Contains(GameScope.Chapters.ToString()))
                problems.Add("Menu scope chapter count mismatch");
            if (!Labels["Briefing steps"].Contains("05"))
                problems.Add("Briefing steps missing chapter 05");
            return problems;
        }
    }
}

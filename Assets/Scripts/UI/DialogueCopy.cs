using System.Collections.Generic;

namespace Rimbara.UI
{
    public static class DialogueCopy
    {
        public const string CoordinatorSpeaker = "KOORDINATOR MISI";
        public const string RangerSpeaker = "PETUGAS HUTAN";
        public const string ElderSpeaker = "TETUA DESA";
        public const string NarratorSpeaker = "NARATOR";
        public const string VillagerSpeaker = "WARGA DESA";

        public static readonly string[] OpeningSpeakers =
        {
            NarratorSpeaker,
            VillagerSpeaker,
            CoordinatorSpeaker,
            NarratorSpeaker
        };

        public static readonly string[] OpeningLines =
        {
            "Kebakaran hebat telah melanda Hutan Arunika. Yang tersisa kini hanya abu, batang hangus, dan titik api yang masih menolak padam.",
            "Asapnya belum benar-benar hilang di ujung hutan... Banyak warga terpaksa meninggalkan rumah, berharap ada yang datang membantu.",
            "Relawan baru sudah tiba? Bagus. Segera datangi posko darurat di depan sana—kita harus memadamkan sisa api sebelum menyebar lagi.",
            "Kamu datang bukan untuk berperang, melainkan untuk memulihkan. Setiap langkah kecil berarti. Setiap pohon menyimpan cerita."
        };

        public static string FormatOpeningSubtitle(int shotIndex)
        {
            int index = shotIndex < 0 ? 0 : (shotIndex >= OpeningLines.Length ? OpeningLines.Length - 1 : shotIndex);
            return "<color=#C99A3F><b>" + OpeningSpeakers[index] + "</b></color>\n" + OpeningLines[index];
        }

        public static readonly string[] CoordinatorBriefing =
        {
            "Selamat datang di posko darurat. Terima kasih sudah datang membantu.",
            "Api masih hidup di beberapa titik. Tugas pertamamu sederhana: padamkan semuanya.",
            "Ambil perlengkapan di samping posko. Dekati api bertanda, arahkan bidikan, lalu tahan KLIK KIRI. Setelah kawasan aman, kita mulai menanam."
        };

        public static readonly string[] CoordinatorDebrief =
        {
            "Lahan akhirnya bersih. Masih gersang, tetapi setiap upaya kecil membawa kita lebih dekat ke pemulihan.",
            "Setiap pemulihan dimulai dari satu langkah. Temui Petugas Hutan di persemaian. Ia akan membimbingmu menanam bibit dan memulihkan habitat."
        };

        public static readonly string[] RangerBriefing =
        {
            "Kamu sudah melakukan pekerjaan luar biasa. Api padam dan kawasan ini bersih. Namun hutan bukan sekadar kumpulan pohon; hutan adalah rumah bagi banyak makhluk hidup.",
            "Kita harus mengembalikan kehidupan itu. Ambil bibit di persemaian. Gali lubang pada lahan bertanda, tanam bibit, lalu siram setiap pohon muda. Beri mereka waktu untuk tumbuh."
        };

        public static string[] RangerPlantingDebrief(int wasteGoal) => new[]
        {
            "Setiap bibit yang kamu tanam adalah janji untuk masa depan. Pohon-pohon muda telah disiram. Rawat mereka dan beri waktu untuk tumbuh. Suatu hari, tanah ini akan kembali hijau.",
            "Sekarang kita perlu memulihkan sungai dan habitat. Kumpulkan " + wasteGoal + " sampah di kedua tepian sungai. Gunakan jembatan untuk menyeberang, lalu pulihkan tempat berlindung dan tanaman penutup pada habitat bertanda."
        };

        public static string[] RangerEcosystemDebrief(bool restoredExists) => new[]
        {
            "Lahan ini tidak lagi kosong. Pohon muda mulai tumbuh, air lebih bersih, dan tanda kehidupan satwa mulai muncul. Upayamu telah memberi hutan ini kesempatan kedua.",
            restoredExists
                ? "Pemulihan membutuhkan waktu dan perawatan bersama. Musim demi musim, hutan akan tumbuh. Suatu hari, kembalilah dan temui Tetua Desa untuk melihat harapan yang telah kita tanam."
                : "Zona Pemulihan selesai. Zona Hutan Pulih masih dalam pengembangan."
        };

        public static readonly string[] ElderBriefing =
        {
            "Lihat apa yang telah kita capai. Dulu hutan ini dipenuhi api dan abu. Desa kami diliputi ketakutan, dan kami khawatir akan kehilangan rumah ini.",
            "Kini pepohonan tumbuh kembali, burung-burung kembali, dan desa kami hidup lagi. Terima kasih. Kamu tidak hanya menyelamatkan hutan; kamu membantu menghidupkan kembali rumah kami.",
            "Bawalah bibit terakhir ini ke lahan pemulihan desa. Tanam dan siram sebagai janji untuk menjaga hutan. Setelah itu, datangi titik pandang dan lihat apa yang telah kita pulihkan bersama."
        };

        public static IEnumerable<string> AllPages(int wasteGoal)
        {
            foreach (string line in OpeningLines) yield return line;
            foreach (string page in CoordinatorBriefing) yield return page;
            foreach (string page in CoordinatorDebrief) yield return page;
            foreach (string page in RangerBriefing) yield return page;
            foreach (string page in RangerPlantingDebrief(wasteGoal)) yield return page;
            foreach (string page in RangerEcosystemDebrief(true)) yield return page;
            foreach (string page in RangerEcosystemDebrief(false)) yield return page;
            foreach (string page in ElderBriefing) yield return page;
        }
    }
}

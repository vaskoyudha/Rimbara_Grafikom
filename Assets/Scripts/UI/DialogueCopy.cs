using System.Collections.Generic;

namespace Rimbara.UI
{
    public readonly struct DialogueOption
    {
        public readonly string Prompt;
        public readonly string NpcReaction;

        public DialogueOption(string prompt, string npcReaction)
        {
            Prompt = prompt;
            NpcReaction = npcReaction;
        }
    }

    public static class DialogueCopy
    {
        public const string CoordinatorName = "BIMA PRATAMA";
        public const string CoordinatorSpeaker = "KOORDINATOR MISI";
        public static string CoordinatorHeader => CoordinatorName + "  <sprite name=\"diamond\" tint=1>  " + CoordinatorSpeaker;
        public const string RangerSpeaker = "PETUGAS HUTAN";
        public const string ElderSpeaker = "TETUA DESA";
        public const string NarratorSpeaker = "NARATOR";
        public const string VillagerSpeaker = "WARGA DESA";

        public static readonly string[] OpeningSpeakers =
        {
            NarratorSpeaker,
            VillagerSpeaker,
            CoordinatorName + "  —  " + CoordinatorSpeaker,
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
            "Selamat datang di posko darurat Arunika, Relawan. Udara di sini masih pekat oleh bau kayu hangus, dan warga menatap bukit dengan cemas. Terima kasih sudah datang saat hutan ini hampir kehabisan waktu.",
            "Api utama memang sudah mereda, tetapi bara masih bersembunyi di balik semak kering dan tunggul kayu. Jika angin malam bertiup kencang, hutan ini bisa terbakar lagi. Tugas pertamamu adalah memadamkan semua titik api yang tersisa.",
            "Ambil alat pemadam di samping posko, lalu dekati setiap titik api bertanda, arahkan bidikan, dan tahan KLIK KIRI hingga padam. Setelah kawasan ini aman dan puingnya bersih, barulah kita bisa menanam kehidupan baru."
        };

        public static readonly string[] CoordinatorDebrief =
        {
            "Lihatlah sekelilingmu—asap tebal itu akhirnya berhenti mengepul. Tanahnya masih kelabu dan gersang, tetapi untuk pertama kalinya sejak kebakaran, Arunika terasa tenang kembali.",
            "Memadamkan api hanyalah awal dari pemulihan. Sekarang saatnya menumbuhkan harapan baru. Temui Petugas Hutan di area persemaian; ia telah menyiapkan bibit asli Arunika dan menunggumu di sana."
        };

        public static readonly string[] RangerBriefing =
        {
            "Selamat datang di persemaian, Relawan. Bima sudah mengabarkan kerja kerasmu memadamkan sisa api. Namun hutan bukan sekadar deretan batang kayu—ia adalah rumah bagi burung, serangga, dan mata air desa.",
            "Kita harus mengembalikan denyut kehidupan itu perlahan-lahan. Ambil bibit di persemaian ini, gali lubang di lahan bertanda, tanam setiap bibit dengan hati-hati, lalu sirami agar akarnya kuat mencengkeram tanah."
        };

        public static string[] RangerPlantingDebrief(int wasteGoal) => new[]
        {
            "Barisan tunas hijau itu tampak kokoh di atas tanah yang dulu hangus. Setiap bibit yang kamu tanam dan sirami hari ini adalah janji bagi generasi mendatang bahwa Arunika akan kembali rimbun.",
            "Namun pekerjaan kita belum selesai—aliran sungai di lembah masih tersumbat sisa sampah dan puing. Kumpulkan " + wasteGoal + " sampah di kedua tepian sungai, seberangi jembatan, lalu pulihkan tempat berlindung satwa dan tanaman penutupnya."
        };

        public static string[] RangerEcosystemDebrief(bool restoredExists) => new[]
        {
            "Dengarkan suara airnya... sungai kembali mengalir jernih, tunas-tunas muda mulai meninggi, dan jejak satwa kecil mulai terlihat lagi di tepian. Kerja kerasmu memberi hutan ini kesempatan kedua.",
            restoredExists
                ? "Pemulihan sejati membutuhkan kesabaran dan kepedulian bersama. Musim demi musim akan menyembuhkan luka tanah ini. Temuilah Tetua Desa di kawasan yang telah pulih untuk melihat buah dari harapan yang kamu tanam."
                : "Zona Pemulihan telah selesai dengan baik. Zona Hutan Pulih masih dalam tahap pengembangan."
        };

        public static readonly string[] ElderBriefing =
        {
            "Lihatlah ke sekelilingmu, anak muda. Dulu langit desa kami tertutup jelaga, dan setiap malam kami tidur dalam ketakutan akan kehilangan tanah kelahiran ini. Kini kicau burung kembali terdengar di antara dedaunan.",
            "Pepohonan tumbuh kembali, udara terasa sejuk, dan anak-anak mulai berani bermain di tepi hutan lagi. Terima kasih—kamu bukan hanya menyelamatkan pepohonan, tetapi juga menghidupkan kembali harapan seluruh warga desa.",
            "Sebagai penutup perjalanan ini, bawalah bibit terakhir ini ke lahan pemulihan desa. Tanam dan sirami sebagai janji abadi untuk menjaga hutan, lalu naiklah ke titik pandang untuk menyaksikan apa yang telah kita pulihkan bersama."
        };

        public static readonly DialogueOption[][] CoordinatorBriefingOptions =
        {
            new[]
            {
                new DialogueOption("Saya siap membantu, Pak Bima. Bagaimana situasinya sekarang?", string.Empty),
                new DialogueOption("Bagaimana kondisi warga yang mengungsi di posko?", "Warga sudah kami evakuasi ke tenda aman di balik posko. Mereka terus menatap ke arah bukit—berharap api benar-benar padam sebelum angin malam turun.")
            },
            new[]
            {
                new DialogueOption("Di mana saya bisa mengambil perlengkapan pemadam?", string.Empty),
                new DialogueOption("Seberapa cepat bara yang tersisa bisa menyebar lagi?", "Dalam kondisi tanah sekering ini, satu hembusan angin saja bisa menyulut kobaran baru. Karena itu setiap titik api bertanda harus dipadamkan tuntas.")
            },
            new[]
            {
                new DialogueOption("Baik, saya ambil perlengkapan dan langsung bergerak!", string.Empty),
                new DialogueOption("Mengerti, saya akan pastikan semuanya padam.", string.Empty)
            }
        };

        public static readonly DialogueOption[][] CoordinatorDebriefOptions =
        {
            new[]
            {
                new DialogueOption("Syukurlah kawasan ini aman. Apa langkah kita berikutnya?", string.Empty),
                new DialogueOption("Apakah tanah yang hangus ini masih bisa ditanami?", "Lapisan abunya justru bisa menyuburkan tanah jika dibersihkan dengan benar. Petugas Hutan di persemaian sudah menyiapkan bibit asli Arunika untukmu.")
            },
            new[]
            {
                new DialogueOption("Baik, saya segera menemui Petugas Hutan.", string.Empty),
                new DialogueOption("Siap, saya menuju persemaian sekarang!", string.Empty)
            }
        };

        public static readonly DialogueOption[][] RangerBriefingOptions =
        {
            new[]
            {
                new DialogueOption("Apa yang paling dibutuhkan hutan ini agar pulih?", string.Empty),
                new DialogueOption("Apakah satwa liar akan kembali setelah pohon ditanam?", "Perlahan-lahan mereka pasti pulang. Saat tunas mulai rindang dan air bersih mengalir, burung dan satwa kecil akan berani kembali bersarang di sini.")
            },
            new[]
            {
                new DialogueOption("Saya ambil bibitnya dan mulai menanam sekarang!", string.Empty),
                new DialogueOption("Dimengerti, akan saya rawat setiap tunasnya.", string.Empty)
            }
        };

        public static readonly DialogueOption[][] RangerPlantingDebriefOptions =
        {
            new[]
            {
                new DialogueOption("Senang melihat bibitnya tumbuh. Apa tugas berikutnya?", string.Empty),
                new DialogueOption("Mengapa kebersihan sungai sangat penting bagi hutan?", "Sungai adalah nadi hutan ini. Jika alirannya tersumbat sampah dan puing, akar pohon muda takkan mendapat air bersih dan satwa takkan bertahan.")
            },
            new[]
            {
                new DialogueOption("Baik, saya bersihkan sungai dan pulihkan habitatnya!", string.Empty),
                new DialogueOption("Saya segera menyeberangi jembatan untuk bekerja.", string.Empty)
            }
        };

        public static readonly DialogueOption[][] RangerEcosystemDebriefOptions =
        {
            new[]
            {
                new DialogueOption("Lega rasanya melihat kehidupan mulai kembali di sini.", string.Empty),
                new DialogueOption("Apakah ekosistem Arunika sudah benar-benar stabil?", "Tanda-tandanya sangat baik—air mulai jernih dan burung sudah singgah. Namun hutan ini tetap butuh penjaga agar terus lestari dari musim ke musim.")
            },
            new[]
            {
                new DialogueOption("Saya akan menemui Tetua Desa sekarang.", string.Empty),
                new DialogueOption("Terima kasih atas bimbingannya selama ini.", string.Empty)
            }
        };

        public static readonly DialogueOption[][] ElderBriefingOptions =
        {
            new[]
            {
                new DialogueOption("Saya ikut bahagia melihat desa ini hidup kembali.", string.Empty),
                new DialogueOption("Bagaimana perasaan warga melihat hutan mulai menghijau?", "Air mata haru tak henti mengalir di desa kami. Kini warga kembali percaya bahwa alam selalu memberi kesempatan kedua bagi siapa saja yang mau merawatnya.")
            },
            new[]
            {
                new DialogueOption("Pemulihan ini adalah hasil kerja kita semua.", string.Empty),
                new DialogueOption("Apa pesan terakhir Tetua untuk menjaga hutan ini?", "Jangan pernah lupakan bau tanah basah setelah hujan pertama, dan ajarkan generasi berikutnya bahwa menjaga hutan berarti menjaga kehidupan kita sendiri.")
            },
            new[]
            {
                new DialogueOption("Saya akan menanam bibit terakhir itu sekarang.", string.Empty),
                new DialogueOption("Dengan penuh hormat, saya terima amanah ini.", string.Empty)
            }
        };

        public static readonly DialogueOption[] ReactionFollowUpOptions =
        {
            new DialogueOption("Saya mengerti. Mari lanjutkan pembicaraan.", string.Empty),
            new DialogueOption("Terima kasih penjelasannya, mari lanjut.", string.Empty)
        };

        public static readonly DialogueOption[] ReminderOptions =
        {
            new DialogueOption("Baik, saya segera laksanakan tugas ini.", string.Empty),
            new DialogueOption("Terima kasih atas pengingatnya!", string.Empty)
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
            foreach (DialogueOption[][] group in new[] { CoordinatorBriefingOptions, CoordinatorDebriefOptions, RangerBriefingOptions, RangerPlantingDebriefOptions, RangerEcosystemDebriefOptions, ElderBriefingOptions })
                foreach (DialogueOption[] row in group)
                    foreach (DialogueOption opt in row)
                    {
                        yield return opt.Prompt;
                        if (!string.IsNullOrEmpty(opt.NpcReaction)) yield return opt.NpcReaction;
                    }
            foreach (DialogueOption opt in ReactionFollowUpOptions) yield return opt.Prompt;
            foreach (DialogueOption opt in ReminderOptions) yield return opt.Prompt;
        }
    }
}

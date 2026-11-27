using Rimbara.Mission;
using TMPro;
using UnityEngine;

namespace Rimbara.UI
{
    public class TasRelawanUI : MonoBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private TMP_Text statusAlat;
        [SerializeField] private TMP_Text jumlahBibit;
        [SerializeField] private TMP_Text statusBibitTerakhir;

        private void OnEnable()
        {
            if (chapter == null || statusAlat == null ||
                jumlahBibit == null || statusBibitTerakhir == null)
            {
                Debug.LogWarning("Tas Relawan: periksa empat kolom di Inspector.", this);
                return;
            }

            var recovery = chapter.Recovery?.Progress;
            var restored = chapter.Recovery?.Restored?.Progress;

            statusAlat.text = chapter.Progress?.HasEquipment == true
                ? "Alat pemadam: Dibawa"
                : "Alat pemadam: Belum diambil";

            jumlahBibit.text = $"Bibit pohon: {recovery?.Carried ?? 0}";

            if (restored?.Planted == true)
                statusBibitTerakhir.text = "Bibit terakhir: Sudah ditanam";
            else if (restored?.HasSeedling == true)
                statusBibitTerakhir.text = "Bibit terakhir: Dibawa";
            else
                statusBibitTerakhir.text = "Bibit terakhir: Belum diterima";
        }
    }
}
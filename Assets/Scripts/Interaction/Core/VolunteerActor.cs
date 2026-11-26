using System;
using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class VolunteerActor : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [SerializeField] private MissionSystem missions;
        private static readonly int PickupTrigger = Animator.StringToHash("AmbilAlat");
        private static readonly int PickupState = Animator.StringToHash("Base Layer.Ambil Alat");
        private Animator animator;
        private EquipmentPickup alatYangDiraih;
        private RecoveryPlantingPoint lahanYangDitanam;
        public bool SedangMenanam => lahanYangDitanam != null;
        private float requestedAt;
        public GameState State => state;
        public MissionSystem Missions => missions;
        public event Action<InteractionResult> ActionPerformed;

        public void Configure(GameState gameState, MissionSystem missionSystem)
        {
            state = gameState;
            missions = missionSystem;
        }
        private void Awake() { animator = GetComponent<Animator>(); }

        private bool InPickupAnimation => animator.GetCurrentAnimatorStateInfo(0).fullPathHash == PickupState
            || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).fullPathHash == PickupState);

        public bool MulaiAmbilAlat(EquipmentPickup alat)
        {
            if (alat == null || alatYangDiraih != null || animator == null || !animator.isActiveAndEnabled
                || !animator.HasState(0, PickupState) || InPickupAnimation) return false;

            alatYangDiraih = alat;
            requestedAt = Time.time;
            animator.SetTrigger(PickupTrigger);
            return true;
        }

        public void GenggamAlat()
        {
            if (alatYangDiraih == null) return;

            EquipmentPickup alat = alatYangDiraih;
            alatYangDiraih = null;
            Report(alat.SelesaikanPengambilan(this));
        }
        public bool MulaiTanamBibit(RecoveryPlantingPoint lahan)
        {
            if (lahan == null || SedangMenanam || alatYangDiraih != null ||
                animator == null || !animator.isActiveAndEnabled ||
                animator.IsInTransition(0) ||
                !animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle - walk - run") ||
                !animator.HasState(0, Animator.StringToHash("Base Layer.Tanam Bibit")))
                return false;

            lahanYangDitanam = lahan;
            animator.SetTrigger("TanamBibit");
            return true;
        }

        public void AkhiriAnimasiTanam(bool selesai)
        {
            var lahan = lahanYangDitanam;
            lahanYangDitanam = null;

            if (selesai && lahan != null)
                Report(lahan.SelesaikanPenanaman(this));
        }
        // An interrupted clip has no grasp event. Release its request so E can retry.
        private void LateUpdate()
        {
            if (alatYangDiraih != null && state.SessionRunning && Time.time - requestedAt > 0.6f
                && (!animator.isActiveAndEnabled || !InPickupAnimation)) alatYangDiraih = null;
        }

        private void OnDisable()
        {
            alatYangDiraih = null;
            lahanYangDitanam = null;
        }
        public void Report(InteractionResult result) { ActionPerformed?.Invoke(result); }
    }
}

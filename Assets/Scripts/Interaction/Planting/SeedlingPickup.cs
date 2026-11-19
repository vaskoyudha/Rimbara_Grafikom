using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class SeedlingPickup : InteractableBehaviour
    {
        [SerializeField] private GameObject seedlingVisual;
        [SerializeField] private int slot;
        public bool Collected { get; private set; }
        public int Slot => slot;
        public override bool Available => !Collected && gameObject.activeInHierarchy;

        public void Configure(string id, GameObject visual, GameObject ring, int index)
        {
            SetIdentity(id, ring);
            seedlingVisual = visual;
            slot = index;
        }

        public override string GetPrompt(VolunteerActor actor)
        {
            return actor.Missions.CanCollectOrPlant ? "[E]  Ambil bibit pohon lokal"
                : "Periksa penanda kuning di hutan terlebih dahulu";
        }

        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Dekati bibit pohon.");
            if (!actor.Missions.CanCollectOrPlant)
                return new InteractionResult(false, "Ikuti misi aktif sebelum mengambil bibit.");
            if (!actor.State.CollectSeedling(objectId)) return new InteractionResult(false, "Bibit ini sudah diambil.");
            Collected = true;
            if (seedlingVisual != null) seedlingVisual.SetActive(false);
            SetFocused(false);
            foreach (Collider collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            Debug.Log("RIMBARA: seedling collected (" + objectId + ").");
            return new InteractionResult(true, "Bibit pohon lokal berhasil diambil. Bawa ke lahan tanam bertanda.");
        }
    }
}

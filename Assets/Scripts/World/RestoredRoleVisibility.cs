using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.World
{
    public sealed class RestoredRoleVisibility : MonoBehaviour
    {
        private RestoredChapter chapter;
        private Renderer[] renderers;
        private void Start() { chapter = FindFirstObjectByType<RestoredChapter>(); renderers = GetComponentsInChildren<Renderer>(true); }
        private void Update()
        {
            foreach (Renderer r in renderers)
                if (!r.transform.name.StartsWith("Label - ")) r.enabled = chapter.EnvironmentReady;
            // The interaction root is unchanged; only its visual child is replaced.
            HumanNpcVisual human = GetComponentInChildren<HumanNpcVisual>(true);
            Transform model = human != null ? human.transform : transform.Find("Elder visual - existing character placeholder");
            if (model != null) model.gameObject.SetActive(chapter.EnvironmentReady);
        }
    }
}

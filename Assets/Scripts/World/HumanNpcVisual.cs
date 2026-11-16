using UnityEngine;

namespace Rimbara.World
{
    // Visual identity only. Mission/interaction components stay on their original roots.
    public sealed class HumanNpcVisual : MonoBehaviour
    {
        [SerializeField] private string characterId;
        public string CharacterId => characterId;
        public void Configure(string id) { characterId = id; }
    }
}

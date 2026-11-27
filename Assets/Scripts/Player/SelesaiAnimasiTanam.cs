using Rimbara.Interaction;
using UnityEngine;

public class SelesaiAnimasiTanam : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (layerIndex != 0) return;

        var actor = animator.GetComponent<VolunteerActor>();
        if (actor != null)
            actor.AkhiriAnimasiTanam(stateInfo.normalizedTime >= 1f);
    }
}
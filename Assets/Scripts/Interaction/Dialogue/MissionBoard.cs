namespace Rimbara.Interaction
{
    public sealed class MissionBoard : InteractableBehaviour
    {
        public override bool Available => true;
        public override string GetPrompt(VolunteerActor actor) => "[E]  Baca pengarahan relawan";

        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!InRange(actor)) return new InteractionResult(false, "Dekati papan misi.");
            if (actor.Missions.DisasterChapter != null)
                return new InteractionResult(true, actor.Missions.DisasterChapter.Title + ": " + actor.Missions.DisasterChapter.Instruction);
            return new InteractionResult(true, actor.Missions.Title + ": " + actor.Missions.Instruction);
        }
    }
}

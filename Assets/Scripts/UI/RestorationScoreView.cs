using Rimbara.Mission;

namespace Rimbara.UI
{
    // View-model handed to the HUD. It carries only values and authored copy,
    // so the HUD never reads GameState/MissionSystem directly for counters or
    // the mission banner.
    public sealed class RestorationScoreView
    {
        public RestorationScore Score { get; }
        public string MissionTitle { get; }
        public string MissionInstruction { get; }
        public float ElapsedSeconds { get; }

        public int MissionNumber => Score.Mission;
        public int MissionCount => RestorationScore.MissionCount;
        public int ChapterCount => GameScope.Chapters;

        public RestorationScoreView(RestorationScore score, string missionTitle, string missionInstruction, float elapsedSeconds)
        {
            Score = score;
            MissionTitle = missionTitle ?? string.Empty;
            MissionInstruction = missionInstruction ?? string.Empty;
            ElapsedSeconds = elapsedSeconds < 0f ? 0f : elapsedSeconds;
        }

        public static RestorationScoreView From(ProgressSnapshot snapshot, MissionStage stage,
            string missionTitle, string missionInstruction, float elapsedSeconds = 0f)
        {
            RestorationScore score = RestorationScore.From(snapshot, RestorationScore.MissionNumberFor(stage));
            return new RestorationScoreView(score, missionTitle, missionInstruction, elapsedSeconds);
        }
    }
}

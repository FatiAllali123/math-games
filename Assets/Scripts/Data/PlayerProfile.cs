using System.Collections.Generic;

[System.Serializable]
public class PlayerProfile
{
    public string playerName;
    public GradeLevel gameLevel;
    public int coins;
    public GradeLevel schoolGrade;
    public int mathLevel;
    public int questionsSolved;
    public List<string> skillsToImprove;
    public RewardData rewardProfile;
    public Dictionary<string, GameProgressEntry> gameProgress;
    public AchievementData achievements;
}

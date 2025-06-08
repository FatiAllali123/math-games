using System.Collections.Generic;

[System.Serializable]
public class UserData
{
    public string uid;
    public string role;
    public string firstName;
    public string lastName;
    public string birthday;
    public string gender;
    public GradeLevel schoolGrade;
    public string linkedTeacherId;
    public PlayerProfile playerProfile;

    // Ces propri�t�s doivent �tre au niveau UserData selon  structure Firebase
    public Dictionary<string, GameProgressEntry> gameProgress;
    public AchievementData achievements;
}
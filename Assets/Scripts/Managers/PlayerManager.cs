using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public PlayerProfile playerProfile;

    private void Awake()
    {
        Instance = this;
        playerProfile = new PlayerProfile { schoolGrade = GradeLevel.One };
    }

    public PlayerProfile GetPlayerState()
    {
        return playerProfile;
    }
}

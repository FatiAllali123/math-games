using UnityEngine;

public class Buttontransition : MonoBehaviour
{
    public void OnMyButtonClick()
    {
        SceneLoader.Instance.LoadSceneWithTransition("TestRoad"); // nom de la scène cible

      
    }
}
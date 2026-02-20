using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition")]
    public string transition;

    public void NextScene()
    {
        SceneTransitionManager.LoadScene(transition);
    }
}

using UnityEngine;

[System.Serializable]
public class TutorialPanelPair : MonoBehaviour
{
    public string tutorialId;
    public GameObject panelA;
    public GameObject panelB;

    public void SetActive(bool value)
    {
        if (panelA) panelA.SetActive(value);
        if (panelB) panelB.SetActive(value);
    }
}

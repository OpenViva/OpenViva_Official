using System.Collections.Generic;
using UnityEngine;

public class TutorialMenuManager : MonoBehaviour
{
    [Header("Root Menu")]
    [SerializeField] private GameObject rootMenu;

    [Header("Back Button")]
    [SerializeField] private GameObject backButton;

    [Header("Tutorial Panels")]
    [SerializeField] private TutorialPanelPair[] tutorials;

    private Dictionary<string, TutorialPanelPair> tutorialLookup;

    private void Awake()
    {
        // Build lookup table for fast access
        tutorialLookup = new Dictionary<string, TutorialPanelPair>();

        foreach (var tutorial in tutorials)
        {
            tutorialLookup.Add(tutorial.tutorialId, tutorial);
            tutorial.SetActive(false);
        }

        OpenRootMenu();
    }

    public void OpenTutorial(string tutorialId)
    {
        CloseAllTutorials();

        if (tutorialLookup.TryGetValue(tutorialId, out var tutorial))
        {
            rootMenu.SetActive(false);
            tutorial.SetActive(true);

            backButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Tutorial ID not found: {tutorialId}");
        }
    }

    public void OpenRootMenu()
    {
        CloseAllTutorials();
        rootMenu.SetActive(true);

        backButton.SetActive(false);
    }

    private void CloseAllTutorials()
    {
        foreach (var tutorial in tutorials)
        {
            tutorial.SetActive(false);
        }
    }
}

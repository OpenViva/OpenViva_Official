using UnityEngine;

public enum VoicePack
{
    Female,
    Male
}

[ExecuteInEditMode]
public class VivaDescriptor : MonoBehaviour
{
    [Header("Character Information")]
    [Tooltip("Name of the model")]
    public string Name = "Fox Girl 9000";

    [Tooltip("Who made this model?")]
    public string AuthorName = "Unknown";

    [Tooltip("Version, usually the amount of times you build the character")]
    public int Version = 1;

    [Header("Personality")]
    [Tooltip("The personality type in a number (0 = default)")]
    public int PersonalityType = 0;

    public VoicePack VoicePack;

    [Header("Optional Info")]
    [Tooltip("Optional description")]
    public string Description = "";

    [Tooltip("Tags for categorization (comma-separated)")]
    public string Tags = "";

    [Tooltip("Custom color for UI highlighting")]
    public Color CustomColor = Color.orange;

    [Header("Unused Values")]
    public int Aux1;
    public int Aux2;
    public int Aux3;
    public string AuxString1;
    public string AuxString2;
    public string AuxString3;

    private void OnValidate()
    {
        if (PersonalityType < 0)
        {
            Debug.LogError("[Viva Descriptor] Personality cannot be negative!");
        }

        if (Version <= 0)
        {
            Debug.LogError("[Viva Descriptor] Version cannot be 0 or negative!");
        }
    }

    public void IncrementVersion()
    {
        Version++;
    }

    public void Reset()
    {
        Name = "Fox Girl #9000";
        AuthorName = "Unknown";
        Version = 1;
        PersonalityType = 0;
        Description = "Return to hot spring when found.";
        Tags = "fox, girl";
    }

    public string GetSummary()
    {
        return $"[{Name}] v{Version} by {AuthorName} - Personality: {PersonalityType}";
    }
}

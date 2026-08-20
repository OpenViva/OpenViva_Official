using UnityEngine;

public class LinkOpener : MonoBehaviour
{
    public void OpenURL(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }
}

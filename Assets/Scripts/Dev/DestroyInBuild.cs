using UnityEngine;

public class DestroyInBuild : MonoBehaviour
{
    private void Awake()
    {
        if (!Application.isEditor)
        {
            Destroy(gameObject);
        }
    }
}

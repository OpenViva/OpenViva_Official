using System.Collections;
using UnityEngine;

public class CropColorChange : MonoBehaviour
{
    private bool _waiting = false;

    private void Start()
    {
        StartCoroutine(ChangeColor());
        _waiting = true;
    }

    private void Update()
    {
        if (!_waiting)
        {
            StartCoroutine(ChangeColor());
            _waiting = true;
        }
    }

    private IEnumerator ChangeColor()
    {
        yield return new WaitForSeconds(2f);
        if (TryGetComponent(out Renderer r))
        {
            r.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
        _waiting = false;
    }
}

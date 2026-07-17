using UnityEngine;

public class FruitPiece : MonoBehaviour
{
    public FruitCutMinigame.Fruit Fruit;
    [SerializeField] private float _price;

    private bool _didOnce;

    public void SetPrice(float price)
    {
        if (_didOnce) return;
        _price *= price;
    }
}

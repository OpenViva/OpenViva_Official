using UnityEngine;

public class JamOnToast : MonoBehaviour
{
    [SerializeField] private GameObject _jam;
    private bool _didOnce = false;
    private Meal _mealScript;

    private void Awake()
    {
        _mealScript = GetComponent<Meal>();
    }

    public bool TrySpreadJam(FruitCutMinigame.Fruit fruit)
    {
        if (_didOnce) { return false; }

        if (_jam.TryGetComponent(out Renderer renderer))
        {
            switch (fruit)
            {
                case FruitCutMinigame.Fruit.Strawberry: 
                    renderer.material.color = Color.firebrick;
                    _mealScript.IncreasePrice(6f);
                    break;

                case FruitCutMinigame.Fruit.Peach: 
                    renderer.material.color = Color.sandyBrown;
                    _mealScript.IncreasePrice(3.50f);
                    break;

                case FruitCutMinigame.Fruit.Cantaloupe: 
                    renderer.material.color = Color.orangeRed;
                    _mealScript.IncreasePrice(8f);
                    break;

                default: 
                    renderer.material.color = Color.tomato;
                    _mealScript.IncreasePrice(11.50f);
                    break;
            }
            _jam.SetActive(true);
            _didOnce = true;
            return true;
        }
        return false;
    }
}

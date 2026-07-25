using UnityEngine;

public class BowlLogic : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _flourBlendShape;
    private float _flourVolume = 0;
    private float _maxFlourVolume = 2100; // recipe needs 300

    [SerializeField] private SkinnedMeshRenderer _waterBlendShape;
    private float _waterVolume = 0;
    private float _maxWaterVolume = 1400; // recipe needs 200

    [SerializeField] private SkinnedMeshRenderer _batterBlendShape;
    private float _batterVolume = 0;
    private float _maxBatterVolume = 3500;

    private Player _player;
    private HintManager _hud;
    private PlayerKB_GrabObject _grabScript;
    [SerializeField] GameObject _doughPrefab;

    private int _strawberryPieces;
    private int _peachPieces;
    private int _cantaloupePieces;
    private int _blueberryPieces;

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        _hud = GameObject.Find("HUD").GetComponent<HintManager>();
        _grabScript = GetComponent<PlayerKB_GrabObject>();

        AssignInputs();
    }

    private void AssignInputs()
    {
        _grabScript.OnGrabbedLeft += BowlGrabbedLeft;
        _grabScript.OnGrabbedRight += BowlGrabbedRight;

        _player.Controls.Viva.InteractLeft.performed += context => CreateDough(false);
        _player.Controls.Viva.InteractRight.performed += context => CreateDough(true);
    }

    private void BowlGrabbedLeft(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.LeftGrabDoughHint); }
        else { _hud.ClearHint(HintConstants.LeftGrabDoughHint); }

        CuttingBoard.Instance.LastTouchedBowl = this;
    }

    private void BowlGrabbedRight(bool show)
    {
        if (show) { _hud.CreateHint(HintConstants.RightGrabDoughHint); }
        else { _hud.ClearHint(HintConstants.RightGrabDoughHint); }

        CuttingBoard.Instance.LastTouchedBowl = this;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.name.Equals("FlourParticle"))
        {
            if (_flourVolume >= _maxFlourVolume) { return; }
            _flourVolume += other.GetComponentInParent<MortarLogic>().GetToSpill() * 100;
            SetFlourBlend();
        }
        else if (other.gameObject.name.Equals("TapFX"))
        {
            if (_waterVolume >= _maxWaterVolume) { return; }
            _waterVolume += 50f;
            SetWaterBlend();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.TryGetComponent(out FruitPiece pieceScript))
        //{
        //    switch (pieceScript.Fruit)
        //    {
        //        case FruitCutMinigame.Fruit.Strawberry: _strawberryPieces += 1; break;
        //        case FruitCutMinigame.Fruit.Peach: _peachPieces += 1; break;
        //        case FruitCutMinigame.Fruit.Cantaloupe: _cantaloupePieces += 1; break;
        //        default: Debug.LogWarning($"Fruit not recognized: {pieceScript.Fruit}"); return;
        //    }

        //    Destroy(pieceScript.gameObject);
        //}
    }

    private void SetFlourBlend()
    {
        if (_flourVolume <= 0)
        {
            _flourBlendShape.SetBlendShapeWeight(0, 0);
            _flourBlendShape.enabled = false;
            return;
        }
        else { _flourBlendShape.enabled = true; }
        
        float percent = _flourVolume / 3500;
        _flourBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetWaterBlend()
    {
        if (_waterVolume <= 0)
        {
            _waterBlendShape.SetBlendShapeWeight(0, 0);
            _waterBlendShape.enabled = false;
            return;
        }
        else { _waterBlendShape.enabled = true; }

        float percent = _waterVolume / 3500;
        _waterBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetBatterBlend()
    {
        if (_batterVolume <= 0)
        {
            _batterBlendShape.SetBlendShapeWeight(0, 0);
            _batterBlendShape.enabled = false;
            return;
        }
        else { _batterBlendShape.enabled = true; }

        float percent = _batterVolume / 3500;
        _batterBlendShape.SetBlendShapeWeight(0, percent * 100);
    }

    private void SetAllBlends()
    {
        SetFlourBlend();
        SetWaterBlend();
        SetBatterBlend();
    }

    public void MixBatter()
    {
        if (_flourVolume <= 0 || _waterVolume <= 0 || _batterVolume >= _maxBatterVolume) { return; }

        _flourVolume -= 0.6f * 0.5f;
        _waterVolume -= 0.4f * 0.5f;
        _batterVolume += 0.5f;

        SetAllBlends();
    }

    private void CreateDough(bool useLeft)
    {
        if (_batterVolume < 250) { return; }
        else if (useLeft && (_grabScript.IsGrabbedLeft || !_grabScript.IsGrabbedRight)) { return; }
        else if (!useLeft && (_grabScript.IsGrabbedRight ||  !_grabScript.IsGrabbedLeft)) { return; }

        GameObject other;
        if (useLeft) { other = PlayerManager.Instance.GetObjectLeft(); }
        else { other = PlayerManager.Instance.GetObjectRight(); }
        // Other hand must be empty.
        if (other != null) { return; }

        int handedness;
        if (useLeft) { handedness = 1; }
        else { handedness = 2; }

        GameObject newDough = Instantiate(_doughPrefab);
        newDough.name = _doughPrefab.name;
        newDough.SetActive(false);
        if (newDough.TryGetComponent(out PlayerKB_GrabObject grabScript))
        {
            grabScript.SetIsActive(true, handedness);
            _batterVolume -= 250;
        }
        else
        {
            Debug.Log("A 'PlayerKB_GrabObject' script has not been added to the dough prefab.");
        }
    }

    public void CollectFruitPieces(FruitCutMinigame.Fruit fruit)
    {
        switch (fruit)
        {
            case FruitCutMinigame.Fruit.Strawberry: _strawberryPieces += 2; break;
            case FruitCutMinigame.Fruit.Peach: _peachPieces += 8; break;
            case FruitCutMinigame.Fruit.Cantaloupe: _cantaloupePieces += 20; break;
        }
    }

    private void OnDestroy()
    {
        _grabScript.OnGrabbedLeft -= BowlGrabbedLeft;
        _grabScript.OnGrabbedRight -= BowlGrabbedRight;
    }
}

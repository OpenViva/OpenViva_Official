using System.Collections.Generic;
using UnityEngine;

namespace NaughtyAttributes.Test
{
    //[CreateAssetMenu(fileName = "NaughtyScriptableObject", menuName = "NaughtyAttributes/TestNaughtyScriptableObject")]
    public class TestNaughtyScriptableObject : ScriptableObject
    {
        [Expandable]
        public List<TestScriptableObjectA> listA;
    }
}

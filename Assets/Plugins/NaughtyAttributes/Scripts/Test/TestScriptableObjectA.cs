using UnityEngine;
using System.Collections.Generic;

namespace NaughtyAttributes.Test
{
    //[CreateAssetMenu(fileName = "TestScriptableObjectA", menuName = "NaughtyAttributes/TestScriptableObjectA")]
    public class TestScriptableObjectA : ScriptableObject
    {
        [Expandable]
        public List<TestScriptableObjectB> listB;
    }
}
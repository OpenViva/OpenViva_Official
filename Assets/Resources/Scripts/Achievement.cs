using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Viva
{
    public partial class Achievement : VivaSessionAsset
    {
        public string name;

        public Achievement(string newName)
        {
            this.name = newName;
        }

        public void Trigger()
        {

        }
    }
}
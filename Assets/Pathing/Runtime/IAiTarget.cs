using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public interface IAiTarget
    {
        public void SetPosition(Vector3 pos);

        public Vector3 GetPosition();
    }
}
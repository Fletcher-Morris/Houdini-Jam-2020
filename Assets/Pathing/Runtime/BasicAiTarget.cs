using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public class BasicAiTarget : MonoBehaviour, IAiTarget
    {
        Vector3 IAiTarget.GetPosition()
        {
            return transform.position;
        }

        void IAiTarget.SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }
    }
}
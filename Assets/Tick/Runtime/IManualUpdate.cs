using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tick
{
    public interface IManualUpdate
    {
        public void AddToUpdateList();

        public void OnInitialise();

        public void OnManualUpdate(float delta);

        public void OnTick(float delta);

        public void OnManualFixedUpdate(float delta);
    }
}
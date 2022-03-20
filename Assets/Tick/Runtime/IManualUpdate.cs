using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tick
{
    public interface IManualUpdate
    {
        public UpdateManager GetUpdateManager();

        public void OnInitialise();

        public void OnManualUpdate(float delta);

        public void OnTick(float delta);

        public void OnManualFixedUpdate(float delta);

        public bool IsEnabled();
    }
}
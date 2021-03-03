using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    public interface IButtonHandler
    {
        void Initialize(GlowingButton button);
        void Teardown();
        void Update();
    }
}

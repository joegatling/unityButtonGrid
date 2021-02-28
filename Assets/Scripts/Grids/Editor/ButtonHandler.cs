using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grids
{
    public interface IButtonHandler
    {
        void Initialize(GlowingButton button);
        void Teardown();
        void Update();
    }
}

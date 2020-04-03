using System;
using System.Collections.Generic;
using System.Text;

namespace tester.Support
{
    internal class Logic
    {
        internal void Process(LightModel[] inputs, LightModel[] outputs)
        {
            for (int i = 0; i < 4; ++i)
                outputs[i].Active = inputs[i].Active;
        }
    }
}

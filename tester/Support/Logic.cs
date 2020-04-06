using System;
using System.Collections.Generic;
using System.Text;

namespace tester.Support
{
    internal class Logic
    {
        Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim, TimeSpan Delay)> Transitions =
            new Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinima, int ContorMaxim, TimeSpan Delay)>
            {
                //initializare
                [(0, -1)] = (0, -1, -1, TimeSpan.Zero),
                [(0, 0b1100)] = (1, -1, -1, TimeSpan.Zero),//stadiul initial -1 nu-i niciodata
                [(1, 0b1100)] = (1, 0, 5, TimeSpan.Zero),                //ramane in stadiul curent
                [(1, 0b0100)] = (2, 0, 5, TimeSpan.Zero),                 //trece in pasul urmator
                [(2, 0b0100)] = (2, 0, 5, TimeSpan.Zero),
                [(2, 0b0000)] = (3, 0, 5, TimeSpan.Zero),
                [(3, 0b0000)] = (3, 0, 5, TimeSpan.Zero),
                [(3, 0b0010)] = (4, 0, 5, TimeSpan.Zero),
                [(4, 0b0010)] = (4, 0, 5, TimeSpan.Zero),
                [(4, 0b0011)] = (5, 0, 5, TimeSpan.Zero),
                [(5, 0b0011)] = (5, 0, 5, TimeSpan.Zero),
                [(5, 0b0010)] = (6, 3, 5, TimeSpan.FromSeconds(5)),        //delay
                [(6, 0b0010)] = (6, 0, 5, TimeSpan.Zero),
                [(6, 0b0000)] = (7, 0, 5, TimeSpan.Zero),
                [(7, 0b0000)] = (7, 0, 5, TimeSpan.Zero),
                [(7, 0b0100)] = (8, 0, 5, TimeSpan.Zero),
                [(8, 0b0100)] = (8, 0, 5, TimeSpan.Zero),
                [(8, 0b1100)] = (9, 0, 5, TimeSpan.Zero),
                [(9, 0b1100)] = (9, 0, 5, TimeSpan.Zero),
                [(9, 0b0100)] = (1, 3, 5, TimeSpan.FromSeconds(5)),
            };
        int Starea;

        internal void Process(LightModel[] inputs, LightModel[] outputs)
        {
            // 1. Convertim dintr-un sir de LightModel in int Conditie
            //    (pentru ca sa-l folosim in dictionarul de tranzitii)
            int conditie = Convert.ToInt32(inputs[0].Active) * 2 * 2 * 2 +
                Convert.ToInt32(inputs[1].Active) * 2 * 2 +
                Convert.ToInt32(inputs[2].Active) * 2 + Convert.ToInt32(inputs[3].Active);

            // 2. Vrem sa stim care este starea urmatoare. Ne trebuie starea, conditia si dictionarul
            if (!Transitions.TryGetValue((Starea, conditie), out var val))
                val = Transitions[(Starea, -1)];
            int StareaUrmatoare = val.StareaUrmatoare;

            // 3. Trecem in starea urmatoare
            Starea = StareaUrmatoare;

            for (int i = 0; i < 4; ++i)
                outputs[i + 3].Active = inputs[i].Active;

            // outputs[3].Active = inputs[0].Active;
            // outputs[4].Active = inputs[1].Active;
            // outputs[5].Active = inputs[2].Active;
            // outputs[6].Active = inputs[3].Active;
            // outputs[5].Active = true;


            //if (inputs[0].Active && inputs[1].Active && !inputs[2].Active && !inputs[3].Active)
            //{
            //    outputs[0].Active = true;
            //    outputs[7].Active = true;
            //}
            //int state;
        }
    }
}

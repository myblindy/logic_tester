using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

public class Logic : ReactiveObject
{
    static readonly Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim, TimeSpan Delay)> Transitions =
        new Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim, TimeSpan Delay)>
        {
            //initializare
            [(0, -1)] = (0, -1, -1, TimeSpan.Zero),           //stadiul initial -1 nu-i niciodata
            [(0, 0b0011)] = (1, -1, -1, TimeSpan.Zero),       //stadiul initial -1 nu-i niciodata
            [(1, 0b0011)] = (1, 0, 5, TimeSpan.Zero),         //ramane in stadiul curent
            [(1, 0b0010)] = (2, 0, 5, TimeSpan.FromSeconds(5)),         //trece in pasul urmator
            [(2, 0b0010)] = (2, 0, 5, TimeSpan.Zero),
            [(2, 0b0000)] = (3, 0, 5, TimeSpan.Zero),
            [(3, 0b0000)] = (3, 0, 5, TimeSpan.Zero),
            [(3, 0b0100)] = (4, 0, 5, TimeSpan.Zero),
            [(4, 0b0100)] = (4, 0, 5, TimeSpan.Zero),
            [(4, 0b1100)] = (5, 0, 5, TimeSpan.Zero),
            [(5, 0b1100)] = (5, 0, 5, TimeSpan.Zero),
            [(5, 0b0100)] = (6, 3, 5, TimeSpan.FromSeconds(5)),        //delay
            [(6, 0b0100)] = (6, 0, 5, TimeSpan.Zero),
            [(6, 0b0000)] = (7, 0, 5, TimeSpan.Zero),
            [(7, 0b0000)] = (7, 0, 5, TimeSpan.Zero),
            [(7, 0b0010)] = (8, 0, 5, TimeSpan.Zero),
            [(8, 0b0010)] = (8, 0, 5, TimeSpan.Zero),
            [(8, 0b0011)] = (1, 0, 5, TimeSpan.Zero),
            [(-1, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(1, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(2, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(3, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(4, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(5, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(6, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(7, -1)] = (-1, -1, -1, TimeSpan.Zero),
            [(8, -1)] = (-1, -1, -1, TimeSpan.Zero),
        };

    static readonly Dictionary<int, (int Outputs, int Region)> StateOutputs = new Dictionary<int, (int, int)>
    {
        [0] = (0, 0),
        [1] = (0b000000010011001, 1),
        [2] = (0b000000100010000, 2),
        [3] = (0b000001000000000, 3),
        [4] = (0b000010000100000, 4),
        [5] = (0b000100001100010, 5),
        [6] = (0b001000000100000, 4),
        [7] = (0b010000000000000, 3),
        [8] = (0b100000000010000, 2),
        [-1] = (0b000000000000100, -1),
    };

    private readonly ILight[] Inputs;
    private readonly ILight[] Outputs;

    int starea;
    public int Starea { get => starea; set => this.RaiseAndSetIfChanged(ref starea, value); }

    public Logic(ILight[] inputs, ILight[] outputs)
    {
        Inputs = inputs;
        Outputs = outputs;
    }

    public void Reset()
    {
        Starea = 0;
    }

    public void Process()
    {
        //ca sa ramana LEDs in starea care au generat eroarea
        if (Starea != -1)
            for (int i = 0; i < 4; ++i)
                Outputs[i + 3].Active = Inputs[i].Active;

        // 1. Convertim dintr-un sir de LightModel in int Conditie
        //    (pentru ca sa-l folosim in dictionarul de tranzitii)
        int conditie = Convert.ToInt32(Inputs[0].Active) +
            Convert.ToInt32(Inputs[1].Active) * 2 +
            Convert.ToInt32(Inputs[2].Active) * 2 * 2 + Convert.ToInt32(Inputs[3].Active) * 2 * 2 * 2;

        // 2. Vrem sa stim care este starea urmatoare. Ne trebuie starea, conditia si dictionarul
        if (!Transitions.TryGetValue((Starea, conditie), out var val))
            val = Transitions[(Starea, -1)];
        int StareaUrmatoare = val.StareaUrmatoare;

        // 3. Trecem in starea urmatoare
        Starea = StareaUrmatoare;

        // 4. Cataum in dictionar stare ouputs corespunzatoare starii actuale
        var (TargetOutputs, _) = StateOutputs[Starea];

        // 5. Scriem in outputs valoarea output din starea curenta
        for (int i = 0; i < Outputs.Length; ++i)
        {
            //Intrarile sunt deja copiate. In starea -1 nu mai copiem altele.
            //Daca este mai mic ca 3 sau mai mare ca sase conditia este adevarata atunci 
            if (i < 3 || i > 6)
                Outputs[i].Active = (TargetOutputs & (1 << i)) != 0;
        }
    }
}

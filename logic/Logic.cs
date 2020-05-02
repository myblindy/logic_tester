﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Logic : ReactiveObject
{
    static readonly Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim)> Transitions =
        new Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim)>
        {
            // initializare
            [(0, -1)] = (0, -1, -1),             // stadiul initial -1 nu-i niciodata
            [(0, 0b0011)] = (1, -1, -1),         // stadiul initial -1 nu-i niciodata
            [(1, 0b0011)] = (1, -1, -1),         // ramane in stadiul curent
            [(1, 0b0010)] = (2, -1, -1),         // trece in pasul urmator
            [(2, 0b0010)] = (2, -1, -1),
            [(2, 0b0000)] = (3, -1, -1),
            [(3, 0b0000)] = (3, -1, -1),
            [(3, 0b0100)] = (4, -1, -1),
            [(4, 0b0100)] = (4, -1, -1),
            [(4, 0b1100)] = (5, 8, 10),           // delay at Open      
            [(5, 0b1100)] = (5, -1, -1),
            [(5, 0b0100)] = (6, -1, -1),
            [(6, 0b0100)] = (6, -1, -1),
            [(6, 0b0000)] = (7, -1, -1),
            [(7, 0b0000)] = (7, -1, -1),
            [(7, 0b0010)] = (8, -1, -1),
            [(8, 0b0010)] = (8, -1, -1),
            [(8, 0b0011)] = (1, 8, 10),            // delay at Close
            [(-1, -1)] = (-1, -1, -1),
            [(1, -1)] = (-1, -1, -1),
            [(2, -1)] = (-1, -1, -1),
            [(3, -1)] = (-1, -1, -1),
            [(4, -1)] = (-1, -1, -1),
            [(5, -1)] = (-1, -1, -1),
            [(6, -1)] = (-1, -1, -1),
            [(7, -1)] = (-1, -1, -1),
            [(8, -1)] = (-1, -1, -1),
        };

    static readonly Dictionary<int, (int Outputs, int Region)> StateOutputs =
        new Dictionary<int, (int, int)>
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

    readonly (TimeSpan StartDelay, TimeSpan StopDelay)[] OutputDelays = new[]
    {
        (TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1)),            // timpul de actiune al butoanelor
        (TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(1.5))
    };

    private readonly ILight[] Inputs;
    private readonly ILight[] Outputs;
    private readonly TaskScheduler MainTaskScheduler;   // ne spune ce ruleaza si cand face asta

    int starea;
    public int Starea { get => starea; set => this.RaiseAndSetIfChanged(ref starea, value); }

    public int Region;

    // Counter
    int counter;
    public int Counter { get => counter; set => this.RaiseAndSetIfChanged(ref counter, value); }

    bool PreviousCounterInput;

    public Logic(ILight[] inputs, ILight[] outputs, TaskScheduler mainTaskSheduler)
    {
        Inputs = inputs;
        Outputs = outputs;
        MainTaskScheduler = mainTaskSheduler;
    }

    public void Reset()
    {
        Starea = 0;
        Counter = 0;
        Outputs[15].Active = false;
        Outputs[16].Active = false;
    }

    public void Process(TimeSpan elapsed)      // Timpul curent
    {
        // Counter
        if (Inputs[4].Active && !PreviousCounterInput)                   // Daca inputul 4 este activ si inputul dinainte este neactiv
            ++Counter;                                                   // Adaugam 1 la counter
        PreviousCounterInput = Inputs[4].Active;

        // ca sa ramana LEDs in starea care au generat eroarea
        if (Starea != -1)
            for (int i = 0; i < 4; ++i)
                Outputs[i + 3].Active = Inputs[i].Active;

        // 1. Convertim dintr-un sir de LightModel in int Conditie
        //    (pentru ca sa-l folosim in dictionarul de tranzitii)
        int conditie =
            Convert.ToInt32(Inputs[0].Active) +
            Convert.ToInt32(Inputs[1].Active) * 2 +
            Convert.ToInt32(Inputs[2].Active) * 2 * 2 +
            Convert.ToInt32(Inputs[3].Active) * 2 * 2 * 2;

        // 2. Vrem sa stim care este starea urmatoare. Ne trebuie starea, conditia si dictionarul
        if (!Transitions.TryGetValue((Starea, conditie), out var val))
            val = Transitions[(Starea, -1)];
        int StareaUrmatoare = val.StareaUrmatoare;

        if (Counter < val.ContorMinim || Counter > val.ContorMaxim && val.ContorMinim != -1)
        {
            if (Counter < val.ContorMinim)
                Outputs[15].Active = true;
            else
                Outputs[16].Active = true;
            // Outputs[Counter < val.ContorMinim ? 15 : 16].Active = true;
            StareaUrmatoare = -1;
        }

        // 3. Cataum in dictionar stare ouputs corespunzatoare starii urmatoare
        var (TargetOutputs, TargetRegion) = StateOutputs[StareaUrmatoare];
        var (CurrentOutputs, _) = StateOutputs[Starea];

        // 4. update-am limitele in timp cand trebuie sa oprim iesirile
        if (Starea != StareaUrmatoare)
        {
            for (int _i = 0; _i < OutputDelays.Length; ++_i)
            {
                var i = _i;
                if ((TargetOutputs & (1 << i)) != 0 && (CurrentOutputs & (1 << i)) == 0)      // rising edge
                    Task.Run(async () =>
                    {
                        await Task.Delay(OutputDelays[i].StartDelay).ConfigureScheduler(MainTaskScheduler);
                        Outputs[i].Active = true;

                        Interlocked.Exchange(ref counter, 0);
                        this.RaisePropertyChanged(nameof(Counter));

                        await Task.Delay(OutputDelays[i].StopDelay).ConfigureScheduler(MainTaskScheduler);
                        Outputs[i].Active = false;
                    });
            }
        }

        Starea = StareaUrmatoare;
        Region = TargetRegion;

        // 6. Scriem in outputs valoarea output din starea curenta
        for (int i = 2; i < Outputs.Length - 2; ++i)
        {
            if (Starea == -1 && i >= 7 && i <= 14)
                continue;

            // Intrarile sunt deja copiate. In starea -1 nu mai copiem altele.
            // Daca este mai mic ca 3 sau mai mare ca sase conditia este adevarata atunci 
            if (i < 3 || i > 6)
                Outputs[i].Active = (TargetOutputs & (1 << i)) != 0;
        }
    }
}

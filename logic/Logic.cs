using logic;
using MoreLinq;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Logic<TLight> : ReactiveObject, ILogic where TLight : ILight
{
    readonly Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim)> Transitions =
        new Dictionary<(int Stare, int Conditie), (int StareaUrmatoare, int ContorMinim, int ContorMaxim)>();
    readonly Dictionary<int, (int Outputs, int Region)> StateOutputs = new Dictionary<int, (int, int)>();
    readonly (TimeSpan StartDelay, TimeSpan StopDelay)[] OutputDelays;

    public TLight[] Inputs { get; }
    public TLight[] Outputs { get; }

    private readonly TaskScheduler MainTaskScheduler;   // ne spune ce ruleaza si cand face asta

    int state;
    public int State { get => state; set => this.RaiseAndSetIfChanged(ref state, value); }

    public int Region { get; set; }

    // Counter
    int counter;
    public int Counter { get => counter; set => this.RaiseAndSetIfChanged(ref counter, value); }

    bool PreviousCounterInput;

    public Logic(TaskScheduler mainTaskSheduler, Func<int, TLight> inputGenerator, Func<int, TLight> outputGenerator)
    {
        Inputs = new TLight[5];
        for (int i = 0; i < Inputs.Length; ++i)
            Inputs[i] = inputGenerator(i);

        Outputs = new TLight[18];
        for (int i = 0; i < Outputs.Length; ++i)
            Outputs[i] = outputGenerator(i);

        MainTaskScheduler = mainTaskSheduler;

        try
        {
            static int BinaryStringToInt(string s) => s.StartsWith("0b") ? Convert.ToInt32(s[2..], 2) : Convert.ToInt32(s);

            var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            config.Transitions.ForEach(t => Transitions.Add((t.SourceState, BinaryStringToInt(t.Condition)), (t.DestinationState, t.CounterMinimum, t.CounterMaximum)));
            config.StateOutputs.ForEach(t => StateOutputs.Add(t.State, (BinaryStringToInt(t.Outputs), t.Region)));
            OutputDelays = config.OutputDelays.Select(d => (TimeSpan.FromTicks((long)(d.StartDelaySeconds * TimeSpan.TicksPerSecond)), TimeSpan.FromTicks((long)(d.StopDelaySeconds * TimeSpan.TicksPerSecond)))).ToArray();
        }
        catch
        {
            // error -24
            Transitions.Add((0, -1), (-24, -1, -1));
            Transitions.Add((-24, -1), (-24, -1, -1));
            StateOutputs.Add(0, (0, 0));
            StateOutputs.Add(-24, (0, 0));
            OutputDelays = Array.Empty<(TimeSpan StartDelay, TimeSpan StopDelay)>();
        }
    }

    public void Reset()
    {
        State = 0;
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
        if (State != -1)
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
        if (!Transitions.TryGetValue((State, conditie), out var val))
            val = Transitions[(State, -1)];
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
        var (CurrentOutputs, _) = StateOutputs[State];

        // 4. update-am limitele in timp cand trebuie sa oprim iesirile
        if (State != StareaUrmatoare)
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

        State = StareaUrmatoare;
        Region = TargetRegion;

        // 6. Scriem in outputs valoarea output din starea curenta
        for (int i = 2; i < Outputs.Length - 2; ++i)
        {
            if (State == -1 && i >= 7 && i <= 14)
                continue;

            // Intrarile sunt deja copiate. In starea -1 nu mai copiem altele.
            // Daca este mai mic ca 3 sau mai mare ca sase conditia este adevarata atunci 
            if (i < 3 || i > 6)
                Outputs[i].Active = (TargetOutputs & (1 << i)) != 0;
        }
    }
}

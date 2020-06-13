using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;

namespace logic
{
    public class Configuration
    {
        public IList<TransitionConfiguration> Transitions { get; set; }
        public IList<StateOutputsConfiguration> StateOutputs { get; set; }
        public IList<OutputDelaysConfiguration> OutputDelays { get; set; }
        public bool DisableCounterFreeze { get; set; }
    }

    public class TransitionConfiguration
    {
        public int SourceState { get; set; }
        public string Condition { get; set; }
        public int DestinationState { get; set; }
        public int CounterMinimum { get; set; }
        public int CounterMaximum { get; set; }
    }

    public class StateOutputsConfiguration
    {
        public int State { get; set; }
        public string Outputs { get; set; }
        public int Region { get; set; }
    }

    public class OutputDelaysConfiguration
    {
        public double StartDelaySeconds { get; set; }
        public double StopDelaySeconds { get; set; }
    }
}
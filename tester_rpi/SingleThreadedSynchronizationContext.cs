using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace tester_rpi
{
    public class SingleThreadedSynchronizationContext : SynchronizationContext
    {
        private sealed class WorkItem
        {
            private readonly SendOrPostCallback Callback;
            private readonly object State;
            private readonly ManualResetEventSlim Reset;

            public WorkItem(SendOrPostCallback callback, object state, ManualResetEventSlim reset)
            {
                Callback = callback ?? throw new ArgumentNullException("callback");
                State = state;
                Reset = reset;
            }

            public void Execute()
            {
                Callback(State);
                Reset?.Set();
            }
        }

        private readonly ConcurrentQueue<WorkItem> WorkItems = new ConcurrentQueue<WorkItem>();
        private readonly Thread ExecutingThread;

        public SingleThreadedSynchronizationContext(Thread executingThread) =>
            ExecutingThread = executingThread ?? throw new ArgumentNullException("executingThread");

        internal bool HasWorkItems => !WorkItems.IsEmpty;

        public void ExecuteAllWorkItems()
        {
            while (!(ExecuteAndReturnNextWorkItem() is null)) { }
        }

        private WorkItem ExecuteAndReturnNextWorkItem()
        {
            if (WorkItems.TryDequeue(out var currentItem))
                currentItem.Execute();
            return currentItem;
        }

        public override void Post(SendOrPostCallback d, object state) =>
            WorkItems.Enqueue(new WorkItem(d, state, null));

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread == ExecutingThread)
            {
                WorkItem requestedWorkItem = new WorkItem(d, state, null);
                WorkItems.Enqueue(requestedWorkItem);

                WorkItem executedWorkItem;
                do
                {
                    executedWorkItem = ExecuteAndReturnNextWorkItem();
                } while (executedWorkItem != null && executedWorkItem != requestedWorkItem);
            }
            else
            {
                using var reset = new ManualResetEventSlim();
                WorkItems.Enqueue(new WorkItem(d, state, reset));
                reset.Wait();
            }
        }
    }
}

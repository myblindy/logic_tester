using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tester_rpi
{
    class SingleThreadedSynchronizationContextTaskScheduler : TaskScheduler
    {
        private readonly SynchronizationContext SynchronizationContext;

        public SingleThreadedSynchronizationContextTaskScheduler(SynchronizationContext context) =>
            SynchronizationContext = context;

        [SecurityCritical]
        protected override void QueueTask(Task task) =>
            SynchronizationContext.Post(PostCallback, task);

        [SecurityCritical]
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (SynchronizationContext.Current == SynchronizationContext)
                return TryExecuteTask(task);
            else
                return false;
        }

        [SecurityCritical]
        protected override IEnumerable<Task> GetScheduledTasks() => null;

        public override int MaximumConcurrencyLevel => 1;

        private void PostCallback(object obj)
        {
            Task task = (Task)obj;
            TryExecuteTask(task);
        }
    }
}
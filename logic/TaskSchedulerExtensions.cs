using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public static class TaskExtension
{
    public static CustomTaskAwaitable ConfigureScheduler(this Task task, TaskScheduler scheduler) =>
        new CustomTaskAwaitable(task, scheduler);
}

public struct CustomTaskAwaitable
{
    readonly CustomTaskAwaiter awaitable;

    public CustomTaskAwaitable(Task task, TaskScheduler scheduler) =>
        awaitable = new CustomTaskAwaiter(task, scheduler);

    public CustomTaskAwaiter GetAwaiter() { return awaitable; }

    public struct CustomTaskAwaiter : INotifyCompletion
    {
        readonly Task task;
        readonly TaskScheduler scheduler;

        public CustomTaskAwaiter(Task task, TaskScheduler scheduler)
        {
            this.task = task;
            this.scheduler = scheduler;
        }

        public void OnCompleted(Action continuation)
        {
            // ContinueWith sets the scheduler to use for the continuation action
            task.ContinueWith(x => continuation(), scheduler);
        }

        public bool IsCompleted => task.IsCompleted;
        public void GetResult() { }
    }
}

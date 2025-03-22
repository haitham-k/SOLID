using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Ex4Dll
{
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Completed
    }
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }
    public class TaskItem
    {
        public TaskItem(string name, TaskPriority priority, DateTimeOffset dueDate)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Name = name;
            Priority = priority;
            DueDate = dueDate;

        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        public TaskPriority Priority { get; set; }
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset DueDate { get; set; }
    }

    public interface IProjectTaskManager
    {
        Task AddTaskAsync(TaskItem task);
        Task MarkTaskAsCompletedAsync(Guid taskId);
        IAsyncEnumerable<TaskItem> GetPendingTasksAsync(CancellationToken cancellationToken);
        Task<TaskSummary> GetProjectSummaryAsync();
    }
    public class ProjectTaskManager : IProjectTaskManager
    {
        public readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();
        public async Task AddTaskAsync(TaskItem task)
        {
            if (task == null) throw new ArgumentNullException("Task cannot be null", nameof(task));
            if (task.Id == Guid.Empty) task.Id = Guid.NewGuid();
            //await Task.Run(() =>
            //{
            //    _tasks.TryAdd(task.Id, task);
            //    OnChanged(new TaskChangedEventArgd("Add", task));
            //});
            bool added = await Task.Run(() => _tasks.TryAdd(task.Id, task));
            if (added)
            {
                OnChanged(new TaskChangedEventArgd("Add", task));
            }
        }

        public async Task MarkTaskAsCompletedAsync(Guid taskId)
        {
            if (taskId == Guid.Empty) throw new ArgumentException("Task ID cannot be empty", nameof(taskId));
            //if (!_tasks.ContainsKey(taskId)) throw new ArgumentOutOfRangeException("Task not found", nameof(taskId));
            //await Task.Run(() => _tasks![taskId].Status = TaskStatus.Completed);
            //OnTaskCompleted(new TaskCompletedEventArgd(_tasks[taskId].Name));
            await Task.Run(() =>
            {
                if (_tasks.TryGetValue(taskId, out var task))
                {
                    var updatedTask = new TaskItem(task.Name, task.Priority, task.DueDate)
                    {
                        Id = task.Id,
                        Status = TaskStatus.Completed,
                        CreatedOn = task.CreatedOn
                    };

                    bool updated = _tasks.TryUpdate(taskId, updatedTask, task);
                    if (updated)
                    {
                        OnTaskCompleted(new TaskCompletedEventArgd(task.Name));
                    }
                }
                else
                {
                    throw new KeyNotFoundException($"Task with ID {taskId} not found.");
                }
            });

        }
        public async Task<bool> RemoveTaskAsync(Guid taskId)
        {
            if (_tasks.TryRemove(taskId, out var removedTask))
            {
                OnChanged(new TaskChangedEventArgd("Deleted", removedTask));
                return await Task.FromResult(true);
            }
            return false;
        }
        public async IAsyncEnumerable<TaskItem> GetPendingTasksAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var query = _tasks.Values
                .AsParallel()
                .WithCancellation(cancellationToken)
                .Where(t => t.Status == TaskStatus.ToDo)
                .OrderByDescending(t => t.Priority);

            foreach (var task in query)
            {
                await Task.Yield();
                if (cancellationToken.IsCancellationRequested) break;
                yield return task!;
            }

        }

        public async Task<TaskSummary> GetProjectSummaryAsync()
        {
            return await Task.Run(() =>
            {
                return new TaskSummary
                {
                    CompletedTasks = _tasks.Values.Count(t => t.Status == TaskStatus.Completed),
                    OverdueTasks = _tasks.Values.Count(t => t.DueDate < DateTimeOffset.UtcNow && t.Status != TaskStatus.Completed),
                    TotalTasks = _tasks.Count
                };
            });
        }

        public delegate void TaskCompletedEventHandler(object? sender, TaskCompletedEventArgd e);
        public event TaskCompletedEventHandler? TaskCompleted;
        public event EventHandler? TaskChanged;
        protected virtual void OnChanged(TaskChangedEventArgd e)
        {
            TaskChanged?.Invoke(this, e);
        }
        protected virtual void OnTaskCompleted(TaskCompletedEventArgd e)
        {
            TaskCompleted?.Invoke(this, e);
        }

    }
    public class TaskSummary
    {
        public int TotalTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedTasks { get; set; }
    }
    public class TaskChangedEventArgd : EventArgs
    {
        public TaskItem? TaskItem { get; set; }
        public string? Operation { get; set; }
        public TaskChangedEventArgd(string operation, TaskItem task)
        {
            TaskItem = task;
            Operation = operation;
        }

    }
    public class TaskCompletedEventArgd : EventArgs
    {
        public string TaskName { get; set; }
        public TaskCompletedEventArgd(string taskName)
        {
            TaskName = taskName;
        }
    }


}

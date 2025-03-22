using Ex4Dll;
using System.Threading.Tasks;

var taskManager = new ProjectTaskManager();

// Abonnement à l'événement de complétion d'une tâche
taskManager.TaskCompleted += (sender, e) =>
{
    Console.WriteLine($"✅ Tâche complétée : {e.TaskName}");
};

// Ajout de tâches
await taskManager.AddTaskAsync(new TaskItem("Créer la base de données", TaskPriority.High, DateTime.UtcNow.AddDays(2)));
await taskManager.AddTaskAsync(new TaskItem("Développer l'API", TaskPriority.Medium, DateTime.UtcNow.AddDays(5)));
await taskManager.AddTaskAsync(new TaskItem("Tester l'application", TaskPriority.Low, DateTime.UtcNow.AddDays(1)));

// Compléter une tâche
var taskId = taskManager._tasks.Keys.First();
await taskManager.MarkTaskAsCompletedAsync(taskId);

// Récupérer toutes les tâches en cours, triées par priorité
await foreach (var task in taskManager.GetPendingTasksAsync())
{
    Console.WriteLine($"📌 {task.Name} - Priorité : {task.Priority}");
}

// Obtenir un résumé du projet
var summary = await taskManager.GetProjectSummaryAsync();
Console.WriteLine($"📊 Total : {summary.TotalTasks}, En retard : {summary.OverdueTasks}, Complétées : {summary.CompletedTasks}");

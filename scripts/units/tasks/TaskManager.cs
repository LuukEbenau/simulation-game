using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.units.tasks
{
    public partial class TaskManager : Node3D, ITaskManager
    {
        private List<UnitTask> UnitTaskQueue { get; set; } = new List<UnitTask> { };

        public IEnumerable<UnitTask> GetTasks() => GetUnitTasksResursive(UnitTaskQueue.ToList());

        private IEnumerable<UnitTask> GetUnitTasksResursive(IEnumerable<UnitTask> tasks)
        {
            foreach (var task in tasks)
            {
                if (task.IsFinished)
                {
                    FinishTask(task); //TODO: will this not give reference issues? since finishtask might alter the UnitTaskQueue
                    continue;
                }
                if (task is CollectionTask ct)
                {
                    foreach (var subtask in GetUnitTasksResursive(ct.Tasks))
                    {
                        yield return subtask;
                    }
                }
                else
                {
                    yield return task;
                }
            }
        }

        public void EnqueueTask(UnitTask task)
        {
            UnitTaskQueue.Add(task);
        }
        /// <summary>
        /// Finish the task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool FinishTask(UnitTask task)
        {
            return FinishTaskRecursive(UnitTaskQueue, task);

        }
        /// <summary>
        /// Recursively searches for the task, then finishes it
        /// </summary>
        /// <param name="taskList"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private bool FinishTaskRecursive(List<UnitTask> taskList, UnitTask task)
        {
            if (taskList.Remove(task))
            {
                if (task.FollowUpTasks.Count > 0)
                {
                    foreach (var followUpTask in task.FollowUpTasks)
                    {
                        EnqueueTask(followUpTask);
                    }
                }

                // if it has a parent task, check if its finished as well now. If so, we can remove this as well. This should go on recursively. 
                if(task.ParentTask != null && task.ParentTask.IsFinished)
                {
                    FinishTask(task.ParentTask);
                }

                return true;
            }
            else
            {
                // its a subtask probably
                foreach (var t2 in UnitTaskQueue)
                {
                    if (t2 is CollectionTask ct)
                    {
                        if (FinishTaskRecursive(ct.Tasks, task))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}

using System.Collections.Generic;

namespace SacaSimulationGame.scripts.units.tasks
{
    public interface ITaskManager
    {
        void EnqueueTask(UnitTask task);
        bool FinishTask(UnitTask task);
        IEnumerable<UnitTask> GetTasks();
    }
}
using System.Threading.Tasks;
using UnityEngine;

namespace HJ.Runtime
{
    public class WaitToTaskComplete : CustomYieldInstruction
    {
        private readonly Task _task;

        public WaitToTaskComplete(Task task)
        {
            this._task = task;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!_task.IsCompleted) return true;
                if (_task.IsFaulted) throw _task.Exception;
                return false;
            }
        }
    }
}
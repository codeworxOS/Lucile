using System.Threading.Tasks;

namespace System
{
    public static class TaskExtensions
    {
        [Obsolete("Use System.Threading.Tasks.LucileCoreTaskExtensions.Invoke instead")]
        public static void InvokeAsync(this Task task)
        {
            System.Threading.Tasks.LucileCoreTaskExtensions.Invoke(task);
        }
    }
}
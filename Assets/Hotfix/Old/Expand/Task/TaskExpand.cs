using System.Threading.Tasks;

namespace LccModel
{
    public static class TaskExpand
    {
        public static async void Continue(this Task task)
        {
            await task;
        }
    }
}
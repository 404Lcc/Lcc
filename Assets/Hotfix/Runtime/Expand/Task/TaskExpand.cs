using System.Threading.Tasks;

namespace LccHotfix
{
    public static class TaskExpand
    {
        public static async void Continue(this Task task)
        {
            await task;
        }
    }
}
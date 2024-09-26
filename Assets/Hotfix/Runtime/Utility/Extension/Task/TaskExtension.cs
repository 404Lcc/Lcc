using System.Threading.Tasks;

namespace LccHotfix
{
    public static class TaskExtension
    {
        public static async void Continue(this Task task)
        {
            await task;
        }
    }
}
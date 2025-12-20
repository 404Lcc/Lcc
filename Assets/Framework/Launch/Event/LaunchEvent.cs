namespace LccModel
{
    public static class LaunchEvent
    {
        public class StateChanged : IEventMessage
        {
            public string From;
            public string To;

            public static void Broadcast(string from, string to)
            {
                Event.SendMessage(new StateChanged
                {
                    From = from,
                    To = to
                });
            }
        }

        public class ShowVersion : IEventMessage
        {
            public string VersionStr = "";

            public static void Broadcast(string Version)
            {
                Event.SendMessage(new ShowVersion
                {
                    VersionStr = Version
                });
            }
        }

        public class ShowMessageBox : IEventMessage
        {
            public UIPanelLaunch.MessageBoxParams Params;

            public static void Broadcast(UIPanelLaunch.MessageBoxParams @params)
            {
                Event.SendMessage(new ShowMessageBox
                {
                    Params = @params
                });
            }
        }

        public class ShowProgress : IEventMessage
        {
            public float Progress = 1f;
            public string ProgressText = "";

            public static void Broadcast(float progress, string progressText)
            {
                Event.SendMessage(new ShowProgress
                {
                    Progress = progress,
                    ProgressText = progressText
                });
            }
        }
    }
}
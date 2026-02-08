namespace LccHotfix
{
    public class GuideMessage : IGuideMessage
    {
        public void GuideStart(int id)
        {
            EvtGuideStart.Broadcast(id);
        }

        public void GuideEnd(int id, bool isException)
        {
            EvtGuideEnd.Broadcast(id, isException);
        }
    }
}
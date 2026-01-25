namespace LccHotfix
{
    public class GuideCheckFinish : IGuideCheckFinish
    {
        public bool CheckGuideFinish(int guideId)
        {
            switch (guideId)
            {
                case 1:
                    return false;
            }

            return false;
        }
    }
}
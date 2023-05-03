﻿namespace LccHotfix
{
    public class ViewModelBase
    {
        public Panel selfPanel;
        public TopData topData;

        public virtual void InitTopData()
        {
            topData = selfPanel.AddChildren<TopData>(selfPanel.Type);
        }
    }
}
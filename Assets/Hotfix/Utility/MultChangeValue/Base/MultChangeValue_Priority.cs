//////////////////////////////////////////////////////////////////////////
//当前值以优先级最高的修改为准

public class MultChangeValue_Priority<T> : MultChangeValue<T>
{
    public MultChangeValue_Priority() : base()
    {
    }

    public MultChangeValue_Priority(T baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        if (mValueChangeList.Count <= 0)
        {
            mCurValue = mBaseValue;
            return;
        }

        int highPriority = 0;
        for (int i = 0; i < mValueChangeList.Count; ++i)
        {
            var v = mValueChangeList[i];
            int priority = MultChangeSetting.GetPriority(v.Flag);
            if (priority >= highPriority)
            {
                highPriority = priority;
                mCurValue = v.Value;
            }
        }
    }
}
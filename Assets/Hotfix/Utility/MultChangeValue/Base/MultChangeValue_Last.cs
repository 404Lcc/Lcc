//////////////////////////////////////////////////////////////////////////
//当前值以最后一次修改的为准

public class MultChangeValue_Last<T> : MultChangeValue<T>
{
    public MultChangeValue_Last() : base()
    {
    }

    public MultChangeValue_Last(T baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        if (mValueChangeList.Count > 0)
            mCurValue = mValueChangeList[mValueChangeList.Count - 1].Value;
        else
            mCurValue = mBaseValue;
    }
}
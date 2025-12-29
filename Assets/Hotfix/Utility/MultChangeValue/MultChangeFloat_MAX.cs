public class MultChangeFloat_MAX : MultChangeValue<float>
{
    public MultChangeFloat_MAX() : base()
    {
    }

    public MultChangeFloat_MAX(float baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        if (mValueChangeList.Count > 0)
        {
            foreach (var v in mValueChangeList)
            {
                if (mCurValue < v.Value)
                {
                    mCurValue = v.Value;
                }
            }
        }
    }
}
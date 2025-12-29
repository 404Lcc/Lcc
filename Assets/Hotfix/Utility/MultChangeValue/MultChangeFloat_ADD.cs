public class MultChangeFloat_ADD : MultChangeValue<float>
{
    public MultChangeFloat_ADD()
    {
        mCurValue = mBaseValue = 0f;
    }

    public MultChangeFloat_ADD(float baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        foreach (var v in mValueChangeList)
        {
            mCurValue += v.Value;
        }
    }
}

public class MultChangeDouble_ADD : MultChangeValue<double>
{
    public MultChangeDouble_ADD()
    {
        mCurValue = mBaseValue = 0f;
    }

    public MultChangeDouble_ADD(float baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        foreach (var v in mValueChangeList)
        {
            mCurValue += v.Value;
        }
    }
}
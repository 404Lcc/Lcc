//当前值是所有改动中的最小值

public class MultChangeFloat_MIN : MultChangeValue<float>
{
    public MultChangeFloat_MIN() : base()
    {
    }

    public MultChangeFloat_MIN(float baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        if (mValueChangeList.Count > 0)
        {
            foreach (var v in mValueChangeList)
            {
                if (mCurValue > v.Value)
                {
                    mCurValue = v.Value;
                }
            }
        }
    }
}

//当前值是所有改动中的最小值
public class MultChangeDouble_MIN : MultChangeValue<double>
{
    public MultChangeDouble_MIN() : base()
    {
    }

    public MultChangeDouble_MIN(double baseValue)
        : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        mCurValue = mBaseValue;
        if (mValueChangeList.Count > 0)
        {
            foreach (var v in mValueChangeList)
            {
                if (mCurValue > v.Value)
                {
                    mCurValue = v.Value;
                }
            }
        }
    }
}
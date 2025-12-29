public class MultChangeInt_MAX : MultChangeValue<int>
{
    public MultChangeInt_MAX() : base()
    {
    }

    public MultChangeInt_MAX(int baseValue) : base(baseValue)
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
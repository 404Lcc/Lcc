//////////////////////////////////////////////////////////////////////////
//可能同时被多个独立模块修改的bool, 只要有一个为true，当前值为true

public class MultChangeBool_OR : MultChangeValue<bool>
{
    public MultChangeBool_OR()
    {
        mCurValue = mBaseValue = false;
    }

    public MultChangeBool_OR(bool baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        if (mBaseValue)
        {
            mCurValue = true;
            return;
        }

        if (mValueChangeList.Exists(v => v.Value == true))
            mCurValue = true;
        else
            mCurValue = false;
    }
}
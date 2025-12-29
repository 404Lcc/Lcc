//////////////////////////////////////////////////////////////////////////
//可能同时被多个独立模块修改的bool, 只要有一个为false，当前值为false

public class MultChangeBool_AND : MultChangeValue<bool>
{
    public MultChangeBool_AND()
    {
        mCurValue = mBaseValue = true;
    }

    public MultChangeBool_AND(bool baseValue) : base(baseValue)
    {
    }

    protected override void CalcuCurValue()
    {
        if (!mBaseValue)
        {
            mCurValue = false;
            return;
        }

        if (mValueChangeList.Exists(v => v.Value == false))
            mCurValue = false;
        else
            mCurValue = true;
    }
}
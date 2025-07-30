using System.Collections.Generic;

namespace LccHotfix
{
	public enum FunctionID
	{

	}

	internal partial class FunctionOpenManager : Module, IFunctionOpenService
	{
        private List<int> _functionOpenStatus = new List<int>();
		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
		}

		internal override void Shutdown()
		{
			_functionOpenStatus.Clear();
		}
		public bool IsFuncOpened(int functionID, bool dataCheck = false)
		{
			if (functionID <= 0) return true;
			bool enable = false;
			FunctionID funcID = (FunctionID)functionID;
			switch (funcID)
			{
				default:
					break;
			}
			enable = GetFuncOpenState((FunctionID)functionID);
			return enable;
		}


		public bool GetFuncOpenState(FunctionID functionID)
		{
			int funcID = (int)functionID;
			return _functionOpenStatus.Contains(funcID);
		}
		public bool IsFunctionOpenedAndShowTips(int functionID, bool useNotice = false, bool popTips = true)
		{
			if (!IsFuncOpened(functionID))
			{
				return false;
			}
			return true;
		}
	}
}
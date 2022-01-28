using LccModel;
using UnityEngine.UI;

namespace LccHotfix
{
    public class LoadPanel : APanelView<LoadModel>
    {
        public Slider loadProcess;
        public override void InitView(LoadModel viewModel)
        {
            Binding<bool>(nameof(viewModel.isLoading), IsLoading);
        }
        public override void Update()
        {
            loadProcess.value = SceneLoadManager.Instance.GetLoadProcess();
            if (loadProcess.value == 100)
            {
                ViewModel.isLoading.Value = true;
            }
        }
        public void IsLoading(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                ClearPanel();
            }
        }
    }
}
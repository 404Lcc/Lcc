using System.Collections;

namespace LccHotfix
{
    public interface ISceneService : IService
    {
        SceneType CurState { get; }

        bool IsLoading { get; }

        void SetSceneHelper(ISceneHelper sceneHelper);
        
        LoadSceneHandler GetScene(SceneType type);

        void ChangeScene(SceneType type);
        
        void CleanScene();

        #region 切场景界面

        void OpenChangeScenePanel();

        void CleanChangeSceneParam();

        #endregion
    }
}
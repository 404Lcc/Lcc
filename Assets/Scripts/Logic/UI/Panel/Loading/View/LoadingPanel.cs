using ET;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public class LoadingPanel : APanelView<LoadingModel>
    {
        private static LoadingPanel _instance;

        private float _currentPercent;
        private float _targetPercent;
        private float _updateRate;

        public Slider progress;
        public Text progressText;
        public static LoadingPanel Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObjectEntity gameObjectEntity = GameEntity.Instance.AddChildren<GameObjectEntity, GameObject>(CreateGameObject("Prefab/Panel/LoadingPanel"));
                    _instance = gameObjectEntity.AddComponent<LoadingPanel>();
                }
                return _instance;
            }
        }
        public static GameObject CreateGameObject(string path)
        {
            GameObject gameObject = Instantiate(path);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            Vector2 anchorMin = Screen.safeArea.position;
            Vector2 anchorMax = Screen.safeArea.position + Screen.safeArea.size;
            anchorMin = new Vector2(anchorMin.x / Screen.width, anchorMin.y / Screen.height);
            anchorMax = new Vector2(anchorMax.x / Screen.width, anchorMax.y / Screen.height);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            return gameObject;
        }
        public static GameObject Instantiate(string path)
        {
            GameObject asset = Resources.Load<GameObject>(path);
            GameObject gameObject = Object.Instantiate(asset, Objects.Canvas.transform);
            gameObject.name = Path.GetFileNameWithoutExtension(path);
            return gameObject;
        }
        public async ETTask UpdateLoadingPercent(int from, int to, float rate = 1)
        {
            gameObject.SetActive(true);
            gameObject.transform.SetAsLastSibling();
            _updateRate = rate;
            _targetPercent = to;
            _currentPercent = Mathf.Clamp(_currentPercent, from, to);

            progress.value = _currentPercent * 0.01f;
            progressText.text = (int)_currentPercent + "%";
            while (_currentPercent < _targetPercent)
            {
                await Task.Delay((int)(1 / 60f * 1000));
                _currentPercent += _updateRate;
                _currentPercent = Mathf.Clamp(_currentPercent, 0, 100);

                progress.value = _currentPercent * 0.01f;
                progressText.text = (int)_currentPercent + "%";
            }
            await Task.Delay((int)(1 / 60f * 1000));
        }
        public override void ClosePanel()
        {
            gameObject.SetActive(false);
        }
    }
}
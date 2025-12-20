using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LccModel
{
    public partial class UIPanelLaunch
    {
        public struct MessageBoxOption
        {
            public string name;
            public System.Action action;
        }

        public struct MessageBoxParams
        {
            public string Content;
            public List<MessageBoxOption> btnOptionList;
        }

        /// <summary>
        /// 对话框封装类
        /// </summary>
        private class MessageBox
        {
            private MessageBoxParams _params;
            private GameObject _gameObject;
            private Text _Text_Content;
            private readonly List<Transform> _btnList = new();
            public bool ActiveSelf => _gameObject.activeSelf;

            public void Create(GameObject gameObject)
            {
                _gameObject = gameObject;
                _Text_Content = gameObject.transform.Find("Text_Content").GetComponent<Text>();

                for (var i = 0; i <= 1; i++)
                {
                    var trans = gameObject.transform.Find($"HLG_Buttons/Btn_{i + 1}");
                    _btnList.Add(trans);
                }
            }

            public void Show(MessageBoxParams @params)
            {
                _params = @params;
                _Text_Content.text = _params.Content;

                for (int i = 0; i < _btnList.Count; i++)
                {
                    var trans = _btnList[i];
                    if (i < _params.btnOptionList.Count)
                    {
                        trans.gameObject.SetActive(true);
                        trans.GetComponentInChildren<Text>().text = _params.btnOptionList[i].name;
                        var btn = trans.GetComponent<Button>();
                        var btnIndex = i;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnClickOptionBtn(btnIndex));
                    }
                    else
                    {
                        trans.gameObject.SetActive(false);
                    }
                }

                _gameObject.SetActive(true);
                _gameObject.transform.SetAsLastSibling();
            }

            private void Hide()
            {
                _Text_Content.text = string.Empty;
                _params = default;
                _gameObject.SetActive(false);
                foreach (var transform in _btnList)
                {
                    var btn = transform.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                }
            }

            private void OnClickOptionBtn(int index)
            {
                if (index < _params.btnOptionList.Count)
                {
                    _params.btnOptionList[index].action?.Invoke();
                    Hide();
                }
            }
        }

        private readonly EventGroup _uniEventGroup = new();
        private readonly List<MessageBox> _msgBoxList = new();
        private GameObject _messageBoxObj;
        private Text _Text_Version;
        private Text _Text_Desc;
        private Text _Text_Progress;
        private Slider _Slider_Progress;
        private float _desiredProgress = 0f;

        private void Init()
        {
            _Text_Version = transform.Find("Canvas/Text_Version").GetComponent<Text>();
            _Text_Desc = transform.Find("Canvas/Text_Desc").GetComponent<Text>();
            _Slider_Progress = transform.Find("Canvas/Slider").GetComponent<Slider>();
            _Text_Progress = transform.Find("Canvas/Slider/Text_Progress").GetComponent<Text>();
            _messageBoxObj = transform.Find("Canvas/MessageBox").gameObject;
            _messageBoxObj.SetActive(false);
            SetHint(string.Empty);

            _Slider_Progress.value = _desiredProgress;
            _Text_Progress.text = string.Empty;
        }

        void Update()
        {
            _Slider_Progress.value = Mathf.Lerp(_Slider_Progress.value, _desiredProgress, 0.1f);
        }

        private void SetVersion(string version)
        {
            _Text_Version.text = version;
        }

        private void SetHint(string hint)
        {
            _Text_Desc.text = hint;
        }

        private void SetProgress(float progress, string progressText)
        {
            // 如果是回退进度，不要插值，直接设置
            if (progress < _Slider_Progress.value)
            {
                _Slider_Progress.value = progress;
            }

            _desiredProgress = progress;
            _Text_Progress.text = progressText;
        }

        private void ShowMessageBox(MessageBoxParams @params)
        {
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            msgBox.Show(@params);
        }
    }
}
using UnityEngine.UI;

namespace LccModel
{
    public class LanguageText : Text
    {
        public override string text
        {
            get
            {
                return GetValue(base.text);
            }
            set
            {
                base.text = value;
            }
        }
        public string GetValue(string key)
        {
            //string text = LanguageManager.Instance.GetValue(key);
            //if (string.IsNullOrEmpty(text))
            //{
            //    return key;
            //}
            //return text;
            return "";
        }
    }
}
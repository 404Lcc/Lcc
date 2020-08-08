using UnityEngine;
using UnityEngine.UI;

public class LogInfo : Info
{
    public LogType type;
    public string value;
    public Text information;
    public void SetInformation(string value)
    {
        this.value = value;
        information.text = value;
    }
    public override void OpenPanel()
    {
        if (ContainerExist())
        {
            state = InfoState.Open;
            container.SetActive(true);
        }
        else
        {
            Debug.Log(type.ToString() + ":null");
        }
    }
    public override void ClosePanel()
    {
        if (ContainerExist())
        {
            state = InfoState.Close;
            container.SetActive(false);
        }
        else
        {
            Debug.Log(type.ToString() + ":null");
        }
    }
}
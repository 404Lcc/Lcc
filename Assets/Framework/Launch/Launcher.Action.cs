using System;

public partial class Launcher
{
    public event Action OnFixedUpdate;
    public event Action OnUpdate;
    public event Action OnLateUpdate;
    public event Action OnClose;
    public event Action OnDrawGizmos;

    private void FixedUpdate()
    {
        if (OnFixedUpdate != null)
        {
            OnFixedUpdate();
        }
    }

    private void Update()
    {
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

    private void LateUpdate()
    {
        if (OnLateUpdate != null)
        {
            OnLateUpdate();
        }
    }

    private void OnApplicationQuit()
    {
        ExcuteClose();
    }

    public void ExcuteClose()
    {
        if (OnClose != null)
        {
            OnClose();
        }
    }
}
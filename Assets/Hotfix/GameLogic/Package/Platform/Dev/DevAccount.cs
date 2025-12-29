using System;

public class DevAccount : BaseAccount
{
    private string _account;
    
    public void SetAccount(string account)
    {
        _account = account;
    }

    public void SetPlayerId(string playerId)
    {
        
    }

    public string GetToken()
    {
        return "";
    }

    public string GetAccount()
    {
        return _account;
    }

    public void Login()
    {
    }


    public void Logout()
    {
        _account = string.Empty;
    }
}
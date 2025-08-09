using System;

public interface BaseAccount
{
    void SetAccount(string account);
    void SetPlayerId(string playerId);
    string GetToken();
    string GetAccount();
    void Login();
    void Logout();
}
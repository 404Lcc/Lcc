using System;
using System.Collections;
using System.Collections.Generic;

namespace LccHotfix
{
    public partial class AudioManager
    {
        private readonly Dictionary<string, AudioBank> _audioBanks = new();

        private void InitBank()
        {
            if (Meta?.InitBanks is null)
            {
                return;
            }
            foreach (var path in Meta.InitBanks)
            {
                LoadBank(path);
            }
        }
        
        private AudioBank GetBank(string bankName)
        {
            if (string.IsNullOrEmpty(bankName))
            {
                return null;
            }
            if (_audioBanks.TryGetValue(bankName, out var bank))
            {
                return bank;
            }
            Audio.LogWarning($"[Audio] AudioBank \"{bankName}\" not found");
            return null;
        }
        
        public void LoadBank(string bankName)
        {
            var bank = AudioBank.Load(bankName);
            if (_audioBanks.ContainsKey(bank.Config.Name))
            {
                return;
            }
            _audioBanks.Add(bank.Config.Name, bank);
            Audio.Log($"[Audio] AudioBank \"{bankName}\" loaded.");
        }
        
        public IEnumerator LoadBankAsync(string bankName, Action callback = null)
        {
            var bank = AudioBank.Create(bankName);
            if (_audioBanks.ContainsKey(bankName))
            {
                callback?.Invoke();
                yield break;
            }
            _audioBanks.Add(bankName, bank);
            yield return bank.LoadAsync(callback);
            Audio.Log($"[Audio] AudioBank \"{bankName}\" loaded async.");
        }
        
        public void UnloadBank(string bankName)
        {
            if (!_audioBanks.ContainsKey(bankName))
            {
                return;
            }
            GetBank(bankName)?.Unload();
            _audioBanks.Remove(bankName);
            Audio.Log($"[Audio] AudioBank \"{bankName}\" unloaded.");;
        }
        
        private void DisposeBank()
        {
            foreach (var bank in _audioBanks.Values)
            {
                bank.Unload();
            }
            _audioBanks.Clear();
        }
    }
}
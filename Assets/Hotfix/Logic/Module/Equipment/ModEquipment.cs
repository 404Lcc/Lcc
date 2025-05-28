using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using LccHotfix;
using UnityEngine;

namespace LccHotfix
{
    [Serializable]
    public class EquipmentData
    {
        private Equipment _equipment;

        public int ID => _equipment.Id;

        public string EquipmentName => _equipment.EquipmentName;

        /// <summary>
        /// 装备类型
        /// </summary>
        public EquipmentType EquipmentType => _equipment.EquipmentType;

        /// <summary>
        /// 行为树脚本
        /// </summary>
        public string BtScript => _equipment.BtScript;
        
        //额外值

        public void InitData(int id)
        {
            _equipment = ConfigManager.Instance.Tables.TBEquipment.Get(id);
        }
    }

    public class CurrentEquipmentSaveData : SaveData
    {
        public List<EquipmentData> CurrentDataList { get; set; }
        public override void CreateNewSaveData()
        {
            CurrentDataList = new List<EquipmentData>();
        }
    }

    public class CurrentEquipmentData : ISaveDataConverter<CurrentEquipmentSaveData>
    {
        public List<EquipmentData> CurrentDataList { get; set; }
        
        public CurrentEquipmentSaveData ToSaveData()
        {
            var save = new CurrentEquipmentSaveData();
            save.CurrentDataList = new List<EquipmentData>();
            save.CurrentDataList.AddRange(save.CurrentDataList);
            return save;
        }

        public void FromSaveData(CurrentEquipmentSaveData data)
        {
            CurrentDataList = new List<EquipmentData>();
            CurrentDataList.AddRange(data.CurrentDataList);
        }
    }


    [Model]
    public class ModEquipment : ModelTemplate
    {
        public CurrentEquipmentData CurrentEquipmentData { get; set; }
        public override void Init()
        {
            base.Init();
        }
        
        public void InitData(GameSaveData gameSaveData)
        {
            var saveData = gameSaveData.GetModule<CurrentEquipmentSaveData>();
            CurrentEquipmentData = new CurrentEquipmentData();
            CurrentEquipmentData.FromSaveData(saveData);
        }

        public void SaveData(GameSaveData gameSaveData)
        {
            var module = CurrentEquipmentData.ToSaveData();
            gameSaveData.SetModule(module);
        }
    }
}
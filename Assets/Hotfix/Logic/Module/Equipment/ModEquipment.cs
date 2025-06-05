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

    public class CurrentEquipmentSaveData : ISave
    {
        public List<EquipmentData> CurrentDataList { get; set; }

        public void Init()
        {
            CurrentDataList = new List<EquipmentData>();
        }
    }

    public class CurrentEquipmentData : ISaveDataConverter<CurrentEquipmentSaveData>
    {
        public CurrentEquipmentSaveData Save { get; set; }
        public List<EquipmentData> CurrentDataList { get; set; }

        public void Flush()
        {
            Save.CurrentDataList = new List<EquipmentData>();
            Save.CurrentDataList.AddRange(Save.CurrentDataList);
        }

        public void Init()
        {
            CurrentDataList = new List<EquipmentData>();
            CurrentDataList.AddRange(Save.CurrentDataList);
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
            CurrentEquipmentData = gameSaveData.GetRunData<CurrentEquipmentData, CurrentEquipmentSaveData>();
        }
    }
}
namespace LccHotfix
{
    public class PlayerSimpleSaveData : SaveData
    {
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称

        public override void CreateNewSaveData()
        {
            UID = 1;
            Name = "";
        }
    }

    //简易信息
    public class PlayerSimpleData : ISaveDataConverter<PlayerSimpleSaveData>
    {
        public long UID { get; private set; } // 角色id
        public string Name { get; private set; } // 昵称

        public PlayerSimpleSaveData ToSaveData()
        {
            var save = new PlayerSimpleSaveData();
            save.UID = UID;
            save.Name = Name;
            return save;
        }

        public void FromSaveData(PlayerSimpleSaveData data)
        {
            this.UID = data.UID;
            this.Name = data.Name;
        }
    }

    [Model]
    public class ModPlayer : ModelTemplate, ISavePipeline
    {
        public PlayerSimpleData PlayerSimpleData { get; private set; }

        public void InitData(GameSaveData gameSaveData)
        {
            var saveData = gameSaveData.GetModule<PlayerSimpleSaveData>();
            PlayerSimpleData = new PlayerSimpleData();
            PlayerSimpleData.FromSaveData(saveData);
        }

        public void SaveData(GameSaveData gameSaveData)
        {
            var module = PlayerSimpleData.ToSaveData();
            gameSaveData.SetModule(module);
        }
    }
}
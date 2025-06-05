namespace LccHotfix
{
    public class PlayerSimpleSaveData : ISave
    {
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称

        public void Init()
        {
            UID = 1;
            Name = "";
        }
    }

    //简易信息
    public class PlayerSimpleData : ISaveDataConverter<PlayerSimpleSaveData>
    {
        public PlayerSimpleSaveData Save { get; set; }
        public long UID { get; private set; } // 角色id
        public string Name { get; private set; } // 昵称

        public void Flush()
        {
            Save.UID = UID;
            Save.Name = Name;
        }

        public void Init()
        {
            this.UID = Save.UID;
            this.Name = Save.Name;
        }
    }

    [Model]
    public class ModPlayer : ModelTemplate, ISavePipeline
    {
        public PlayerSimpleData PlayerSimpleData { get; private set; }

        public void InitData(GameSaveData gameSaveData)
        {
            PlayerSimpleData = gameSaveData.GetRunData<PlayerSimpleData, PlayerSimpleSaveData>();
        }
    }
}
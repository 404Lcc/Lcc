namespace LccHotfix
{
    public class PlayerSimpleSaveData : ISave
    {
        public string TypeName => GetType().FullName;
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称

        public void Init()
        {
            UID = 1;
            Name = "";
        }
    }

    //简易信息
    public class PlayerSimpleData : ISaveConverter<PlayerSimpleSaveData>
    {
        public PlayerSimpleSaveData Save { get; set; }
        public long UID { get; private set; } // 角色id
        public string Name { get; private set; } // 昵称

        public ISave Flush()
        {
            Save.UID = UID;
            Save.Name = Name;
            return Save;
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
            PlayerSimpleData = gameSaveData.GetSaveConverterData<PlayerSimpleData, PlayerSimpleSaveData>();
        }
    }
}
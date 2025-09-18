namespace LccHotfix
{
    public class PlayerSaveData : ISave
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

    public class PlayerData : ISaveConverter<PlayerSaveData>
    {
        public PlayerSaveData Save { get; set; }
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称

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

    //简易信息
    public class PlayerSimpleData
    {
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称

        public void InitData(long uid, string name)
        {
            UID = uid;
            Name = name;
        }
    }

    [Model]
    public class ModPlayer : ModelTemplate, ISavePipeline
    {
        public PlayerData PlayerData { get; set; }

        public void InitData(GameSaveData gameSaveData)
        {
            PlayerData = gameSaveData.GetSaveConverterData<PlayerData, PlayerSaveData>();
        }

        /// <summary>
        /// 获取本地玩家简易数据
        /// </summary>
        public PlayerSimpleData GetLocalPlayerSimpleData()
        {
            PlayerSimpleData simpleData = new PlayerSimpleData();
            simpleData.InitData(PlayerData.UID, PlayerData.Name);
            return simpleData;
        }
    }
}
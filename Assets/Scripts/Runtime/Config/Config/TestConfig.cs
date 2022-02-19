using LccModel;
using ProtoBuf;
using System;
using System.Collections.Generic;

namespace LccModel
{
    [Config]
    [ProtoContract]
    public partial class TestConfigCategory : ProtobufObject
    {
        [ProtoIgnore]
        public Dictionary<int, TestConfig> dict = new Dictionary<int, TestConfig>();
        [ProtoMember(1)]
        public List<TestConfig> list = new List<TestConfig>();
        public static TestConfigCategory Instance
        {
            get; set;
        }
        public TestConfig Current
        {
            get
            {
                if (dict == null || dict.Count == 0)
                {
                    return null;
                }
                return dict.Values.GetEnumerator().Current;
            }
        }
        public TestConfigCategory()
        {
            Instance = this;
        }
        public override void AfterDeserialization()
        {
            foreach (TestConfig item in list)
            {
                dict.Add(item.Id, item);
            }
        }
        public bool ConfigExist(int id)
        {
            return dict.ContainsKey(id);
        }
        public TestConfig Get(int id)
        {
            if (!ConfigExist(id))
            {
                throw new Exception($"{nameof(TestConfig)}配置表 Id : {id}不存在");
            }
            TestConfig config = dict[id];
            return config;
        }
    }
    [ProtoContract]
    public partial class TestConfig : ProtobufObject, IConfig
    {
		[ProtoMember(1, IsRequired = true)]
		public int Id { get; set; }
    }
}
using ProtoBuf;
using System.Collections.Generic;

namespace LccHotfix
{
	[ProtoContract]
	public partial class CGTestInfo : MessageObject
	{
		[ProtoMember(1)]
		public int id;
	}
	[ProtoContract]
	public partial class GCTestInfo : MessageObject
	{
		[ProtoMember(1)]
		public int id;
	}
}
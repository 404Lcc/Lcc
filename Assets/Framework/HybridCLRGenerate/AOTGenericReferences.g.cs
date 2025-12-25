using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DesperateDevs.Utils.dll",
		"Framework.dll",
		"Google.Protobuf.dll",
		"Newtonsoft.Json.dll",
		"RuntimeCompiler.dll",
		"Sirenix.Utilities.dll",
		"StompyRobot.SRDebugger.dll",
		"StompyRobot.SRF.dll",
		"System.Collections.Immutable.dll",
		"System.Core.dll",
		"System.dll",
		"UniFramework.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// DesperateDevs.Utils.ObjectPool<object>
	// Framework.Singleton.<>c<object>
	// Framework.Singleton<object>
	// Framework.SmallList<float>
	// Framework.SmallList<int>
	// Framework.SmallList<object>
	// Google.Protobuf.Collections.MapField.<>c<int,byte>
	// Google.Protobuf.Collections.MapField.<>c<int,int>
	// Google.Protobuf.Collections.MapField.<>c<int,long>
	// Google.Protobuf.Collections.MapField.<>c<int,object>
	// Google.Protobuf.Collections.MapField.<>c<long,long>
	// Google.Protobuf.Collections.MapField.<>c<long,object>
	// Google.Protobuf.Collections.MapField.<>c<object,long>
	// Google.Protobuf.Collections.MapField.<>c<object,object>
	// Google.Protobuf.Collections.MapField.<>c<uint,int>
	// Google.Protobuf.Collections.MapField.<>c<uint,long>
	// Google.Protobuf.Collections.MapField.<>c<uint,object>
	// Google.Protobuf.Collections.MapField.<>c<uint,uint>
	// Google.Protobuf.Collections.MapField.<>c<uint,ulong>
	// Google.Protobuf.Collections.MapField.<>c<ulong,int>
	// Google.Protobuf.Collections.MapField.<>c<ulong,long>
	// Google.Protobuf.Collections.MapField.<>c<ulong,object>
	// Google.Protobuf.Collections.MapField.<>c<ulong,uint>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<int,byte>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<int,int>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<int,long>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<int,object>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<long,long>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<long,object>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<object,long>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<object,object>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<uint,int>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<uint,long>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<uint,object>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<uint,uint>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<uint,ulong>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<ulong,int>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<ulong,long>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<ulong,object>
	// Google.Protobuf.Collections.MapField.<>c__DisplayClass7_0<ulong,uint>
	// Google.Protobuf.Collections.MapField.Codec<int,byte>
	// Google.Protobuf.Collections.MapField.Codec<int,int>
	// Google.Protobuf.Collections.MapField.Codec<int,long>
	// Google.Protobuf.Collections.MapField.Codec<int,object>
	// Google.Protobuf.Collections.MapField.Codec<long,long>
	// Google.Protobuf.Collections.MapField.Codec<long,object>
	// Google.Protobuf.Collections.MapField.Codec<object,long>
	// Google.Protobuf.Collections.MapField.Codec<object,object>
	// Google.Protobuf.Collections.MapField.Codec<uint,int>
	// Google.Protobuf.Collections.MapField.Codec<uint,long>
	// Google.Protobuf.Collections.MapField.Codec<uint,object>
	// Google.Protobuf.Collections.MapField.Codec<uint,uint>
	// Google.Protobuf.Collections.MapField.Codec<uint,ulong>
	// Google.Protobuf.Collections.MapField.Codec<ulong,int>
	// Google.Protobuf.Collections.MapField.Codec<ulong,long>
	// Google.Protobuf.Collections.MapField.Codec<ulong,object>
	// Google.Protobuf.Collections.MapField.Codec<ulong,uint>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<int,byte>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<int,int>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<int,long>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<int,object>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<long,long>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<long,object>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<object,long>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<object,object>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<uint,int>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<uint,long>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<uint,object>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<uint,uint>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<uint,ulong>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<ulong,int>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<ulong,long>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<ulong,object>
	// Google.Protobuf.Collections.MapField.DictionaryEnumerator<ulong,uint>
	// Google.Protobuf.Collections.MapField.MapView<int,byte,byte>
	// Google.Protobuf.Collections.MapField.MapView<int,byte,int>
	// Google.Protobuf.Collections.MapField.MapView<int,int,int>
	// Google.Protobuf.Collections.MapField.MapView<int,long,int>
	// Google.Protobuf.Collections.MapField.MapView<int,long,long>
	// Google.Protobuf.Collections.MapField.MapView<int,object,int>
	// Google.Protobuf.Collections.MapField.MapView<int,object,object>
	// Google.Protobuf.Collections.MapField.MapView<long,long,long>
	// Google.Protobuf.Collections.MapField.MapView<long,object,long>
	// Google.Protobuf.Collections.MapField.MapView<long,object,object>
	// Google.Protobuf.Collections.MapField.MapView<object,long,long>
	// Google.Protobuf.Collections.MapField.MapView<object,long,object>
	// Google.Protobuf.Collections.MapField.MapView<object,object,object>
	// Google.Protobuf.Collections.MapField.MapView<uint,int,int>
	// Google.Protobuf.Collections.MapField.MapView<uint,int,uint>
	// Google.Protobuf.Collections.MapField.MapView<uint,long,long>
	// Google.Protobuf.Collections.MapField.MapView<uint,long,uint>
	// Google.Protobuf.Collections.MapField.MapView<uint,object,object>
	// Google.Protobuf.Collections.MapField.MapView<uint,object,uint>
	// Google.Protobuf.Collections.MapField.MapView<uint,uint,uint>
	// Google.Protobuf.Collections.MapField.MapView<uint,ulong,uint>
	// Google.Protobuf.Collections.MapField.MapView<uint,ulong,ulong>
	// Google.Protobuf.Collections.MapField.MapView<ulong,int,int>
	// Google.Protobuf.Collections.MapField.MapView<ulong,int,ulong>
	// Google.Protobuf.Collections.MapField.MapView<ulong,long,long>
	// Google.Protobuf.Collections.MapField.MapView<ulong,long,ulong>
	// Google.Protobuf.Collections.MapField.MapView<ulong,object,object>
	// Google.Protobuf.Collections.MapField.MapView<ulong,object,ulong>
	// Google.Protobuf.Collections.MapField.MapView<ulong,uint,uint>
	// Google.Protobuf.Collections.MapField.MapView<ulong,uint,ulong>
	// Google.Protobuf.Collections.MapField<int,byte>
	// Google.Protobuf.Collections.MapField<int,int>
	// Google.Protobuf.Collections.MapField<int,long>
	// Google.Protobuf.Collections.MapField<int,object>
	// Google.Protobuf.Collections.MapField<long,long>
	// Google.Protobuf.Collections.MapField<long,object>
	// Google.Protobuf.Collections.MapField<object,long>
	// Google.Protobuf.Collections.MapField<object,object>
	// Google.Protobuf.Collections.MapField<uint,int>
	// Google.Protobuf.Collections.MapField<uint,long>
	// Google.Protobuf.Collections.MapField<uint,object>
	// Google.Protobuf.Collections.MapField<uint,uint>
	// Google.Protobuf.Collections.MapField<uint,ulong>
	// Google.Protobuf.Collections.MapField<ulong,int>
	// Google.Protobuf.Collections.MapField<ulong,long>
	// Google.Protobuf.Collections.MapField<ulong,object>
	// Google.Protobuf.Collections.MapField<ulong,uint>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<float>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<int>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<long>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<object>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<uint>
	// Google.Protobuf.Collections.RepeatedField.<GetEnumerator>d__28<ulong>
	// Google.Protobuf.Collections.RepeatedField<float>
	// Google.Protobuf.Collections.RepeatedField<int>
	// Google.Protobuf.Collections.RepeatedField<long>
	// Google.Protobuf.Collections.RepeatedField<object>
	// Google.Protobuf.Collections.RepeatedField<uint>
	// Google.Protobuf.Collections.RepeatedField<ulong>
	// Google.Protobuf.Extension<object,byte>
	// Google.Protobuf.Extension<object,int>
	// Google.Protobuf.Extension<object,object>
	// Google.Protobuf.Extension<object,uint>
	// Google.Protobuf.ExtensionSet.<>c<object>
	// Google.Protobuf.ExtensionSet<object>
	// Google.Protobuf.ExtensionValue<byte>
	// Google.Protobuf.ExtensionValue<int>
	// Google.Protobuf.ExtensionValue<object>
	// Google.Protobuf.ExtensionValue<uint>
	// Google.Protobuf.FieldCodec.<>c<byte>
	// Google.Protobuf.FieldCodec.<>c<float>
	// Google.Protobuf.FieldCodec.<>c<int>
	// Google.Protobuf.FieldCodec.<>c<long>
	// Google.Protobuf.FieldCodec.<>c<object>
	// Google.Protobuf.FieldCodec.<>c<uint>
	// Google.Protobuf.FieldCodec.<>c<ulong>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass31_0<int>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<byte>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<float>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<int>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<long>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<object>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<uint>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass38_0<ulong>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<byte>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<float>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<int>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<long>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<object>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<uint>
	// Google.Protobuf.FieldCodec.<>c__DisplayClass39_0<ulong>
	// Google.Protobuf.FieldCodec.InputMerger<byte>
	// Google.Protobuf.FieldCodec.InputMerger<float>
	// Google.Protobuf.FieldCodec.InputMerger<int>
	// Google.Protobuf.FieldCodec.InputMerger<long>
	// Google.Protobuf.FieldCodec.InputMerger<object>
	// Google.Protobuf.FieldCodec.InputMerger<uint>
	// Google.Protobuf.FieldCodec.InputMerger<ulong>
	// Google.Protobuf.FieldCodec.ValuesMerger<byte>
	// Google.Protobuf.FieldCodec.ValuesMerger<float>
	// Google.Protobuf.FieldCodec.ValuesMerger<int>
	// Google.Protobuf.FieldCodec.ValuesMerger<long>
	// Google.Protobuf.FieldCodec.ValuesMerger<object>
	// Google.Protobuf.FieldCodec.ValuesMerger<uint>
	// Google.Protobuf.FieldCodec.ValuesMerger<ulong>
	// Google.Protobuf.FieldCodec<byte>
	// Google.Protobuf.FieldCodec<float>
	// Google.Protobuf.FieldCodec<int>
	// Google.Protobuf.FieldCodec<long>
	// Google.Protobuf.FieldCodec<object>
	// Google.Protobuf.FieldCodec<uint>
	// Google.Protobuf.FieldCodec<ulong>
	// Google.Protobuf.IDeepCloneable<byte>
	// Google.Protobuf.IDeepCloneable<float>
	// Google.Protobuf.IDeepCloneable<int>
	// Google.Protobuf.IDeepCloneable<long>
	// Google.Protobuf.IDeepCloneable<object>
	// Google.Protobuf.IDeepCloneable<uint>
	// Google.Protobuf.IDeepCloneable<ulong>
	// Google.Protobuf.IMessage<object>
	// Google.Protobuf.MessageParser.<>c__DisplayClass2_0<object>
	// Google.Protobuf.MessageParser<object>
	// Google.Protobuf.ValueReader<byte>
	// Google.Protobuf.ValueReader<float>
	// Google.Protobuf.ValueReader<int>
	// Google.Protobuf.ValueReader<long>
	// Google.Protobuf.ValueReader<object>
	// Google.Protobuf.ValueReader<uint>
	// Google.Protobuf.ValueReader<ulong>
	// Google.Protobuf.ValueWriter<byte>
	// Google.Protobuf.ValueWriter<float>
	// Google.Protobuf.ValueWriter<int>
	// Google.Protobuf.ValueWriter<long>
	// Google.Protobuf.ValueWriter<object>
	// Google.Protobuf.ValueWriter<uint>
	// Google.Protobuf.ValueWriter<ulong>
	// SRDebugger.CircularBuffer.<GetEnumerator>d__18<object>
	// SRDebugger.CircularBuffer<object>
	// SRF.Components.SRAutoSingleton<object>
	// SRF.SRList.<GetEnumerator>d__15<object>
	// SRF.SRList<object>
	// System.Action<Framework.Inputs.KClickEvent>
	// System.Action<Framework.Inputs.KDragEndEvent>
	// System.Action<Framework.Inputs.KDragMoveEvent>
	// System.Action<Framework.Inputs.KDragStartEvent>
	// System.Action<Framework.Inputs.KGestureEndEvent>
	// System.Action<Framework.Inputs.KGesturePinchEvent>
	// System.Action<Framework.Inputs.KGestureStartEvent>
	// System.Action<Framework.Inputs.KMouseScrollEvent>
	// System.Action<Framework.Inputs.KTouchEndEvent>
	// System.Action<Framework.Inputs.KTouchMoveEvent>
	// System.Action<Framework.Inputs.KTouchStartEvent>
	// System.Action<HotUpdate.Config.TCommonFSMCondition>
	// System.Action<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Action<HotUpdate.Framework.CameraView.CameraViewModeChangeEvent>
	// System.Action<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Action<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Action<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Action<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Action<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Action<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Action<System.ValueTuple<int,int,int>>
	// System.Action<System.ValueTuple<int,int>>
	// System.Action<System.ValueTuple<object,object>>
	// System.Action<UnityEngine.EventSystems.RaycastResult>
	// System.Action<UnityEngine.Matrix4x4>
	// System.Action<UnityEngine.Vector2,UnityEngine.Vector2>
	// System.Action<UnityEngine.Vector2>
	// System.Action<UnityEngine.Vector3,UnityEngine.Vector3>
	// System.Action<byte>
	// System.Action<float,float>
	// System.Action<float,int>
	// System.Action<float>
	// System.Action<int,byte>
	// System.Action<int,object>
	// System.Action<int,uint>
	// System.Action<int>
	// System.Action<long>
	// System.Action<object,byte,int>
	// System.Action<object,byte,object>
	// System.Action<object,byte>
	// System.Action<object,float>
	// System.Action<object,int>
	// System.Action<object,object,object>
	// System.Action<object,object>
	// System.Action<object,uint,long>
	// System.Action<object,uint>
	// System.Action<object,ulong>
	// System.Action<object>
	// System.Action<uint,uint>
	// System.Action<uint>
	// System.Action<ulong>
	// System.Action<ushort>
	// System.ArraySegment.Enumerator<object>
	// System.ArraySegment<object>
	// System.Buffers.MemoryManager<System.ValueTuple<int,object>>
	// System.Buffers.MemoryManager<object>
	// System.ByReference<System.Collections.Generic.KeyValuePair<int,int>>
	// System.ByReference<System.Collections.Generic.KeyValuePair<int,object>>
	// System.ByReference<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.ByReference<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.ByReference<System.ValueTuple<int,object>>
	// System.ByReference<System.ValueTuple<object,object>>
	// System.ByReference<byte>
	// System.ByReference<object>
	// System.Collections.Generic.ArraySortHelper<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.ArraySortHelper<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.ArraySortHelper<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.ArraySortHelper<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.ArraySortHelper<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.ArraySortHelper<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<int,int>>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<object,object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Matrix4x4>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<long>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.ArraySortHelper<uint>
	// System.Collections.Generic.ArraySortHelper<ulong>
	// System.Collections.Generic.ArraySortHelper<ushort>
	// System.Collections.Generic.Comparer<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.Comparer<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.Comparer<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.Comparer<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.Comparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.Comparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,int>>
	// System.Collections.Generic.Comparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.Comparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.Comparer<UnityEngine.Matrix4x4>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<uint>
	// System.Collections.Generic.Comparer<ulong>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.ComparisonComparer<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.ComparisonComparer<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.ComparisonComparer<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.ComparisonComparer<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.ComparisonComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.ComparisonComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<int,int>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Matrix4x4>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ComparisonComparer<byte>
	// System.Collections.Generic.ComparisonComparer<int>
	// System.Collections.Generic.ComparisonComparer<long>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.ComparisonComparer<uint>
	// System.Collections.Generic.ComparisonComparer<ulong>
	// System.Collections.Generic.ComparisonComparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary.Enumerator<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary.Enumerator<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,uint>
	// System.Collections.Generic.Dictionary.Enumerator<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary.Enumerator<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary.Enumerator<uint,int>
	// System.Collections.Generic.Dictionary.Enumerator<uint,long>
	// System.Collections.Generic.Dictionary.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.Enumerator<uint,ushort>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,long>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,uint>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary.KeyCollection<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary.KeyCollection<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary.KeyCollection<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.KeyCollection<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,uint>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,int>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,long>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,uint>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,ushort>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,int>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,long>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,uint>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary.ValueCollection<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary.ValueCollection<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary.ValueCollection<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary.ValueCollection<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,uint>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,int>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,long>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,uint>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,ushort>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,int>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,long>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary<UnityEngine.PropertyName,double>
	// System.Collections.Generic.Dictionary<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.Dictionary<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Dictionary<int,byte>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,UnityEngine.Vector3>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,float>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<object,uint>
	// System.Collections.Generic.Dictionary<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.Dictionary<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.Dictionary<uint,int>
	// System.Collections.Generic.Dictionary<uint,long>
	// System.Collections.Generic.Dictionary<uint,object>
	// System.Collections.Generic.Dictionary<uint,uint>
	// System.Collections.Generic.Dictionary<uint,ushort>
	// System.Collections.Generic.Dictionary<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.Dictionary<ulong,int>
	// System.Collections.Generic.Dictionary<ulong,long>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.EqualityComparer<HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.EqualityComparer<HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.PropertyName>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<double>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<uint>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet.Enumerator<uint>
	// System.Collections.Generic.HashSet.Enumerator<ulong>
	// System.Collections.Generic.HashSet<System.ValueTuple<object,object>>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<uint>
	// System.Collections.Generic.HashSet<ulong>
	// System.Collections.Generic.HashSetEqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.HashSetEqualityComparer<uint>
	// System.Collections.Generic.HashSetEqualityComparer<ulong>
	// System.Collections.Generic.ICollection<HotUpdate.Config.MailDisplayConfig>
	// System.Collections.Generic.ICollection<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.ICollection<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.ICollection<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.ICollection<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.ICollection<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.ICollection<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<UnityEngine.PropertyName,double>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,HotUpdate.Config.PrototypeData>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,System.Collections.Generic.KeyValuePair<object,object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector3>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,System.Collections.Generic.KeyValuePair<uint,int>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,System.ValueTuple<object,object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.ICollection<System.ValueTuple<int,int>>
	// System.Collections.Generic.ICollection<System.ValueTuple<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ICollection<UnityEngine.Matrix4x4>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<long>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.ICollection<uint>
	// System.Collections.Generic.ICollection<ulong>
	// System.Collections.Generic.ICollection<ushort>
	// System.Collections.Generic.IComparer<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.IComparer<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.IComparer<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.IComparer<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.IComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.IComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,int>>
	// System.Collections.Generic.IComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.IComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IComparer<UnityEngine.Matrix4x4>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IComparer<uint>
	// System.Collections.Generic.IComparer<ulong>
	// System.Collections.Generic.IComparer<ushort>
	// System.Collections.Generic.IDictionary<int,int>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IDictionary<int,uint>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IDictionary<uint,long>
	// System.Collections.Generic.IDictionary<uint,object>
	// System.Collections.Generic.IDictionary<ulong,object>
	// System.Collections.Generic.IEnumerable<CapnpGen.CreateAsset.READER>
	// System.Collections.Generic.IEnumerable<CapnpGen.CreateFx.READER>
	// System.Collections.Generic.IEnumerable<CapnpGen.CreateInstance.READER>
	// System.Collections.Generic.IEnumerable<CapnpGen.DestroyAsset.READER>
	// System.Collections.Generic.IEnumerable<CapnpGen.PlayAnimation.READER>
	// System.Collections.Generic.IEnumerable<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.IEnumerable<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.IEnumerable<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.IEnumerable<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.IEnumerable<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.IEnumerable<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<UnityEngine.PropertyName,double>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,HotUpdate.Config.PrototypeData>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,System.Collections.Generic.KeyValuePair<object,object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector3>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,System.Collections.Generic.KeyValuePair<uint,int>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,System.ValueTuple<object,object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,int>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<int,uint>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerable<UnityEngine.Matrix4x4>
	// System.Collections.Generic.IEnumerable<UnityEngine.Playables.PlayableBinding>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<float>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<long>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<uint>
	// System.Collections.Generic.IEnumerable<ulong>
	// System.Collections.Generic.IEnumerable<ushort>
	// System.Collections.Generic.IEnumerator<CapnpGen.CreateAsset.READER>
	// System.Collections.Generic.IEnumerator<CapnpGen.CreateFx.READER>
	// System.Collections.Generic.IEnumerator<CapnpGen.CreateInstance.READER>
	// System.Collections.Generic.IEnumerator<CapnpGen.DestroyAsset.READER>
	// System.Collections.Generic.IEnumerator<CapnpGen.PlayAnimation.READER>
	// System.Collections.Generic.IEnumerator<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.IEnumerator<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.IEnumerator<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.IEnumerator<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.IEnumerator<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.IEnumerator<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<UnityEngine.PropertyName,double>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,HotUpdate.Config.PrototypeData>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,System.Collections.Generic.KeyValuePair<object,object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector3>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,System.Collections.Generic.KeyValuePair<uint,int>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,System.ValueTuple<object,object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,int>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<int,uint>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerator<UnityEngine.Matrix4x4>
	// System.Collections.Generic.IEnumerator<UnityEngine.Playables.PlayableBinding>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<float>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<long>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<uint>
	// System.Collections.Generic.IEnumerator<ulong>
	// System.Collections.Generic.IEnumerator<ushort>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>
	// System.Collections.Generic.IEqualityComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.IEqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEqualityComparer<UnityEngine.PropertyName>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<uint>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IList<HotUpdate.Config.MailDisplayConfig>
	// System.Collections.Generic.IList<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.IList<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.IList<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.IList<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.IList<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.IList<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IList<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.IList<System.ValueTuple<int,int>>
	// System.Collections.Generic.IList<System.ValueTuple<object,object>>
	// System.Collections.Generic.IList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IList<UnityEngine.Matrix4x4>
	// System.Collections.Generic.IList<UnityEngine.Vector2>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<long>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IList<uint>
	// System.Collections.Generic.IList<ulong>
	// System.Collections.Generic.IList<ushort>
	// System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IReadOnlyCollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IReadOnlyCollection<int>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyCollection<uint>
	// System.Collections.Generic.IReadOnlyCollection<ulong>
	// System.Collections.Generic.IReadOnlyDictionary<int,int>
	// System.Collections.Generic.IReadOnlyDictionary<int,long>
	// System.Collections.Generic.IReadOnlyDictionary<int,object>
	// System.Collections.Generic.IReadOnlyDictionary<int,uint>
	// System.Collections.Generic.IReadOnlyDictionary<uint,object>
	// System.Collections.Generic.IReadOnlyDictionary<ulong,object>
	// System.Collections.Generic.IReadOnlyList<int>
	// System.Collections.Generic.IReadOnlyList<uint>
	// System.Collections.Generic.IReadOnlyList<ulong>
	// System.Collections.Generic.KeyValuePair<UnityEngine.PropertyName,double>
	// System.Collections.Generic.KeyValuePair<int,HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.KeyValuePair<int,System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>
	// System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>
	// System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>
	// System.Collections.Generic.KeyValuePair<int,System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>
	// System.Collections.Generic.KeyValuePair<int,byte>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,uint>
	// System.Collections.Generic.KeyValuePair<long,long>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,UnityEngine.Vector3>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.KeyValuePair<object,float>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,long>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<object,uint>
	// System.Collections.Generic.KeyValuePair<uint,System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.KeyValuePair<uint,System.ValueTuple<object,object>>
	// System.Collections.Generic.KeyValuePair<uint,int>
	// System.Collections.Generic.KeyValuePair<uint,long>
	// System.Collections.Generic.KeyValuePair<uint,object>
	// System.Collections.Generic.KeyValuePair<uint,uint>
	// System.Collections.Generic.KeyValuePair<uint,ulong>
	// System.Collections.Generic.KeyValuePair<uint,ushort>
	// System.Collections.Generic.KeyValuePair<ulong,HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.KeyValuePair<ulong,int>
	// System.Collections.Generic.KeyValuePair<ulong,long>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.KeyValuePair<ulong,uint>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.LinkedList.Enumerator<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.LinkedList<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.LinkedListNode<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.List.Enumerator<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.List.Enumerator<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.List.Enumerator<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.List.Enumerator<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.List.Enumerator<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<int,int>>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Matrix4x4>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<uint>
	// System.Collections.Generic.List.Enumerator<ulong>
	// System.Collections.Generic.List.Enumerator<ushort>
	// System.Collections.Generic.List<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.List<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.List<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.List<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.List<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.List<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.List<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.List<System.ValueTuple<int,int>>
	// System.Collections.Generic.List<System.ValueTuple<object,object>>
	// System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List<UnityEngine.Matrix4x4>
	// System.Collections.Generic.List<UnityEngine.Vector2>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<uint>
	// System.Collections.Generic.List<ulong>
	// System.Collections.Generic.List<ushort>
	// System.Collections.Generic.ObjectComparer<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.Generic.ObjectComparer<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.Generic.ObjectComparer<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.Generic.ObjectComparer<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.Generic.ObjectComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.Generic.ObjectComparer<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,int,int>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,int>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Matrix4x4>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<uint>
	// System.Collections.Generic.ObjectComparer<ulong>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<HotUpdate.Config.PrototypeData>
	// System.Collections.Generic.ObjectEqualityComparer<HotUpdate.Product.Modules.Player.RequestPlayerInfoData>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<long,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,ulong>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<ulong,int>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<ulong,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<ulong,uint>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<int,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.PropertyName>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<double>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<uint>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Queue.Enumerator<int>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<int>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<int,int>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<int,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<int,uint>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<uint,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<int,int>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<int,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<int,uint>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<uint,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<int,int>
	// System.Collections.Generic.SortedDictionary.Enumerator<int,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<int,uint>
	// System.Collections.Generic.SortedDictionary.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<int,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<int,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<int,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<int,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<int,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<int,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<int,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<int,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection<int,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<int,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<int,int>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<int,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<int,uint>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<int,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<int,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<int,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<int,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<int,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<int,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection<uint,object>
	// System.Collections.Generic.SortedDictionary<int,int>
	// System.Collections.Generic.SortedDictionary<int,object>
	// System.Collections.Generic.SortedDictionary<int,uint>
	// System.Collections.Generic.SortedDictionary<uint,object>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<int>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<int>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.Enumerator<int>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.Node<int>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet<int>
	// System.Collections.Generic.Stack.Enumerator<System.Collections.Immutable.RefAsValueType<object>>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<System.Collections.Immutable.RefAsValueType<object>>
	// System.Collections.Generic.Stack<object>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.TreeWalkPredicate<int>
	// System.Collections.Immutable.AllocFreeConcurrentStack<object>
	// System.Collections.Immutable.DictionaryEnumerator<int,int>
	// System.Collections.Immutable.DictionaryEnumerator<int,object>
	// System.Collections.Immutable.DictionaryEnumerator<int,uint>
	// System.Collections.Immutable.DictionaryEnumerator<uint,object>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<System.Collections.Generic.KeyValuePair<int,int>,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<System.Collections.Generic.KeyValuePair<int,object>,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<System.Collections.Generic.KeyValuePair<int,uint>,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,uint>>>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<System.Collections.Generic.KeyValuePair<uint,object>,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<System.ValueTuple<object,object>,System.Collections.Immutable.ImmutableList.Enumerator<System.ValueTuple<object,object>>>
	// System.Collections.Immutable.DisposableEnumeratorAdapter<object,System.Collections.Immutable.ImmutableList.Enumerator<object>>
	// System.Collections.Immutable.IImmutableDictionaryInternal<int,int>
	// System.Collections.Immutable.IImmutableDictionaryInternal<int,object>
	// System.Collections.Immutable.IImmutableDictionaryInternal<int,uint>
	// System.Collections.Immutable.IImmutableDictionaryInternal<uint,object>
	// System.Collections.Immutable.IOrderedCollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Immutable.IOrderedCollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Immutable.IOrderedCollection<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Immutable.IOrderedCollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Immutable.IOrderedCollection<System.ValueTuple<object,object>>
	// System.Collections.Immutable.IOrderedCollection<object>
	// System.Collections.Immutable.ImmutableArray.Builder.<GetEnumerator>d__66<System.ValueTuple<int,object>>
	// System.Collections.Immutable.ImmutableArray.Builder.<GetEnumerator>d__66<object>
	// System.Collections.Immutable.ImmutableArray.Builder<System.ValueTuple<int,object>>
	// System.Collections.Immutable.ImmutableArray.Builder<object>
	// System.Collections.Immutable.ImmutableArray.Enumerator<System.ValueTuple<int,object>>
	// System.Collections.Immutable.ImmutableArray.Enumerator<object>
	// System.Collections.Immutable.ImmutableArray.EnumeratorObject<System.ValueTuple<int,object>>
	// System.Collections.Immutable.ImmutableArray.EnumeratorObject<object>
	// System.Collections.Immutable.ImmutableArray<System.ValueTuple<int,object>>
	// System.Collections.Immutable.ImmutableArray<object>
	// System.Collections.Immutable.ImmutableDictionary.<>c<int,int>
	// System.Collections.Immutable.ImmutableDictionary.<>c<int,object>
	// System.Collections.Immutable.ImmutableDictionary.<>c<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.<>c<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.<get_Keys>d__25<int,int>
	// System.Collections.Immutable.ImmutableDictionary.<get_Keys>d__25<int,object>
	// System.Collections.Immutable.ImmutableDictionary.<get_Keys>d__25<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.<get_Keys>d__25<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.<get_Values>d__27<int,int>
	// System.Collections.Immutable.ImmutableDictionary.<get_Values>d__27<int,object>
	// System.Collections.Immutable.ImmutableDictionary.<get_Values>d__27<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.<get_Values>d__27<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Keys>d__18<int,int>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Keys>d__18<int,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Keys>d__18<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Keys>d__18<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Values>d__22<int,int>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Values>d__22<int,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Values>d__22<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.Builder.<get_Values>d__22<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder<int,int>
	// System.Collections.Immutable.ImmutableDictionary.Builder<int,object>
	// System.Collections.Immutable.ImmutableDictionary.Builder<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.Builder<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.Comparers<int,int>
	// System.Collections.Immutable.ImmutableDictionary.Comparers<int,object>
	// System.Collections.Immutable.ImmutableDictionary.Comparers<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.Comparers<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.Enumerator<int,int>
	// System.Collections.Immutable.ImmutableDictionary.Enumerator<int,object>
	// System.Collections.Immutable.ImmutableDictionary.Enumerator<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.Enumerator<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket.Enumerator<int,int>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket.Enumerator<int,object>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket.Enumerator<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket.Enumerator<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.MutationInput<int,int>
	// System.Collections.Immutable.ImmutableDictionary.MutationInput<int,object>
	// System.Collections.Immutable.ImmutableDictionary.MutationInput<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.MutationInput<uint,object>
	// System.Collections.Immutable.ImmutableDictionary.MutationResult<int,int>
	// System.Collections.Immutable.ImmutableDictionary.MutationResult<int,object>
	// System.Collections.Immutable.ImmutableDictionary.MutationResult<int,uint>
	// System.Collections.Immutable.ImmutableDictionary.MutationResult<uint,object>
	// System.Collections.Immutable.ImmutableDictionary<int,int>
	// System.Collections.Immutable.ImmutableDictionary<int,object>
	// System.Collections.Immutable.ImmutableDictionary<int,uint>
	// System.Collections.Immutable.ImmutableDictionary<uint,object>
	// System.Collections.Immutable.ImmutableList.Builder<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Immutable.ImmutableList.Builder<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Immutable.ImmutableList.Builder<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Immutable.ImmutableList.Builder<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Immutable.ImmutableList.Builder<System.ValueTuple<object,object>>
	// System.Collections.Immutable.ImmutableList.Builder<object>
	// System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Immutable.ImmutableList.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Immutable.ImmutableList.Enumerator<object>
	// System.Collections.Immutable.ImmutableList.Node<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Immutable.ImmutableList.Node<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Immutable.ImmutableList.Node<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Immutable.ImmutableList.Node<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Immutable.ImmutableList.Node<System.ValueTuple<object,object>>
	// System.Collections.Immutable.ImmutableList.Node<object>
	// System.Collections.Immutable.ImmutableList<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Immutable.ImmutableList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Immutable.ImmutableList<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Immutable.ImmutableList<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Immutable.ImmutableList<System.ValueTuple<object,object>>
	// System.Collections.Immutable.ImmutableList<object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Builder<int,int>
	// System.Collections.Immutable.ImmutableSortedDictionary.Builder<int,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Builder<int,uint>
	// System.Collections.Immutable.ImmutableSortedDictionary.Builder<uint,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,int>
	// System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,uint>
	// System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<uint,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node.<>c<int,int>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node.<>c<int,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node.<>c<int,uint>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node.<>c<uint,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node<int,int>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node<int,object>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node<int,uint>
	// System.Collections.Immutable.ImmutableSortedDictionary.Node<uint,object>
	// System.Collections.Immutable.ImmutableSortedDictionary<int,int>
	// System.Collections.Immutable.ImmutableSortedDictionary<int,object>
	// System.Collections.Immutable.ImmutableSortedDictionary<int,uint>
	// System.Collections.Immutable.ImmutableSortedDictionary<uint,object>
	// System.Collections.Immutable.KeysCollectionAccessor<int,int>
	// System.Collections.Immutable.KeysCollectionAccessor<int,object>
	// System.Collections.Immutable.KeysCollectionAccessor<int,uint>
	// System.Collections.Immutable.KeysCollectionAccessor<uint,object>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<int,int,int>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<int,object,int>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<int,object,object>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<int,uint,int>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<int,uint,uint>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<uint,object,object>
	// System.Collections.Immutable.KeysOrValuesCollectionAccessor<uint,object,uint>
	// System.Collections.Immutable.RefAsValueType<object>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,int>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<int,uint>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<System.ValueTuple<object,object>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableList.Enumerator<object>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,int>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,object>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<int,uint>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.ImmutableSortedDictionary.Enumerator<uint,object>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>>
	// System.Collections.Immutable.SecureObjectPool<object,System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>>
	// System.Collections.Immutable.SecurePooledObject<object>
	// System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>
	// System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>
	// System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>
	// System.Collections.Immutable.SortedInt32KeyNode.Enumerator<System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>
	// System.Collections.Immutable.SortedInt32KeyNode<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,int>>
	// System.Collections.Immutable.SortedInt32KeyNode<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,object>>
	// System.Collections.Immutable.SortedInt32KeyNode<System.Collections.Immutable.ImmutableDictionary.HashBucket<int,uint>>
	// System.Collections.Immutable.SortedInt32KeyNode<System.Collections.Immutable.ImmutableDictionary.HashBucket<uint,object>>
	// System.Collections.Immutable.ValuesCollectionAccessor<int,int>
	// System.Collections.Immutable.ValuesCollectionAccessor<int,object>
	// System.Collections.Immutable.ValuesCollectionAccessor<int,uint>
	// System.Collections.Immutable.ValuesCollectionAccessor<uint,object>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotUpdate.Config.TCommonFSMCondition>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Collections.ObjectModel.ReadOnlyCollection<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<int,int,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<int,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Matrix4x4>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<long>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<uint>
	// System.Collections.ObjectModel.ReadOnlyCollection<ulong>
	// System.Collections.ObjectModel.ReadOnlyCollection<ushort>
	// System.Comparison<HotUpdate.Config.TCommonFSMCondition>
	// System.Comparison<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Comparison<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Comparison<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Comparison<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Comparison<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Comparison<System.ValueTuple<int,int,int>>
	// System.Comparison<System.ValueTuple<int,int>>
	// System.Comparison<System.ValueTuple<int,object>>
	// System.Comparison<System.ValueTuple<object,object>>
	// System.Comparison<UnityEngine.EventSystems.RaycastResult>
	// System.Comparison<UnityEngine.Matrix4x4>
	// System.Comparison<UnityEngine.Vector2>
	// System.Comparison<byte>
	// System.Comparison<int>
	// System.Comparison<long>
	// System.Comparison<object>
	// System.Comparison<uint>
	// System.Comparison<ulong>
	// System.Comparison<ushort>
	// System.Func<System.Collections.Generic.KeyValuePair<int,byte>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<int,byte>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,byte>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,int>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<int,int>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,int>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<int,uint>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,uint>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<long,long>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<long,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<long,long>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<long,object>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<long,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<long,object>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<long,object>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,long>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<object,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<object,long>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<object,long>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,int>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,int>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,int>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,int>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,long>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,long>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,long>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,object>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,object>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,object>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,uint>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,uint>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,uint>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,ulong>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,ulong>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,ulong>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<uint,ulong>,ulong>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,int>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,int>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,int>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,int>,ulong>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,long>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,long>,long>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,long>,ulong>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,object>,ulong>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,uint>,System.Collections.DictionaryEntry>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,uint>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,uint>,uint>
	// System.Func<System.Collections.Generic.KeyValuePair<ulong,uint>,ulong>
	// System.Func<byte,byte>
	// System.Func<byte,int>
	// System.Func<float,float,float,float>
	// System.Func<float,int>
	// System.Func<int,byte>
	// System.Func<int,int>
	// System.Func<int,object,object>
	// System.Func<int,object>
	// System.Func<int>
	// System.Func<long,byte>
	// System.Func<long,int>
	// System.Func<long>
	// System.Func<object,byte>
	// System.Func<object,int>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object,uint>
	// System.Func<object,ulong>
	// System.Func<object>
	// System.Func<uint,byte>
	// System.Func<uint,int>
	// System.Func<uint,object>
	// System.Func<uint,uint>
	// System.Func<ulong,byte>
	// System.Func<ulong,int>
	// System.Func<ulong,object>
	// System.IComparable<object>
	// System.IEquatable<object>
	// System.Linq.Buffer<int>
	// System.Linq.Buffer<object>
	// System.Linq.Buffer<uint>
	// System.Linq.Enumerable.<AppendIterator>d__61<object>
	// System.Linq.Enumerable.<CastIterator>d__99<int>
	// System.Linq.Enumerable.<CastIterator>d__99<object>
	// System.Linq.Enumerable.<PrependIterator>d__63<byte>
	// System.Linq.Enumerable.<SelectManyIterator>d__17<object,object>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Linq.Enumerable.Iterator<byte>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.Iterator<uint>
	// System.Linq.Enumerable.Iterator<ulong>
	// System.Linq.Enumerable.WhereArrayIterator<byte>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<byte>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<uint>
	// System.Linq.Enumerable.WhereEnumerableIterator<ulong>
	// System.Linq.Enumerable.WhereListIterator<byte>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<int,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,uint>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,ulong>
	// System.Linq.Enumerable.WhereSelectArrayIterator<uint,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<ulong,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<int,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,uint>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,ulong>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<uint,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<ulong,object>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectListIterator<int,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,uint>
	// System.Linq.Enumerable.WhereSelectListIterator<object,ulong>
	// System.Linq.Enumerable.WhereSelectListIterator<uint,object>
	// System.Linq.Enumerable.WhereSelectListIterator<ulong,object>
	// System.Linq.EnumerableSorter<object,int>
	// System.Linq.EnumerableSorter<object,uint>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.EnumerableSorter<uint,uint>
	// System.Linq.EnumerableSorter<uint>
	// System.Linq.IOrderedEnumerable<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<uint>
	// System.Linq.OrderedEnumerable<object,int>
	// System.Linq.OrderedEnumerable<object,uint>
	// System.Linq.OrderedEnumerable<object>
	// System.Linq.OrderedEnumerable<uint,uint>
	// System.Linq.OrderedEnumerable<uint>
	// System.Memory<System.ValueTuple<int,object>>
	// System.Memory<object>
	// System.Nullable<HotUpdate.Config.MailDisplayConfig>
	// System.Nullable<System.Collections.Immutable.ImmutableArray<System.ValueTuple<int,object>>>
	// System.Nullable<System.Collections.Immutable.ImmutableArray<object>>
	// System.Nullable<System.DateTime>
	// System.Nullable<UnityEngine.Vector3>
	// System.Nullable<byte>
	// System.Nullable<float>
	// System.Nullable<long>
	// System.Nullable<ulong>
	// System.Predicate<HotUpdate.Config.TCommonFSMCondition>
	// System.Predicate<HotUpdate.Config.TCommonFSMTransitionConfig>
	// System.Predicate<HotUpdate.Product.Modules.Facility.OreResourcePerLevel>
	// System.Predicate<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>
	// System.Predicate<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<object>>
	// System.Predicate<Script.HotUpdate.Product.MapData.MapGenericQuadTreeNode<ushort>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<uint,int>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Predicate<System.ValueTuple<int,int,int>>
	// System.Predicate<System.ValueTuple<int,int>>
	// System.Predicate<System.ValueTuple<int,object>>
	// System.Predicate<System.ValueTuple<object,object>>
	// System.Predicate<UnityEngine.EventSystems.RaycastResult>
	// System.Predicate<UnityEngine.Matrix4x4>
	// System.Predicate<UnityEngine.Vector2>
	// System.Predicate<byte>
	// System.Predicate<int>
	// System.Predicate<long>
	// System.Predicate<object>
	// System.Predicate<uint>
	// System.Predicate<ulong>
	// System.Predicate<ushort>
	// System.ReadOnlyMemory<System.ValueTuple<int,object>>
	// System.ReadOnlyMemory<object>
	// System.ReadOnlySpan<System.Collections.Generic.KeyValuePair<int,int>>
	// System.ReadOnlySpan<System.Collections.Generic.KeyValuePair<int,object>>
	// System.ReadOnlySpan<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.ReadOnlySpan<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.ReadOnlySpan<System.ValueTuple<int,object>>
	// System.ReadOnlySpan<System.ValueTuple<object,object>>
	// System.ReadOnlySpan<byte>
	// System.ReadOnlySpan<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.ITextEvaluateEnv<object>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Span<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Span<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Span<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Span<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Span<System.ValueTuple<int,object>>
	// System.Span<System.ValueTuple<object,object>>
	// System.Span<byte>
	// System.Span<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<object>
	// System.Tuple<object,int>
	// System.Tuple<uint,byte,object>
	// System.ValueTuple<int,int,int>
	// System.ValueTuple<int,int>
	// System.ValueTuple<int,object>
	// System.ValueTuple<int,uint>
	// System.ValueTuple<object,object>
	// UnityEngine.Events.InvokableCall<UnityEngine.Vector2>
	// UnityEngine.Events.InvokableCall<byte>
	// UnityEngine.Events.InvokableCall<float>
	// UnityEngine.Events.InvokableCall<int>
	// UnityEngine.Events.InvokableCall<object,float>
	// UnityEngine.Events.InvokableCall<object,int>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.InvokableCall<ulong>
	// UnityEngine.Events.UnityAction<UnityEngine.Vector2>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityAction<object,float>
	// UnityEngine.Events.UnityAction<object,int>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityAction<ulong>
	// UnityEngine.Events.UnityEvent<UnityEngine.Vector2>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<float>
	// UnityEngine.Events.UnityEvent<int>
	// UnityEngine.Events.UnityEvent<object,float>
	// UnityEngine.Events.UnityEvent<object,int>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.Events.UnityEvent<ulong>
	// }}

	public void RefMethods()
	{
		// bool DesperateDevs.Utils.InterfaceTypeExtension.ImplementsInterface<object>(System.Type)
		// System.Void Framework.Debug.DebugLogConsole.AddCommand<object>(string,string,System.Action<object>)
		// object Framework.JsonHelper.FromJson<object>(string)
		// object Framework.JsonHelper.FromJsonNewtonsoft<object>(string)
		// object Framework.JsonHelper.GetProperty<object>(Newtonsoft.Json.Linq.JObject,string)
		// uint Framework.JsonHelper.GetProperty<uint>(Newtonsoft.Json.Linq.JObject,string)
		// object Google.Protobuf.ExtensionSet.Get<object,object>(Google.Protobuf.ExtensionSet<object>&,Google.Protobuf.Extension<object,object>)
		// uint Google.Protobuf.ExtensionSet.Get<object,uint>(Google.Protobuf.ExtensionSet<object>&,Google.Protobuf.Extension<object,uint>)
		// bool Google.Protobuf.ExtensionSet.TryGetValue<object>(Google.Protobuf.ExtensionSet<object>&,Google.Protobuf.Extension,Google.Protobuf.IExtensionValue&)
		// Google.Protobuf.FieldCodec<int> Google.Protobuf.FieldCodec.ForEnum<int>(uint,System.Func<int,int>,System.Func<int,int>)
		// Google.Protobuf.FieldCodec<int> Google.Protobuf.FieldCodec.ForEnum<int>(uint,System.Func<int,int>,System.Func<int,int>,int)
		// object Google.Protobuf.Reflection.EnumValueOptions.GetExtension<object>(Google.Protobuf.Extension<Google.Protobuf.Reflection.EnumValueOptions,object>)
		// object Google.Protobuf.Reflection.FieldOptions.GetExtension<object>(Google.Protobuf.Extension<Google.Protobuf.Reflection.FieldOptions,object>)
		// object Google.Protobuf.Reflection.MessageOptions.GetExtension<object>(Google.Protobuf.Extension<Google.Protobuf.Reflection.MessageOptions,object>)
		// uint Google.Protobuf.Reflection.MessageOptions.GetExtension<uint>(Google.Protobuf.Extension<Google.Protobuf.Reflection.MessageOptions,uint>)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// object Newtonsoft.Json.Linq.Extensions.Convert<object,object>(object)
		// uint Newtonsoft.Json.Linq.Extensions.Convert<object,uint>(object)
		// object Newtonsoft.Json.Linq.Extensions.Value<object,object>(System.Collections.Generic.IEnumerable<object>)
		// object Newtonsoft.Json.Linq.Extensions.Value<object>(System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken>)
		// uint Newtonsoft.Json.Linq.Extensions.Value<object,uint>(System.Collections.Generic.IEnumerable<object>)
		// uint Newtonsoft.Json.Linq.Extensions.Value<uint>(System.Collections.Generic.IEnumerable<Newtonsoft.Json.Linq.JToken>)
		// System.Void Sirenix.Utilities.LinqExtensions.AddRange<ulong>(System.Collections.Generic.HashSet<ulong>,System.Collections.Generic.IEnumerable<ulong>)
		// object System.Activator.CreateInstance<object>()
		// object[] System.Array.Empty<object>()
		// uint[] System.Array.Empty<uint>()
		// ulong[] System.Array.Empty<ulong>()
		// System.Void System.Array.Fill<int>(int[],int)
		// System.Void System.Array.Resize<object>(object[]&,int)
		// System.Void System.Array.Reverse<ushort>(ushort[])
		// System.Void System.Array.Reverse<ushort>(ushort[],int,int)
		// System.Void System.Array.Sort<int>(int[])
		// System.Void System.Array.Sort<int>(int[],int,int,System.Collections.Generic.IComparer<int>)
		// System.Void System.CollectionExtension.MoveToTop<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>(System.Collections.Generic.IList<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>,HotUpdate.Product.UI.UIDomain.UIStackRecord<object>)
		// System.Void System.CollectionExtension.MoveToTop<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>(System.Collections.Generic.IList<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>,int)
		// long System.Collections.Generic.CollectionExtensions.GetValueOrDefault<int,long>(System.Collections.Generic.IReadOnlyDictionary<int,long>,int,long)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<int,object>(System.Collections.Generic.IReadOnlyDictionary<int,object>,int)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<int,object>(System.Collections.Generic.IReadOnlyDictionary<int,object>,int,object)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<uint,object>(System.Collections.Generic.IReadOnlyDictionary<uint,object>,uint)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<uint,object>(System.Collections.Generic.IReadOnlyDictionary<uint,object>,uint,object)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<ulong,object>(System.Collections.Generic.IReadOnlyDictionary<ulong,object>,ulong)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<ulong,object>(System.Collections.Generic.IReadOnlyDictionary<ulong,object>,ulong,object)
		// System.Collections.Generic.KeyValuePair<int,object> System.Collections.Generic.KeyValuePair.Create<int,object>(int,object)
		// System.Collections.Immutable.ImmutableArray<System.ValueTuple<int,object>> System.Collections.Immutable.ImmutableArray.Create<System.ValueTuple<int,object>>()
		// System.Collections.Immutable.ImmutableArray.Builder<System.ValueTuple<int,object>> System.Collections.Immutable.ImmutableArray.CreateBuilder<System.ValueTuple<int,object>>()
		// System.Collections.Immutable.ImmutableArray<object> System.Collections.Immutable.ImmutableArray.CreateRange<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Immutable.ImmutableArray<System.ValueTuple<int,object>> System.Collections.Immutable.ImmutableArray.ToImmutableArray<System.ValueTuple<int,object>>(System.Collections.Immutable.ImmutableArray.Builder<System.ValueTuple<int,object>>)
		// System.Collections.Immutable.ImmutableArray<object> System.Collections.Immutable.ImmutableArray.ToImmutableArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Immutable.ImmutableDictionary<int,int> System.Collections.Immutable.ImmutableDictionary.Create<int,int>()
		// System.Collections.Immutable.ImmutableDictionary<int,uint> System.Collections.Immutable.ImmutableDictionary.Create<int,uint>()
		// System.Collections.Immutable.ImmutableDictionary.Builder<int,int> System.Collections.Immutable.ImmutableDictionary.CreateBuilder<int,int>()
		// System.Collections.Immutable.ImmutableDictionary.Builder<int,uint> System.Collections.Immutable.ImmutableDictionary.CreateBuilder<int,uint>()
		// System.Collections.Immutable.ImmutableDictionary<int,object> System.Collections.Immutable.ImmutableDictionary.CreateRange<int,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>)
		// System.Collections.Immutable.ImmutableDictionary<int,object> System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary<int,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>)
		// System.Collections.Immutable.ImmutableDictionary<int,object> System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary<int,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Collections.Generic.IEqualityComparer<int>,System.Collections.Generic.IEqualityComparer<object>)
		// System.Collections.Immutable.ImmutableDictionary<uint,object> System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary<uint,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,object>>)
		// System.Collections.Immutable.ImmutableDictionary<uint,object> System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary<uint,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,object>>,System.Collections.Generic.IEqualityComparer<uint>,System.Collections.Generic.IEqualityComparer<object>)
		// object[] System.Collections.Immutable.ImmutableExtensions.ToArray<object>(System.Collections.Generic.IEnumerable<object>,int)
		// bool System.Collections.Immutable.ImmutableExtensions.TryCopyTo<object>(System.Collections.Generic.IEnumerable<object>,object[],int)
		// bool System.Collections.Immutable.ImmutableExtensions.TryGetCount<object>(System.Collections.Generic.IEnumerable<object>,int&)
		// bool System.Collections.Immutable.ImmutableExtensions.TryGetCount<object>(System.Collections.IEnumerable,int&)
		// System.Collections.Immutable.ImmutableList<System.ValueTuple<object,object>> System.Collections.Immutable.ImmutableList.ToImmutableList<System.ValueTuple<object,object>>(System.Collections.Generic.IEnumerable<System.ValueTuple<object,object>>)
		// System.Collections.Immutable.ImmutableList<object> System.Collections.Immutable.ImmutableList.ToImmutableList<object>(System.Collections.Generic.IEnumerable<object>)
		// bool System.Enum.TryParse<int>(string,bool,int&)
		// bool System.Enum.TryParse<int>(string,int&)
		// bool System.Linq.Enumerable.Any<int>(System.Collections.Generic.IEnumerable<int>)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Append<object>(System.Collections.Generic.IEnumerable<object>,object)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.AppendIterator<object>(System.Collections.Generic.IEnumerable<object>,object)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Cast<int>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Cast<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.CastIterator<int>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.CastIterator<object>(System.Collections.IEnumerable)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// int System.Linq.Enumerable.Count<uint>(System.Collections.Generic.IEnumerable<uint>)
		// int System.Linq.Enumerable.Count<ulong>(System.Collections.Generic.IEnumerable<ulong>,System.Func<ulong,bool>)
		// object System.Linq.Enumerable.ElementAt<object>(System.Collections.Generic.IEnumerable<object>,int)
		// object System.Linq.Enumerable.ElementAtOrDefault<object>(System.Collections.Generic.IEnumerable<object>,int)
		// uint System.Linq.Enumerable.ElementAtOrDefault<uint>(System.Collections.Generic.IEnumerable<uint>,int)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// HotUpdate.Product.UI.UIDomain.UIStackRecord<object> System.Linq.Enumerable.Last<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>(System.Collections.Generic.IEnumerable<HotUpdate.Product.UI.UIDomain.UIStackRecord<object>>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// int System.Linq.Enumerable.Min<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<uint> System.Linq.Enumerable.OrderBy<uint,uint>(System.Collections.Generic.IEnumerable<uint>,System.Func<uint,uint>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderByDescending<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderByDescending<object,uint>(System.Collections.Generic.IEnumerable<object>,System.Func<object,uint>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Prepend<byte>(System.Collections.Generic.IEnumerable<byte>,byte)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.PrependIterator<byte>(System.Collections.Generic.IEnumerable<byte>,byte)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<object,object>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>,System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<uint,object>(System.Collections.Generic.IEnumerable<uint>,System.Func<uint,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<ulong,object>(System.Collections.Generic.IEnumerable<ulong>,System.Func<ulong,object>)
		// System.Collections.Generic.IEnumerable<uint> System.Linq.Enumerable.Select<object,uint>(System.Collections.Generic.IEnumerable<object>,System.Func<object,uint>)
		// System.Collections.Generic.IEnumerable<ulong> System.Linq.Enumerable.Select<object,ulong>(System.Collections.Generic.IEnumerable<object>,System.Func<object,ulong>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectMany<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectManyIterator<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<object>>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.ThenByDescending<object,int>(System.Linq.IOrderedEnumerable<object>,System.Func<object,int>)
		// int[] System.Linq.Enumerable.ToArray<int>(System.Collections.Generic.IEnumerable<int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<int,int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>,System.Func<int,object>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<int,int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>,System.Func<int,object>,System.Collections.Generic.IEqualityComparer<int>)
		// System.Collections.Generic.Dictionary<object,int> System.Linq.Enumerable.ToDictionary<int,object,int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,object>,System.Func<int,int>,System.Collections.Generic.IEqualityComparer<object>)
		// System.Collections.Generic.Dictionary<uint,int> System.Linq.Enumerable.ToDictionary<object,uint,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,uint>,System.Func<object,int>)
		// System.Collections.Generic.Dictionary<uint,int> System.Linq.Enumerable.ToDictionary<object,uint,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,uint>,System.Func<object,int>,System.Collections.Generic.IEqualityComparer<uint>)
		// System.Collections.Generic.List<byte> System.Linq.Enumerable.ToList<byte>(System.Collections.Generic.IEnumerable<byte>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<uint> System.Linq.Enumerable.ToList<uint>(System.Collections.Generic.IEnumerable<uint>)
		// System.Collections.Generic.List<ulong> System.Linq.Enumerable.ToList<ulong>(System.Collections.Generic.IEnumerable<ulong>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Where<byte>(System.Collections.Generic.IEnumerable<byte>,System.Func<byte,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>.Select<object>(System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<int>.Select<object>(System.Func<int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<uint>.Select<object>(System.Func<uint,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<ulong>.Select<object>(System.Func<ulong,object>)
		// System.Collections.Generic.IEnumerable<uint> System.Linq.Enumerable.Iterator<object>.Select<uint>(System.Func<object,uint>)
		// System.Collections.Generic.IEnumerable<ulong> System.Linq.Enumerable.Iterator<object>.Select<ulong>(System.Func<object,ulong>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.IOrderedEnumerable<object>.CreateOrderedEnumerable<int>(System.Func<object,int>,System.Collections.Generic.IComparer<int>,bool)
		// bool System.Linq.ImmutableArrayExtensions.Any<object>(System.Collections.Immutable.ImmutableArray<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.ImmutableArrayExtensions.Where<object>(System.Collections.Immutable.ImmutableArray<object>,System.Func<object,bool>)
		// object System.Reflection.CustomAttributeExtensions.GetCustomAttribute<object>(System.Reflection.MemberInfo)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Framework.Network.Session.NetSession.<Connect>d__11>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Framework.Network.Session.NetSession.<Connect>d__11&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Framework.Network.Session.NetSession.<Connect>d__11>(HotUpdate.Framework.Network.Session.NetSession.<Connect>d__11&)
		// long System.Runtime.CompilerServices.TextCompiler.Evaluate<long>(System.Runtime.CompilerServices.ITextEvaluateEnv<long>)
		// object System.Runtime.CompilerServices.TextCompiler.Evaluate<object>(System.Runtime.CompilerServices.ITextEvaluateEnv<object>)
		// long System.Runtime.CompilerServices.TextCompiler.CompiledMethod.Invoke<long>(System.Runtime.CompilerServices.TextCompiler,System.Runtime.CompilerServices.ITextEvaluateEnv<long>)
		// object System.Runtime.CompilerServices.TextCompiler.CompiledMethod.Invoke<object>(System.Runtime.CompilerServices.TextCompiler,System.Runtime.CompilerServices.ITextEvaluateEnv<object>)
		// ushort& System.Runtime.CompilerServices.Unsafe.Add<ushort>(ushort&,int)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// ushort& System.Runtime.CompilerServices.Unsafe.As<byte,ushort>(byte&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// bool System.Runtime.CompilerServices.Unsafe.IsAddressLessThan<ushort>(ushort&,ushort&)
		// System.IntPtr System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate<object>(object)
		// object TinyJson.JSONParser.FromJson<object>(string)
		// System.Void UniFramework.Event.UniEvent.AddListener<object>(System.Action<UniFramework.Event.IEventMessage>)
		// System.Void UniFramework.Event.UniEvent.RemoveListener<object>(System.Action<UniFramework.Event.IEventMessage>)
		// System.Void UniFramework.Machine.StateMachine.ChangeState<object>()
		// System.Void UniFramework.Machine.StateMachine.Run<object>()
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponents<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// object[] UnityEngine.GameObject.GetComponents<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object[] UnityEngine.Object.FindObjectsOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Vector3,UnityEngine.Quaternion)
		// object[] UnityEngine.Resources.ConvertObjects<object>(UnityEngine.Object[])
		// object YooAsset.AssetHandle.GetAssetObject<object>()
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetSync<object>(string)
	}
}
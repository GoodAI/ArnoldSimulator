// automatically generated, do not modify

using System;
using FlatBuffers;

public sealed class Position : Table {
  public static Position GetRootAsPosition(ByteBuffer _bb) { return GetRootAsPosition(_bb, new Position()); }
  public static Position GetRootAsPosition(ByteBuffer _bb, Position obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Position __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public float X { get { int o = __offset(4); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Y { get { int o = __offset(6); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Z { get { int o = __offset(8); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }

  public static Offset<Position> CreatePosition(FlatBufferBuilder builder,
      float x = 0,
      float y = 0,
      float z = 0) {
    builder.StartObject(3);
    Position.AddZ(builder, z);
    Position.AddY(builder, y);
    Position.AddX(builder, x);
    return Position.EndPosition(builder);
  }

  public static void StartPosition(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddX(FlatBufferBuilder builder, float x) { builder.AddFloat(0, x, 0); }
  public static void AddY(FlatBufferBuilder builder, float y) { builder.AddFloat(1, y, 0); }
  public static void AddZ(FlatBufferBuilder builder, float z) { builder.AddFloat(2, z, 0); }
  public static Offset<Position> EndPosition(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Position>(o);
  }
};

public sealed class NeuronId : Table {
  public static NeuronId GetRootAsNeuronId(ByteBuffer _bb) { return GetRootAsNeuronId(_bb, new NeuronId()); }
  public static NeuronId GetRootAsNeuronId(ByteBuffer _bb, NeuronId obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public NeuronId __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint Neuron { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public uint Region { get { int o = __offset(6); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<NeuronId> CreateNeuronId(FlatBufferBuilder builder,
      uint neuron = 0,
      uint region = 0) {
    builder.StartObject(2);
    NeuronId.AddRegion(builder, region);
    NeuronId.AddNeuron(builder, neuron);
    return NeuronId.EndNeuronId(builder);
  }

  public static void StartNeuronId(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddNeuron(FlatBufferBuilder builder, uint neuron) { builder.AddUint(0, neuron, 0); }
  public static void AddRegion(FlatBufferBuilder builder, uint region) { builder.AddUint(1, region, 0); }
  public static Offset<NeuronId> EndNeuronId(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<NeuronId>(o);
  }
};

public sealed class Observer : Table {
  public static Observer GetRootAsObserver(ByteBuffer _bb) { return GetRootAsObserver(_bb, new Observer()); }
  public static Observer GetRootAsObserver(ByteBuffer _bb, Observer obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Observer __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public NeuronId NeuronId { get { return GetNeuronId(new NeuronId()); } }
  public NeuronId GetNeuronId(NeuronId obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public string Type { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetTypeBytes() { return __vector_as_arraysegment(6); }

  public static Offset<Observer> CreateObserver(FlatBufferBuilder builder,
      Offset<NeuronId> neuronIdOffset = default(Offset<NeuronId>),
      StringOffset typeOffset = default(StringOffset)) {
    builder.StartObject(2);
    Observer.AddType(builder, typeOffset);
    Observer.AddNeuronId(builder, neuronIdOffset);
    return Observer.EndObserver(builder);
  }

  public static void StartObserver(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddNeuronId(FlatBufferBuilder builder, Offset<NeuronId> neuronIdOffset) { builder.AddOffset(0, neuronIdOffset.Value, 0); }
  public static void AddType(FlatBufferBuilder builder, StringOffset typeOffset) { builder.AddOffset(1, typeOffset.Value, 0); }
  public static Offset<Observer> EndObserver(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Observer>(o);
  }
};


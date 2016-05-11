// automatically generated, do not modify

namespace GoodAI.Arnold.Network
{

using System;
using FlatBuffers;

public enum StateType : sbyte
{
 Empty = 0,
 Running = 1,
 Paused = 2,
 ShuttingDown = 3,
};

public enum Direction : sbyte
{
 Forward = 0,
 Backward = 1,
};

public enum Response : byte
{
 NONE = 0,
 ErrorResponse = 1,
 StateResponse = 2,
 ModelResponse = 3,
};

public sealed class ErrorResponse : Table {
  public static ErrorResponse GetRootAsErrorResponse(ByteBuffer _bb) { return GetRootAsErrorResponse(_bb, new ErrorResponse()); }
  public static ErrorResponse GetRootAsErrorResponse(ByteBuffer _bb, ErrorResponse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ErrorResponse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public string Message { get { int o = __offset(4); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetMessageBytes() { return __vector_as_arraysegment(4); }

  public static Offset<ErrorResponse> CreateErrorResponse(FlatBufferBuilder builder,
      StringOffset messageOffset = default(StringOffset)) {
    builder.StartObject(1);
    ErrorResponse.AddMessage(builder, messageOffset);
    return ErrorResponse.EndErrorResponse(builder);
  }

  public static void StartErrorResponse(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddMessage(FlatBufferBuilder builder, StringOffset messageOffset) { builder.AddOffset(0, messageOffset.Value, 0); }
  public static Offset<ErrorResponse> EndErrorResponse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ErrorResponse>(o);
  }
};

public sealed class StateResponse : Table {
  public static StateResponse GetRootAsStateResponse(ByteBuffer _bb) { return GetRootAsStateResponse(_bb, new StateResponse()); }
  public static StateResponse GetRootAsStateResponse(ByteBuffer _bb, StateResponse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public StateResponse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public StateType State { get { int o = __offset(4); return o != 0 ? (StateType)bb.GetSbyte(o + bb_pos) : StateType.Empty; } }

  public static Offset<StateResponse> CreateStateResponse(FlatBufferBuilder builder,
      StateType state = StateType.Empty) {
    builder.StartObject(1);
    StateResponse.AddState(builder, state);
    return StateResponse.EndStateResponse(builder);
  }

  public static void StartStateResponse(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddState(FlatBufferBuilder builder, StateType state) { builder.AddSbyte(0, (sbyte)state, 0); }
  public static Offset<StateResponse> EndStateResponse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<StateResponse>(o);
  }
};

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

public sealed class Region : Table {
  public static Region GetRootAsRegion(ByteBuffer _bb) { return GetRootAsRegion(_bb, new Region()); }
  public static Region GetRootAsRegion(ByteBuffer _bb, Region obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Region __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint Index { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Name { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(6); }
  public string Type { get { int o = __offset(8); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetTypeBytes() { return __vector_as_arraysegment(8); }
  public Position LowerBound { get { return GetLowerBound(new Position()); } }
  public Position GetLowerBound(Position obj) { int o = __offset(10); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public Position UpperBound { get { return GetUpperBound(new Position()); } }
  public Position GetUpperBound(Position obj) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Region> CreateRegion(FlatBufferBuilder builder,
      uint index = 0,
      StringOffset nameOffset = default(StringOffset),
      StringOffset typeOffset = default(StringOffset),
      Offset<Position> lowerBoundOffset = default(Offset<Position>),
      Offset<Position> upperBoundOffset = default(Offset<Position>)) {
    builder.StartObject(5);
    Region.AddUpperBound(builder, upperBoundOffset);
    Region.AddLowerBound(builder, lowerBoundOffset);
    Region.AddType(builder, typeOffset);
    Region.AddName(builder, nameOffset);
    Region.AddIndex(builder, index);
    return Region.EndRegion(builder);
  }

  public static void StartRegion(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddIndex(FlatBufferBuilder builder, uint index) { builder.AddUint(0, index, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddType(FlatBufferBuilder builder, StringOffset typeOffset) { builder.AddOffset(2, typeOffset.Value, 0); }
  public static void AddLowerBound(FlatBufferBuilder builder, Offset<Position> lowerBoundOffset) { builder.AddOffset(3, lowerBoundOffset.Value, 0); }
  public static void AddUpperBound(FlatBufferBuilder builder, Offset<Position> upperBoundOffset) { builder.AddOffset(4, upperBoundOffset.Value, 0); }
  public static Offset<Region> EndRegion(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Region>(o);
  }
};

public sealed class Connector : Table {
  public static Connector GetRootAsConnector(ByteBuffer _bb) { return GetRootAsConnector(_bb, new Connector()); }
  public static Connector GetRootAsConnector(ByteBuffer _bb, Connector obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Connector __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint RegionIndex { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Name { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(6); }
  public Direction Direction { get { int o = __offset(8); return o != 0 ? (Direction)bb.GetSbyte(o + bb_pos) : Direction.Forward; } }
  public uint Size { get { int o = __offset(10); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<Connector> CreateConnector(FlatBufferBuilder builder,
      uint regionIndex = 0,
      StringOffset nameOffset = default(StringOffset),
      Direction direction = Direction.Forward,
      uint size = 0) {
    builder.StartObject(4);
    Connector.AddSize(builder, size);
    Connector.AddName(builder, nameOffset);
    Connector.AddRegionIndex(builder, regionIndex);
    Connector.AddDirection(builder, direction);
    return Connector.EndConnector(builder);
  }

  public static void StartConnector(FlatBufferBuilder builder) { builder.StartObject(4); }
  public static void AddRegionIndex(FlatBufferBuilder builder, uint regionIndex) { builder.AddUint(0, regionIndex, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddDirection(FlatBufferBuilder builder, Direction direction) { builder.AddSbyte(2, (sbyte)direction, 0); }
  public static void AddSize(FlatBufferBuilder builder, uint size) { builder.AddUint(3, size, 0); }
  public static Offset<Connector> EndConnector(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Connector>(o);
  }
};

public sealed class Connection : Table {
  public static Connection GetRootAsConnection(ByteBuffer _bb) { return GetRootAsConnection(_bb, new Connection()); }
  public static Connection GetRootAsConnection(ByteBuffer _bb, Connection obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Connection __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint FromRegion { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string FromConnector { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetFromConnectorBytes() { return __vector_as_arraysegment(6); }
  public uint ToRegion { get { int o = __offset(8); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string ToConnector { get { int o = __offset(10); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetToConnectorBytes() { return __vector_as_arraysegment(10); }
  public Direction Direction { get { int o = __offset(12); return o != 0 ? (Direction)bb.GetSbyte(o + bb_pos) : Direction.Forward; } }

  public static Offset<Connection> CreateConnection(FlatBufferBuilder builder,
      uint fromRegion = 0,
      StringOffset fromConnectorOffset = default(StringOffset),
      uint toRegion = 0,
      StringOffset toConnectorOffset = default(StringOffset),
      Direction direction = Direction.Forward) {
    builder.StartObject(5);
    Connection.AddToConnector(builder, toConnectorOffset);
    Connection.AddToRegion(builder, toRegion);
    Connection.AddFromConnector(builder, fromConnectorOffset);
    Connection.AddFromRegion(builder, fromRegion);
    Connection.AddDirection(builder, direction);
    return Connection.EndConnection(builder);
  }

  public static void StartConnection(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddFromRegion(FlatBufferBuilder builder, uint fromRegion) { builder.AddUint(0, fromRegion, 0); }
  public static void AddFromConnector(FlatBufferBuilder builder, StringOffset fromConnectorOffset) { builder.AddOffset(1, fromConnectorOffset.Value, 0); }
  public static void AddToRegion(FlatBufferBuilder builder, uint toRegion) { builder.AddUint(2, toRegion, 0); }
  public static void AddToConnector(FlatBufferBuilder builder, StringOffset toConnectorOffset) { builder.AddOffset(3, toConnectorOffset.Value, 0); }
  public static void AddDirection(FlatBufferBuilder builder, Direction direction) { builder.AddSbyte(4, (sbyte)direction, 0); }
  public static Offset<Connection> EndConnection(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Connection>(o);
  }
};

public sealed class Neuron : Table {
  public static Neuron GetRootAsNeuron(ByteBuffer _bb) { return GetRootAsNeuron(_bb, new Neuron()); }
  public static Neuron GetRootAsNeuron(ByteBuffer _bb, Neuron obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Neuron __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint Id { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Type { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetTypeBytes() { return __vector_as_arraysegment(6); }
  public Position Position { get { return GetPosition(new Position()); } }
  public Position GetPosition(Position obj) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Neuron> CreateNeuron(FlatBufferBuilder builder,
      uint id = 0,
      StringOffset typeOffset = default(StringOffset),
      Offset<Position> positionOffset = default(Offset<Position>)) {
    builder.StartObject(3);
    Neuron.AddPosition(builder, positionOffset);
    Neuron.AddType(builder, typeOffset);
    Neuron.AddId(builder, id);
    return Neuron.EndNeuron(builder);
  }

  public static void StartNeuron(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddId(FlatBufferBuilder builder, uint id) { builder.AddUint(0, id, 0); }
  public static void AddType(FlatBufferBuilder builder, StringOffset typeOffset) { builder.AddOffset(1, typeOffset.Value, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(2, positionOffset.Value, 0); }
  public static Offset<Neuron> EndNeuron(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Neuron>(o);
  }
};

public sealed class Synapse : Table {
  public static Synapse GetRootAsSynapse(ByteBuffer _bb) { return GetRootAsSynapse(_bb, new Synapse()); }
  public static Synapse GetRootAsSynapse(ByteBuffer _bb, Synapse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Synapse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint From { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public uint To { get { int o = __offset(6); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<Synapse> CreateSynapse(FlatBufferBuilder builder,
      uint from = 0,
      uint to = 0) {
    builder.StartObject(2);
    Synapse.AddTo(builder, to);
    Synapse.AddFrom(builder, from);
    return Synapse.EndSynapse(builder);
  }

  public static void StartSynapse(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddFrom(FlatBufferBuilder builder, uint from) { builder.AddUint(0, from, 0); }
  public static void AddTo(FlatBufferBuilder builder, uint to) { builder.AddUint(1, to, 0); }
  public static Offset<Synapse> EndSynapse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Synapse>(o);
  }
};

public sealed class ModelResponse : Table {
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb) { return GetRootAsModelResponse(_bb, new ModelResponse()); }
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb, ModelResponse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ModelResponse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public Region GetAddedRegions(int j) { return GetAddedRegions(new Region(), j); }
  public Region GetAddedRegions(Region obj, int j) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedRegionsLength { get { int o = __offset(4); return o != 0 ? __vector_len(o) : 0; } }
  public Region GetRepositionedRegions(int j) { return GetRepositionedRegions(new Region(), j); }
  public Region GetRepositionedRegions(Region obj, int j) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RepositionedRegionsLength { get { int o = __offset(6); return o != 0 ? __vector_len(o) : 0; } }
  public uint GetRemovedRegions(int j) { int o = __offset(8); return o != 0 ? bb.GetUint(__vector(o) + j * 4) : (uint)0; }
  public int RemovedRegionsLength { get { int o = __offset(8); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetRemovedRegionsBytes() { return __vector_as_arraysegment(8); }
  public Connector GetAddedConnectors(int j) { return GetAddedConnectors(new Connector(), j); }
  public Connector GetAddedConnectors(Connector obj, int j) { int o = __offset(10); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectorsLength { get { int o = __offset(10); return o != 0 ? __vector_len(o) : 0; } }
  public Connector GetRemovedConnectors(int j) { return GetRemovedConnectors(new Connector(), j); }
  public Connector GetRemovedConnectors(Connector obj, int j) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectorsLength { get { int o = __offset(12); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetAddedConnections(int j) { return GetAddedConnections(new Connection(), j); }
  public Connection GetAddedConnections(Connection obj, int j) { int o = __offset(14); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectionsLength { get { int o = __offset(14); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetRemovedConnections(int j) { return GetRemovedConnections(new Connection(), j); }
  public Connection GetRemovedConnections(Connection obj, int j) { int o = __offset(16); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectionsLength { get { int o = __offset(16); return o != 0 ? __vector_len(o) : 0; } }
  public Neuron GetAddedNeurons(int j) { return GetAddedNeurons(new Neuron(), j); }
  public Neuron GetAddedNeurons(Neuron obj, int j) { int o = __offset(18); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedNeuronsLength { get { int o = __offset(18); return o != 0 ? __vector_len(o) : 0; } }
  public Neuron GetRepositionedNeurons(int j) { return GetRepositionedNeurons(new Neuron(), j); }
  public Neuron GetRepositionedNeurons(Neuron obj, int j) { int o = __offset(20); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RepositionedNeuronsLength { get { int o = __offset(20); return o != 0 ? __vector_len(o) : 0; } }
  public uint GetRemovedNeurons(int j) { int o = __offset(22); return o != 0 ? bb.GetUint(__vector(o) + j * 4) : (uint)0; }
  public int RemovedNeuronsLength { get { int o = __offset(22); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetRemovedNeuronsBytes() { return __vector_as_arraysegment(22); }
  public Synapse GetAddedSynapses(int j) { return GetAddedSynapses(new Synapse(), j); }
  public Synapse GetAddedSynapses(Synapse obj, int j) { int o = __offset(24); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedSynapsesLength { get { int o = __offset(24); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetSpikedSynapses(int j) { return GetSpikedSynapses(new Synapse(), j); }
  public Synapse GetSpikedSynapses(Synapse obj, int j) { int o = __offset(26); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int SpikedSynapsesLength { get { int o = __offset(26); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetRemovedSynapses(int j) { return GetRemovedSynapses(new Synapse(), j); }
  public Synapse GetRemovedSynapses(Synapse obj, int j) { int o = __offset(28); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedSynapsesLength { get { int o = __offset(28); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<ModelResponse> CreateModelResponse(FlatBufferBuilder builder,
      VectorOffset addedRegionsOffset = default(VectorOffset),
      VectorOffset repositionedRegionsOffset = default(VectorOffset),
      VectorOffset removedRegionsOffset = default(VectorOffset),
      VectorOffset addedConnectorsOffset = default(VectorOffset),
      VectorOffset removedConnectorsOffset = default(VectorOffset),
      VectorOffset addedConnectionsOffset = default(VectorOffset),
      VectorOffset removedConnectionsOffset = default(VectorOffset),
      VectorOffset addedNeuronsOffset = default(VectorOffset),
      VectorOffset repositionedNeuronsOffset = default(VectorOffset),
      VectorOffset removedNeuronsOffset = default(VectorOffset),
      VectorOffset addedSynapsesOffset = default(VectorOffset),
      VectorOffset SpikedSynapsesOffset = default(VectorOffset),
      VectorOffset removedSynapsesOffset = default(VectorOffset)) {
    builder.StartObject(13);
    ModelResponse.AddRemovedSynapses(builder, removedSynapsesOffset);
    ModelResponse.AddSpikedSynapses(builder, SpikedSynapsesOffset);
    ModelResponse.AddAddedSynapses(builder, addedSynapsesOffset);
    ModelResponse.AddRemovedNeurons(builder, removedNeuronsOffset);
    ModelResponse.AddRepositionedNeurons(builder, repositionedNeuronsOffset);
    ModelResponse.AddAddedNeurons(builder, addedNeuronsOffset);
    ModelResponse.AddRemovedConnections(builder, removedConnectionsOffset);
    ModelResponse.AddAddedConnections(builder, addedConnectionsOffset);
    ModelResponse.AddRemovedConnectors(builder, removedConnectorsOffset);
    ModelResponse.AddAddedConnectors(builder, addedConnectorsOffset);
    ModelResponse.AddRemovedRegions(builder, removedRegionsOffset);
    ModelResponse.AddRepositionedRegions(builder, repositionedRegionsOffset);
    ModelResponse.AddAddedRegions(builder, addedRegionsOffset);
    return ModelResponse.EndModelResponse(builder);
  }

  public static void StartModelResponse(FlatBufferBuilder builder) { builder.StartObject(13); }
  public static void AddAddedRegions(FlatBufferBuilder builder, VectorOffset addedRegionsOffset) { builder.AddOffset(0, addedRegionsOffset.Value, 0); }
  public static VectorOffset CreateAddedRegionsVector(FlatBufferBuilder builder, Offset<Region>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRepositionedRegions(FlatBufferBuilder builder, VectorOffset repositionedRegionsOffset) { builder.AddOffset(1, repositionedRegionsOffset.Value, 0); }
  public static VectorOffset CreateRepositionedRegionsVector(FlatBufferBuilder builder, Offset<Region>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRepositionedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedRegions(FlatBufferBuilder builder, VectorOffset removedRegionsOffset) { builder.AddOffset(2, removedRegionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedRegionsVector(FlatBufferBuilder builder, uint[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddUint(data[i]); return builder.EndVector(); }
  public static void StartRemovedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnectors(FlatBufferBuilder builder, VectorOffset addedConnectorsOffset) { builder.AddOffset(3, addedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectorsVector(FlatBufferBuilder builder, Offset<Connector>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnectors(FlatBufferBuilder builder, VectorOffset removedConnectorsOffset) { builder.AddOffset(4, removedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectorsVector(FlatBufferBuilder builder, Offset<Connector>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnections(FlatBufferBuilder builder, VectorOffset addedConnectionsOffset) { builder.AddOffset(5, addedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnections(FlatBufferBuilder builder, VectorOffset removedConnectionsOffset) { builder.AddOffset(6, removedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedNeurons(FlatBufferBuilder builder, VectorOffset addedNeuronsOffset) { builder.AddOffset(7, addedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateAddedNeuronsVector(FlatBufferBuilder builder, Offset<Neuron>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRepositionedNeurons(FlatBufferBuilder builder, VectorOffset repositionedNeuronsOffset) { builder.AddOffset(8, repositionedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateRepositionedNeuronsVector(FlatBufferBuilder builder, Offset<Neuron>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRepositionedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedNeurons(FlatBufferBuilder builder, VectorOffset removedNeuronsOffset) { builder.AddOffset(9, removedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateRemovedNeuronsVector(FlatBufferBuilder builder, uint[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddUint(data[i]); return builder.EndVector(); }
  public static void StartRemovedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedSynapses(FlatBufferBuilder builder, VectorOffset addedSynapsesOffset) { builder.AddOffset(10, addedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateAddedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddSpikedSynapses(FlatBufferBuilder builder, VectorOffset SpikedSynapsesOffset) { builder.AddOffset(11, SpikedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateSpikedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartSpikedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedSynapses(FlatBufferBuilder builder, VectorOffset removedSynapsesOffset) { builder.AddOffset(12, removedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateRemovedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<ModelResponse> EndModelResponse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ModelResponse>(o);
  }
};

public sealed class ResponseMessage : Table {
  public static ResponseMessage GetRootAsResponseMessage(ByteBuffer _bb) { return GetRootAsResponseMessage(_bb, new ResponseMessage()); }
  public static ResponseMessage GetRootAsResponseMessage(ByteBuffer _bb, ResponseMessage obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ResponseMessage __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public Response ResponseType { get { int o = __offset(4); return o != 0 ? (Response)bb.Get(o + bb_pos) : Response.NONE; } }
  public TTable GetResponse<TTable>(TTable obj) where TTable : Table { int o = __offset(6); return o != 0 ? __union(obj, o) : null; }

  public static Offset<ResponseMessage> CreateResponseMessage(FlatBufferBuilder builder,
      Response response_type = Response.NONE,
      int responseOffset = 0) {
    builder.StartObject(2);
    ResponseMessage.AddResponse(builder, responseOffset);
    ResponseMessage.AddResponseType(builder, response_type);
    return ResponseMessage.EndResponseMessage(builder);
  }

  public static void StartResponseMessage(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddResponseType(FlatBufferBuilder builder, Response responseType) { builder.AddByte(0, (byte)responseType, 0); }
  public static void AddResponse(FlatBufferBuilder builder, int responseOffset) { builder.AddOffset(1, responseOffset, 0); }
  public static Offset<ResponseMessage> EndResponseMessage(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ResponseMessage>(o);
  }
  public static void FinishResponseMessageBuffer(FlatBufferBuilder builder, Offset<ResponseMessage> offset) { builder.Finish(offset.Value); }
};


}

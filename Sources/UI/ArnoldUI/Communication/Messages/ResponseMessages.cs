// automatically generated, do not modify

namespace GoodAI.Arnold.Communication
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
  public uint BrainStep { get { int o = __offset(6); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public uint BodyStep { get { int o = __offset(8); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public uint BrainStepsPerBodyStep { get { int o = __offset(10); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<StateResponse> CreateStateResponse(FlatBufferBuilder builder,
      StateType state = StateType.Empty,
      uint brainStep = 0,
      uint bodyStep = 0,
      uint brainStepsPerBodyStep = 0) {
    builder.StartObject(4);
    StateResponse.AddBrainStepsPerBodyStep(builder, brainStepsPerBodyStep);
    StateResponse.AddBodyStep(builder, bodyStep);
    StateResponse.AddBrainStep(builder, brainStep);
    StateResponse.AddState(builder, state);
    return StateResponse.EndStateResponse(builder);
  }

  public static void StartStateResponse(FlatBufferBuilder builder) { builder.StartObject(4); }
  public static void AddState(FlatBufferBuilder builder, StateType state) { builder.AddSbyte(0, (sbyte)state, 0); }
  public static void AddBrainStep(FlatBufferBuilder builder, uint brainStep) { builder.AddUint(1, brainStep, 0); }
  public static void AddBodyStep(FlatBufferBuilder builder, uint bodyStep) { builder.AddUint(2, bodyStep, 0); }
  public static void AddBrainStepsPerBodyStep(FlatBufferBuilder builder, uint brainStepsPerBodyStep) { builder.AddUint(3, brainStepsPerBodyStep, 0); }
  public static Offset<StateResponse> EndStateResponse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<StateResponse>(o);
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
  public GoodAI.Arnold.Communication.Position Position { get { return GetPosition(new GoodAI.Arnold.Communication.Position()); } }
  public GoodAI.Arnold.Communication.Position GetPosition(GoodAI.Arnold.Communication.Position obj) { int o = __offset(10); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public GoodAI.Arnold.Communication.Position Size { get { return GetSize(new GoodAI.Arnold.Communication.Position()); } }
  public GoodAI.Arnold.Communication.Position GetSize(GoodAI.Arnold.Communication.Position obj) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Region> CreateRegion(FlatBufferBuilder builder,
      uint index = 0,
      StringOffset nameOffset = default(StringOffset),
      StringOffset typeOffset = default(StringOffset),
      Offset<GoodAI.Arnold.Communication.Position> positionOffset = default(Offset<GoodAI.Arnold.Communication.Position>),
      Offset<GoodAI.Arnold.Communication.Position> sizeOffset = default(Offset<GoodAI.Arnold.Communication.Position>)) {
    builder.StartObject(5);
    Region.AddSize(builder, sizeOffset);
    Region.AddPosition(builder, positionOffset);
    Region.AddType(builder, typeOffset);
    Region.AddName(builder, nameOffset);
    Region.AddIndex(builder, index);
    return Region.EndRegion(builder);
  }

  public static void StartRegion(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddIndex(FlatBufferBuilder builder, uint index) { builder.AddUint(0, index, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddType(FlatBufferBuilder builder, StringOffset typeOffset) { builder.AddOffset(2, typeOffset.Value, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.Position> positionOffset) { builder.AddOffset(3, positionOffset.Value, 0); }
  public static void AddSize(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.Position> sizeOffset) { builder.AddOffset(4, sizeOffset.Value, 0); }
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

public sealed class ConnectorRemoval : Table {
  public static ConnectorRemoval GetRootAsConnectorRemoval(ByteBuffer _bb) { return GetRootAsConnectorRemoval(_bb, new ConnectorRemoval()); }
  public static ConnectorRemoval GetRootAsConnectorRemoval(ByteBuffer _bb, ConnectorRemoval obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ConnectorRemoval __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint RegionIndex { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Name { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(6); }
  public Direction Direction { get { int o = __offset(8); return o != 0 ? (Direction)bb.GetSbyte(o + bb_pos) : Direction.Forward; } }

  public static Offset<ConnectorRemoval> CreateConnectorRemoval(FlatBufferBuilder builder,
      uint regionIndex = 0,
      StringOffset nameOffset = default(StringOffset),
      Direction direction = Direction.Forward) {
    builder.StartObject(3);
    ConnectorRemoval.AddName(builder, nameOffset);
    ConnectorRemoval.AddRegionIndex(builder, regionIndex);
    ConnectorRemoval.AddDirection(builder, direction);
    return ConnectorRemoval.EndConnectorRemoval(builder);
  }

  public static void StartConnectorRemoval(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddRegionIndex(FlatBufferBuilder builder, uint regionIndex) { builder.AddUint(0, regionIndex, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddDirection(FlatBufferBuilder builder, Direction direction) { builder.AddSbyte(2, (sbyte)direction, 0); }
  public static Offset<ConnectorRemoval> EndConnectorRemoval(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ConnectorRemoval>(o);
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

  public static Offset<Connection> CreateConnection(FlatBufferBuilder builder,
      uint fromRegion = 0,
      StringOffset fromConnectorOffset = default(StringOffset),
      uint toRegion = 0,
      StringOffset toConnectorOffset = default(StringOffset)) {
    builder.StartObject(4);
    Connection.AddToConnector(builder, toConnectorOffset);
    Connection.AddToRegion(builder, toRegion);
    Connection.AddFromConnector(builder, fromConnectorOffset);
    Connection.AddFromRegion(builder, fromRegion);
    return Connection.EndConnection(builder);
  }

  public static void StartConnection(FlatBufferBuilder builder) { builder.StartObject(4); }
  public static void AddFromRegion(FlatBufferBuilder builder, uint fromRegion) { builder.AddUint(0, fromRegion, 0); }
  public static void AddFromConnector(FlatBufferBuilder builder, StringOffset fromConnectorOffset) { builder.AddOffset(1, fromConnectorOffset.Value, 0); }
  public static void AddToRegion(FlatBufferBuilder builder, uint toRegion) { builder.AddUint(2, toRegion, 0); }
  public static void AddToConnector(FlatBufferBuilder builder, StringOffset toConnectorOffset) { builder.AddOffset(3, toConnectorOffset.Value, 0); }
  public static Offset<Connection> EndConnection(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Connection>(o);
  }
};

public sealed class Neuron : Table {
  public static Neuron GetRootAsNeuron(ByteBuffer _bb) { return GetRootAsNeuron(_bb, new Neuron()); }
  public static Neuron GetRootAsNeuron(ByteBuffer _bb, Neuron obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Neuron __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public GoodAI.Arnold.Communication.NeuronId Id { get { return GetId(new GoodAI.Arnold.Communication.NeuronId()); } }
  public GoodAI.Arnold.Communication.NeuronId GetId(GoodAI.Arnold.Communication.NeuronId obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public string Type { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetTypeBytes() { return __vector_as_arraysegment(6); }
  public GoodAI.Arnold.Communication.Position Position { get { return GetPosition(new GoodAI.Arnold.Communication.Position()); } }
  public GoodAI.Arnold.Communication.Position GetPosition(GoodAI.Arnold.Communication.Position obj) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Neuron> CreateNeuron(FlatBufferBuilder builder,
      Offset<GoodAI.Arnold.Communication.NeuronId> idOffset = default(Offset<GoodAI.Arnold.Communication.NeuronId>),
      StringOffset typeOffset = default(StringOffset),
      Offset<GoodAI.Arnold.Communication.Position> positionOffset = default(Offset<GoodAI.Arnold.Communication.Position>)) {
    builder.StartObject(3);
    Neuron.AddPosition(builder, positionOffset);
    Neuron.AddType(builder, typeOffset);
    Neuron.AddId(builder, idOffset);
    return Neuron.EndNeuron(builder);
  }

  public static void StartNeuron(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddId(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.NeuronId> idOffset) { builder.AddOffset(0, idOffset.Value, 0); }
  public static void AddType(FlatBufferBuilder builder, StringOffset typeOffset) { builder.AddOffset(1, typeOffset.Value, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.Position> positionOffset) { builder.AddOffset(2, positionOffset.Value, 0); }
  public static Offset<Neuron> EndNeuron(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Neuron>(o);
  }
};

public sealed class Synapse : Table {
  public static Synapse GetRootAsSynapse(ByteBuffer _bb) { return GetRootAsSynapse(_bb, new Synapse()); }
  public static Synapse GetRootAsSynapse(ByteBuffer _bb, Synapse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Synapse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public GoodAI.Arnold.Communication.NeuronId From { get { return GetFrom(new GoodAI.Arnold.Communication.NeuronId()); } }
  public GoodAI.Arnold.Communication.NeuronId GetFrom(GoodAI.Arnold.Communication.NeuronId obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public GoodAI.Arnold.Communication.NeuronId To { get { return GetTo(new GoodAI.Arnold.Communication.NeuronId()); } }
  public GoodAI.Arnold.Communication.NeuronId GetTo(GoodAI.Arnold.Communication.NeuronId obj) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Synapse> CreateSynapse(FlatBufferBuilder builder,
      Offset<GoodAI.Arnold.Communication.NeuronId> fromOffset = default(Offset<GoodAI.Arnold.Communication.NeuronId>),
      Offset<GoodAI.Arnold.Communication.NeuronId> toOffset = default(Offset<GoodAI.Arnold.Communication.NeuronId>)) {
    builder.StartObject(2);
    Synapse.AddTo(builder, toOffset);
    Synapse.AddFrom(builder, fromOffset);
    return Synapse.EndSynapse(builder);
  }

  public static void StartSynapse(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddFrom(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.NeuronId> fromOffset) { builder.AddOffset(0, fromOffset.Value, 0); }
  public static void AddTo(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.NeuronId> toOffset) { builder.AddOffset(1, toOffset.Value, 0); }
  public static Offset<Synapse> EndSynapse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Synapse>(o);
  }
};

public sealed class ObserverData : Table {
  public static ObserverData GetRootAsObserverData(ByteBuffer _bb) { return GetRootAsObserverData(_bb, new ObserverData()); }
  public static ObserverData GetRootAsObserverData(ByteBuffer _bb, ObserverData obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ObserverData __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public GoodAI.Arnold.Communication.Observer Observer { get { return GetObserver(new GoodAI.Arnold.Communication.Observer()); } }
  public GoodAI.Arnold.Communication.Observer GetObserver(GoodAI.Arnold.Communication.Observer obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public byte GetData(int j) { int o = __offset(6); return o != 0 ? bb.Get(__vector(o) + j * 1) : (byte)0; }
  public int DataLength { get { int o = __offset(6); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetDataBytes() { return __vector_as_arraysegment(6); }

  public static Offset<ObserverData> CreateObserverData(FlatBufferBuilder builder,
      Offset<GoodAI.Arnold.Communication.Observer> observerOffset = default(Offset<GoodAI.Arnold.Communication.Observer>),
      VectorOffset dataOffset = default(VectorOffset)) {
    builder.StartObject(2);
    ObserverData.AddData(builder, dataOffset);
    ObserverData.AddObserver(builder, observerOffset);
    return ObserverData.EndObserverData(builder);
  }

  public static void StartObserverData(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddObserver(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.Observer> observerOffset) { builder.AddOffset(0, observerOffset.Value, 0); }
  public static void AddData(FlatBufferBuilder builder, VectorOffset dataOffset) { builder.AddOffset(1, dataOffset.Value, 0); }
  public static VectorOffset CreateDataVector(FlatBufferBuilder builder, byte[] data) { builder.StartVector(1, data.Length, 1); for (int i = data.Length - 1; i >= 0; i--) builder.AddByte(data[i]); return builder.EndVector(); }
  public static void StartDataVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(1, numElems, 1); }
  public static Offset<ObserverData> EndObserverData(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ObserverData>(o);
  }
};

public sealed class ModelResponse : Table {
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb) { return GetRootAsModelResponse(_bb, new ModelResponse()); }
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb, ModelResponse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ModelResponse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public bool IsFull { get { int o = __offset(4); return o != 0 ? 0!=bb.Get(o + bb_pos) : (bool)false; } }
  public Region GetAddedRegions(int j) { return GetAddedRegions(new Region(), j); }
  public Region GetAddedRegions(Region obj, int j) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedRegionsLength { get { int o = __offset(6); return o != 0 ? __vector_len(o) : 0; } }
  public Region GetRepositionedRegions(int j) { return GetRepositionedRegions(new Region(), j); }
  public Region GetRepositionedRegions(Region obj, int j) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RepositionedRegionsLength { get { int o = __offset(8); return o != 0 ? __vector_len(o) : 0; } }
  public uint GetRemovedRegions(int j) { int o = __offset(10); return o != 0 ? bb.GetUint(__vector(o) + j * 4) : (uint)0; }
  public int RemovedRegionsLength { get { int o = __offset(10); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetRemovedRegionsBytes() { return __vector_as_arraysegment(10); }
  public Connector GetAddedConnectors(int j) { return GetAddedConnectors(new Connector(), j); }
  public Connector GetAddedConnectors(Connector obj, int j) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectorsLength { get { int o = __offset(12); return o != 0 ? __vector_len(o) : 0; } }
  public ConnectorRemoval GetRemovedConnectors(int j) { return GetRemovedConnectors(new ConnectorRemoval(), j); }
  public ConnectorRemoval GetRemovedConnectors(ConnectorRemoval obj, int j) { int o = __offset(14); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectorsLength { get { int o = __offset(14); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetAddedConnections(int j) { return GetAddedConnections(new Connection(), j); }
  public Connection GetAddedConnections(Connection obj, int j) { int o = __offset(16); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectionsLength { get { int o = __offset(16); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetRemovedConnections(int j) { return GetRemovedConnections(new Connection(), j); }
  public Connection GetRemovedConnections(Connection obj, int j) { int o = __offset(18); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectionsLength { get { int o = __offset(18); return o != 0 ? __vector_len(o) : 0; } }
  public Neuron GetAddedNeurons(int j) { return GetAddedNeurons(new Neuron(), j); }
  public Neuron GetAddedNeurons(Neuron obj, int j) { int o = __offset(20); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedNeuronsLength { get { int o = __offset(20); return o != 0 ? __vector_len(o) : 0; } }
  public Neuron GetRepositionedNeurons(int j) { return GetRepositionedNeurons(new Neuron(), j); }
  public Neuron GetRepositionedNeurons(Neuron obj, int j) { int o = __offset(22); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RepositionedNeuronsLength { get { int o = __offset(22); return o != 0 ? __vector_len(o) : 0; } }
  public GoodAI.Arnold.Communication.NeuronId GetRemovedNeurons(int j) { return GetRemovedNeurons(new GoodAI.Arnold.Communication.NeuronId(), j); }
  public GoodAI.Arnold.Communication.NeuronId GetRemovedNeurons(GoodAI.Arnold.Communication.NeuronId obj, int j) { int o = __offset(24); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedNeuronsLength { get { int o = __offset(24); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetAddedSynapses(int j) { return GetAddedSynapses(new Synapse(), j); }
  public Synapse GetAddedSynapses(Synapse obj, int j) { int o = __offset(26); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedSynapsesLength { get { int o = __offset(26); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetSpikedSynapses(int j) { return GetSpikedSynapses(new Synapse(), j); }
  public Synapse GetSpikedSynapses(Synapse obj, int j) { int o = __offset(28); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int SpikedSynapsesLength { get { int o = __offset(28); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetRemovedSynapses(int j) { return GetRemovedSynapses(new Synapse(), j); }
  public Synapse GetRemovedSynapses(Synapse obj, int j) { int o = __offset(30); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedSynapsesLength { get { int o = __offset(30); return o != 0 ? __vector_len(o) : 0; } }
  public ObserverData GetObservers(int j) { return GetObservers(new ObserverData(), j); }
  public ObserverData GetObservers(ObserverData obj, int j) { int o = __offset(32); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int ObserversLength { get { int o = __offset(32); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<ModelResponse> CreateModelResponse(FlatBufferBuilder builder,
      bool isFull = false,
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
      VectorOffset spikedSynapsesOffset = default(VectorOffset),
      VectorOffset removedSynapsesOffset = default(VectorOffset),
      VectorOffset observersOffset = default(VectorOffset)) {
    builder.StartObject(15);
    ModelResponse.AddObservers(builder, observersOffset);
    ModelResponse.AddRemovedSynapses(builder, removedSynapsesOffset);
    ModelResponse.AddSpikedSynapses(builder, spikedSynapsesOffset);
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
    ModelResponse.AddIsFull(builder, isFull);
    return ModelResponse.EndModelResponse(builder);
  }

  public static void StartModelResponse(FlatBufferBuilder builder) { builder.StartObject(15); }
  public static void AddIsFull(FlatBufferBuilder builder, bool isFull) { builder.AddBool(0, isFull, false); }
  public static void AddAddedRegions(FlatBufferBuilder builder, VectorOffset addedRegionsOffset) { builder.AddOffset(1, addedRegionsOffset.Value, 0); }
  public static VectorOffset CreateAddedRegionsVector(FlatBufferBuilder builder, Offset<Region>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRepositionedRegions(FlatBufferBuilder builder, VectorOffset repositionedRegionsOffset) { builder.AddOffset(2, repositionedRegionsOffset.Value, 0); }
  public static VectorOffset CreateRepositionedRegionsVector(FlatBufferBuilder builder, Offset<Region>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRepositionedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedRegions(FlatBufferBuilder builder, VectorOffset removedRegionsOffset) { builder.AddOffset(3, removedRegionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedRegionsVector(FlatBufferBuilder builder, uint[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddUint(data[i]); return builder.EndVector(); }
  public static void StartRemovedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnectors(FlatBufferBuilder builder, VectorOffset addedConnectorsOffset) { builder.AddOffset(4, addedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectorsVector(FlatBufferBuilder builder, Offset<Connector>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnectors(FlatBufferBuilder builder, VectorOffset removedConnectorsOffset) { builder.AddOffset(5, removedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectorsVector(FlatBufferBuilder builder, Offset<ConnectorRemoval>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnections(FlatBufferBuilder builder, VectorOffset addedConnectionsOffset) { builder.AddOffset(6, addedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnections(FlatBufferBuilder builder, VectorOffset removedConnectionsOffset) { builder.AddOffset(7, removedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedNeurons(FlatBufferBuilder builder, VectorOffset addedNeuronsOffset) { builder.AddOffset(8, addedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateAddedNeuronsVector(FlatBufferBuilder builder, Offset<Neuron>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRepositionedNeurons(FlatBufferBuilder builder, VectorOffset repositionedNeuronsOffset) { builder.AddOffset(9, repositionedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateRepositionedNeuronsVector(FlatBufferBuilder builder, Offset<Neuron>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRepositionedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedNeurons(FlatBufferBuilder builder, VectorOffset removedNeuronsOffset) { builder.AddOffset(10, removedNeuronsOffset.Value, 0); }
  public static VectorOffset CreateRemovedNeuronsVector(FlatBufferBuilder builder, Offset<GoodAI.Arnold.Communication.NeuronId>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedSynapses(FlatBufferBuilder builder, VectorOffset addedSynapsesOffset) { builder.AddOffset(11, addedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateAddedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddSpikedSynapses(FlatBufferBuilder builder, VectorOffset spikedSynapsesOffset) { builder.AddOffset(12, spikedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateSpikedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartSpikedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedSynapses(FlatBufferBuilder builder, VectorOffset removedSynapsesOffset) { builder.AddOffset(13, removedSynapsesOffset.Value, 0); }
  public static VectorOffset CreateRemovedSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddObservers(FlatBufferBuilder builder, VectorOffset observersOffset) { builder.AddOffset(14, observersOffset.Value, 0); }
  public static VectorOffset CreateObserversVector(FlatBufferBuilder builder, Offset<ObserverData>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartObserversVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
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

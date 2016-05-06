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

public enum ConnectorType : sbyte
{
 Input = 0,
 Output = 1,
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

  public uint Id { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Name { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(6); }
  public Position Position { get { return GetPosition(new Position()); } }
  public Position GetPosition(Position obj) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<Region> CreateRegion(FlatBufferBuilder builder,
      uint id = 0,
      StringOffset nameOffset = default(StringOffset),
      Offset<Position> positionOffset = default(Offset<Position>)) {
    builder.StartObject(3);
    Region.AddPosition(builder, positionOffset);
    Region.AddName(builder, nameOffset);
    Region.AddId(builder, id);
    return Region.EndRegion(builder);
  }

  public static void StartRegion(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddId(FlatBufferBuilder builder, uint id) { builder.AddUint(0, id, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(2, positionOffset.Value, 0); }
  public static Offset<Region> EndRegion(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Region>(o);
  }
};

public sealed class ConnectorId : Table {
  public static ConnectorId GetRootAsConnectorId(ByteBuffer _bb) { return GetRootAsConnectorId(_bb, new ConnectorId()); }
  public static ConnectorId GetRootAsConnectorId(ByteBuffer _bb, ConnectorId obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ConnectorId __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint RegionId { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public string Name { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetNameBytes() { return __vector_as_arraysegment(6); }

  public static Offset<ConnectorId> CreateConnectorId(FlatBufferBuilder builder,
      uint regionId = 0,
      StringOffset nameOffset = default(StringOffset)) {
    builder.StartObject(2);
    ConnectorId.AddName(builder, nameOffset);
    ConnectorId.AddRegionId(builder, regionId);
    return ConnectorId.EndConnectorId(builder);
  }

  public static void StartConnectorId(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddRegionId(FlatBufferBuilder builder, uint regionId) { builder.AddUint(0, regionId, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static Offset<ConnectorId> EndConnectorId(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ConnectorId>(o);
  }
};

public sealed class Connector : Table {
  public static Connector GetRootAsConnector(ByteBuffer _bb) { return GetRootAsConnector(_bb, new Connector()); }
  public static Connector GetRootAsConnector(ByteBuffer _bb, Connector obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Connector __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public ConnectorId Id { get { return GetId(new ConnectorId()); } }
  public ConnectorId GetId(ConnectorId obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public ConnectorType Type { get { int o = __offset(6); return o != 0 ? (ConnectorType)bb.GetSbyte(o + bb_pos) : ConnectorType.Input; } }
  public uint Size { get { int o = __offset(8); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<Connector> CreateConnector(FlatBufferBuilder builder,
      Offset<ConnectorId> idOffset = default(Offset<ConnectorId>),
      ConnectorType type = ConnectorType.Input,
      uint size = 0) {
    builder.StartObject(3);
    Connector.AddSize(builder, size);
    Connector.AddId(builder, idOffset);
    Connector.AddType(builder, type);
    return Connector.EndConnector(builder);
  }

  public static void StartConnector(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddId(FlatBufferBuilder builder, Offset<ConnectorId> idOffset) { builder.AddOffset(0, idOffset.Value, 0); }
  public static void AddType(FlatBufferBuilder builder, ConnectorType type) { builder.AddSbyte(1, (sbyte)type, 0); }
  public static void AddSize(FlatBufferBuilder builder, uint size) { builder.AddUint(2, size, 0); }
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

  public uint Id { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public Position Position { get { return GetPosition(new Position()); } }
  public Position GetPosition(Position obj) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public bool Spiked { get { int o = __offset(8); return o != 0 ? 0!=bb.Get(o + bb_pos) : (bool)false; } }

  public static Offset<Neuron> CreateNeuron(FlatBufferBuilder builder,
      uint id = 0,
      Offset<Position> positionOffset = default(Offset<Position>),
      bool spiked = false) {
    builder.StartObject(3);
    Neuron.AddPosition(builder, positionOffset);
    Neuron.AddId(builder, id);
    Neuron.AddSpiked(builder, spiked);
    return Neuron.EndNeuron(builder);
  }

  public static void StartNeuron(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddId(FlatBufferBuilder builder, uint id) { builder.AddUint(0, id, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<Position> positionOffset) { builder.AddOffset(1, positionOffset.Value, 0); }
  public static void AddSpiked(FlatBufferBuilder builder, bool spiked) { builder.AddBool(2, spiked, false); }
  public static Offset<Neuron> EndNeuron(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Neuron>(o);
  }
};

public sealed class Synapse : Table {
  public static Synapse GetRootAsSynapse(ByteBuffer _bb) { return GetRootAsSynapse(_bb, new Synapse()); }
  public static Synapse GetRootAsSynapse(ByteBuffer _bb, Synapse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Synapse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint FromNeuron { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public uint ToNeuron { get { int o = __offset(6); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public bool Spiked { get { int o = __offset(8); return o != 0 ? 0!=bb.Get(o + bb_pos) : (bool)false; } }

  public static Offset<Synapse> CreateSynapse(FlatBufferBuilder builder,
      uint fromNeuron = 0,
      uint toNeuron = 0,
      bool spiked = false) {
    builder.StartObject(3);
    Synapse.AddToNeuron(builder, toNeuron);
    Synapse.AddFromNeuron(builder, fromNeuron);
    Synapse.AddSpiked(builder, spiked);
    return Synapse.EndSynapse(builder);
  }

  public static void StartSynapse(FlatBufferBuilder builder) { builder.StartObject(3); }
  public static void AddFromNeuron(FlatBufferBuilder builder, uint fromNeuron) { builder.AddUint(0, fromNeuron, 0); }
  public static void AddToNeuron(FlatBufferBuilder builder, uint toNeuron) { builder.AddUint(1, toNeuron, 0); }
  public static void AddSpiked(FlatBufferBuilder builder, bool spiked) { builder.AddBool(2, spiked, false); }
  public static Offset<Synapse> EndSynapse(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Synapse>(o);
  }
};

public sealed class RegionView : Table {
  public static RegionView GetRootAsRegionView(ByteBuffer _bb) { return GetRootAsRegionView(_bb, new RegionView()); }
  public static RegionView GetRootAsRegionView(ByteBuffer _bb, RegionView obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public RegionView __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public uint RegionId { get { int o = __offset(4); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }
  public Position LowerBound { get { return GetLowerBound(new Position()); } }
  public Position GetLowerBound(Position obj) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public Position UpperBound { get { return GetUpperBound(new Position()); } }
  public Position GetUpperBound(Position obj) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }
  public Neuron GetNeurons(int j) { return GetNeurons(new Neuron(), j); }
  public Neuron GetNeurons(Neuron obj, int j) { int o = __offset(10); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int NeuronsLength { get { int o = __offset(10); return o != 0 ? __vector_len(o) : 0; } }
  public Synapse GetSynapses(int j) { return GetSynapses(new Synapse(), j); }
  public Synapse GetSynapses(Synapse obj, int j) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int SynapsesLength { get { int o = __offset(12); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<RegionView> CreateRegionView(FlatBufferBuilder builder,
      uint regionId = 0,
      Offset<Position> lowerBoundOffset = default(Offset<Position>),
      Offset<Position> upperBoundOffset = default(Offset<Position>),
      VectorOffset neuronsOffset = default(VectorOffset),
      VectorOffset synapsesOffset = default(VectorOffset)) {
    builder.StartObject(5);
    RegionView.AddSynapses(builder, synapsesOffset);
    RegionView.AddNeurons(builder, neuronsOffset);
    RegionView.AddUpperBound(builder, upperBoundOffset);
    RegionView.AddLowerBound(builder, lowerBoundOffset);
    RegionView.AddRegionId(builder, regionId);
    return RegionView.EndRegionView(builder);
  }

  public static void StartRegionView(FlatBufferBuilder builder) { builder.StartObject(5); }
  public static void AddRegionId(FlatBufferBuilder builder, uint regionId) { builder.AddUint(0, regionId, 0); }
  public static void AddLowerBound(FlatBufferBuilder builder, Offset<Position> lowerBoundOffset) { builder.AddOffset(1, lowerBoundOffset.Value, 0); }
  public static void AddUpperBound(FlatBufferBuilder builder, Offset<Position> upperBoundOffset) { builder.AddOffset(2, upperBoundOffset.Value, 0); }
  public static void AddNeurons(FlatBufferBuilder builder, VectorOffset neuronsOffset) { builder.AddOffset(3, neuronsOffset.Value, 0); }
  public static VectorOffset CreateNeuronsVector(FlatBufferBuilder builder, Offset<Neuron>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartNeuronsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddSynapses(FlatBufferBuilder builder, VectorOffset synapsesOffset) { builder.AddOffset(4, synapsesOffset.Value, 0); }
  public static VectorOffset CreateSynapsesVector(FlatBufferBuilder builder, Offset<Synapse>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartSynapsesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<RegionView> EndRegionView(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<RegionView>(o);
  }
};

public sealed class ModelResponse : Table {
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb) { return GetRootAsModelResponse(_bb, new ModelResponse()); }
  public static ModelResponse GetRootAsModelResponse(ByteBuffer _bb, ModelResponse obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public ModelResponse __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public Region GetAddedRegions(int j) { return GetAddedRegions(new Region(), j); }
  public Region GetAddedRegions(Region obj, int j) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedRegionsLength { get { int o = __offset(4); return o != 0 ? __vector_len(o) : 0; } }
  public uint GetRemovedRegions(int j) { int o = __offset(6); return o != 0 ? bb.GetUint(__vector(o) + j * 4) : (uint)0; }
  public int RemovedRegionsLength { get { int o = __offset(6); return o != 0 ? __vector_len(o) : 0; } }
  public ArraySegment<byte>? GetRemovedRegionsBytes() { return __vector_as_arraysegment(6); }
  public Connector GetAddedConnectors(int j) { return GetAddedConnectors(new Connector(), j); }
  public Connector GetAddedConnectors(Connector obj, int j) { int o = __offset(8); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectorsLength { get { int o = __offset(8); return o != 0 ? __vector_len(o) : 0; } }
  public ConnectorId GetRemovedConnectors(int j) { return GetRemovedConnectors(new ConnectorId(), j); }
  public ConnectorId GetRemovedConnectors(ConnectorId obj, int j) { int o = __offset(10); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectorsLength { get { int o = __offset(10); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetAddedConnections(int j) { return GetAddedConnections(new Connection(), j); }
  public Connection GetAddedConnections(Connection obj, int j) { int o = __offset(12); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int AddedConnectionsLength { get { int o = __offset(12); return o != 0 ? __vector_len(o) : 0; } }
  public Connection GetRemovedConnections(int j) { return GetRemovedConnections(new Connection(), j); }
  public Connection GetRemovedConnections(Connection obj, int j) { int o = __offset(14); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RemovedConnectionsLength { get { int o = __offset(14); return o != 0 ? __vector_len(o) : 0; } }
  public RegionView GetRegionViews(int j) { return GetRegionViews(new RegionView(), j); }
  public RegionView GetRegionViews(RegionView obj, int j) { int o = __offset(16); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int RegionViewsLength { get { int o = __offset(16); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<ModelResponse> CreateModelResponse(FlatBufferBuilder builder,
      VectorOffset addedRegionsOffset = default(VectorOffset),
      VectorOffset removedRegionsOffset = default(VectorOffset),
      VectorOffset addedConnectorsOffset = default(VectorOffset),
      VectorOffset removedConnectorsOffset = default(VectorOffset),
      VectorOffset addedConnectionsOffset = default(VectorOffset),
      VectorOffset removedConnectionsOffset = default(VectorOffset),
      VectorOffset regionViewsOffset = default(VectorOffset)) {
    builder.StartObject(7);
    ModelResponse.AddRegionViews(builder, regionViewsOffset);
    ModelResponse.AddRemovedConnections(builder, removedConnectionsOffset);
    ModelResponse.AddAddedConnections(builder, addedConnectionsOffset);
    ModelResponse.AddRemovedConnectors(builder, removedConnectorsOffset);
    ModelResponse.AddAddedConnectors(builder, addedConnectorsOffset);
    ModelResponse.AddRemovedRegions(builder, removedRegionsOffset);
    ModelResponse.AddAddedRegions(builder, addedRegionsOffset);
    return ModelResponse.EndModelResponse(builder);
  }

  public static void StartModelResponse(FlatBufferBuilder builder) { builder.StartObject(7); }
  public static void AddAddedRegions(FlatBufferBuilder builder, VectorOffset addedRegionsOffset) { builder.AddOffset(0, addedRegionsOffset.Value, 0); }
  public static VectorOffset CreateAddedRegionsVector(FlatBufferBuilder builder, Offset<Region>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedRegions(FlatBufferBuilder builder, VectorOffset removedRegionsOffset) { builder.AddOffset(1, removedRegionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedRegionsVector(FlatBufferBuilder builder, uint[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddUint(data[i]); return builder.EndVector(); }
  public static void StartRemovedRegionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnectors(FlatBufferBuilder builder, VectorOffset addedConnectorsOffset) { builder.AddOffset(2, addedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectorsVector(FlatBufferBuilder builder, Offset<Connector>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnectors(FlatBufferBuilder builder, VectorOffset removedConnectorsOffset) { builder.AddOffset(3, removedConnectorsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectorsVector(FlatBufferBuilder builder, Offset<ConnectorId>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectorsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddAddedConnections(FlatBufferBuilder builder, VectorOffset addedConnectionsOffset) { builder.AddOffset(4, addedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateAddedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartAddedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRemovedConnections(FlatBufferBuilder builder, VectorOffset removedConnectionsOffset) { builder.AddOffset(5, removedConnectionsOffset.Value, 0); }
  public static VectorOffset CreateRemovedConnectionsVector(FlatBufferBuilder builder, Offset<Connection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRemovedConnectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddRegionViews(FlatBufferBuilder builder, VectorOffset regionViewsOffset) { builder.AddOffset(6, regionViewsOffset.Value, 0); }
  public static VectorOffset CreateRegionViewsVector(FlatBufferBuilder builder, Offset<RegionView>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartRegionViewsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
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

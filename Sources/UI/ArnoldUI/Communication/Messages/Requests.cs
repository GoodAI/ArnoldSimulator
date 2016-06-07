// automatically generated, do not modify

namespace GoodAI.Arnold.Communication
{

using System;
using FlatBuffers;

public enum CommandType : sbyte
{
 Load = 0,
 Run = 1,
 Pause = 2,
 Clear = 3,
 Shutdown = 4,
};

public enum Request : byte
{
 NONE = 0,
 CommandRequest = 1,
 GetStateRequest = 2,
 GetModelRequest = 3,
};

public sealed class CommandRequest : Table {
  public static CommandRequest GetRootAsCommandRequest(ByteBuffer _bb) { return GetRootAsCommandRequest(_bb, new CommandRequest()); }
  public static CommandRequest GetRootAsCommandRequest(ByteBuffer _bb, CommandRequest obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public CommandRequest __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public CommandType Command { get { int o = __offset(4); return o != 0 ? (CommandType)bb.GetSbyte(o + bb_pos) : CommandType.Load; } }
  public uint StepsToRun { get { int o = __offset(6); return o != 0 ? bb.GetUint(o + bb_pos) : (uint)0; } }

  public static Offset<CommandRequest> CreateCommandRequest(FlatBufferBuilder builder,
      CommandType command = CommandType.Load,
      uint stepsToRun = 0) {
    builder.StartObject(2);
    CommandRequest.AddStepsToRun(builder, stepsToRun);
    CommandRequest.AddCommand(builder, command);
    return CommandRequest.EndCommandRequest(builder);
  }

  public static void StartCommandRequest(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddCommand(FlatBufferBuilder builder, CommandType command) { builder.AddSbyte(0, (sbyte)command, 0); }
  public static void AddStepsToRun(FlatBufferBuilder builder, uint stepsToRun) { builder.AddUint(1, stepsToRun, 0); }
  public static Offset<CommandRequest> EndCommandRequest(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<CommandRequest>(o);
  }
};

public sealed class GetStateRequest : Table {
  public static GetStateRequest GetRootAsGetStateRequest(ByteBuffer _bb) { return GetRootAsGetStateRequest(_bb, new GetStateRequest()); }
  public static GetStateRequest GetRootAsGetStateRequest(ByteBuffer _bb, GetStateRequest obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public GetStateRequest __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }


  public static void StartGetStateRequest(FlatBufferBuilder builder) { builder.StartObject(0); }
  public static Offset<GetStateRequest> EndGetStateRequest(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<GetStateRequest>(o);
  }
};

public sealed class Box3D : Table {
  public static Box3D GetRootAsBox3D(ByteBuffer _bb) { return GetRootAsBox3D(_bb, new Box3D()); }
  public static Box3D GetRootAsBox3D(ByteBuffer _bb, Box3D obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Box3D __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public float X { get { int o = __offset(4); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Y { get { int o = __offset(6); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float Z { get { int o = __offset(8); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float SizeX { get { int o = __offset(10); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float SizeY { get { int o = __offset(12); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }
  public float SizeZ { get { int o = __offset(14); return o != 0 ? bb.GetFloat(o + bb_pos) : (float)0; } }

  public static Offset<Box3D> CreateBox3D(FlatBufferBuilder builder,
      float x = 0,
      float y = 0,
      float z = 0,
      float sizeX = 0,
      float sizeY = 0,
      float sizeZ = 0) {
    builder.StartObject(6);
    Box3D.AddSizeZ(builder, sizeZ);
    Box3D.AddSizeY(builder, sizeY);
    Box3D.AddSizeX(builder, sizeX);
    Box3D.AddZ(builder, z);
    Box3D.AddY(builder, y);
    Box3D.AddX(builder, x);
    return Box3D.EndBox3D(builder);
  }

  public static void StartBox3D(FlatBufferBuilder builder) { builder.StartObject(6); }
  public static void AddX(FlatBufferBuilder builder, float x) { builder.AddFloat(0, x, 0); }
  public static void AddY(FlatBufferBuilder builder, float y) { builder.AddFloat(1, y, 0); }
  public static void AddZ(FlatBufferBuilder builder, float z) { builder.AddFloat(2, z, 0); }
  public static void AddSizeX(FlatBufferBuilder builder, float sizeX) { builder.AddFloat(3, sizeX, 0); }
  public static void AddSizeY(FlatBufferBuilder builder, float sizeY) { builder.AddFloat(4, sizeY, 0); }
  public static void AddSizeZ(FlatBufferBuilder builder, float sizeZ) { builder.AddFloat(5, sizeZ, 0); }
  public static Offset<Box3D> EndBox3D(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Box3D>(o);
  }
};

public sealed class Filter : Table {
  public static Filter GetRootAsFilter(ByteBuffer _bb) { return GetRootAsFilter(_bb, new Filter()); }
  public static Filter GetRootAsFilter(ByteBuffer _bb, Filter obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Filter __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public Box3D GetBoxes(int j) { return GetBoxes(new Box3D(), j); }
  public Box3D GetBoxes(Box3D obj, int j) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(__vector(o) + j * 4), bb) : null; }
  public int BoxesLength { get { int o = __offset(4); return o != 0 ? __vector_len(o) : 0; } }

  public static Offset<Filter> CreateFilter(FlatBufferBuilder builder,
      VectorOffset boxesOffset = default(VectorOffset)) {
    builder.StartObject(1);
    Filter.AddBoxes(builder, boxesOffset);
    return Filter.EndFilter(builder);
  }

  public static void StartFilter(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddBoxes(FlatBufferBuilder builder, VectorOffset boxesOffset) { builder.AddOffset(0, boxesOffset.Value, 0); }
  public static VectorOffset CreateBoxesVector(FlatBufferBuilder builder, Offset<Box3D>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartBoxesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Filter> EndFilter(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Filter>(o);
  }
};

public sealed class GetModelRequest : Table {
  public static GetModelRequest GetRootAsGetModelRequest(ByteBuffer _bb) { return GetRootAsGetModelRequest(_bb, new GetModelRequest()); }
  public static GetModelRequest GetRootAsGetModelRequest(ByteBuffer _bb, GetModelRequest obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public GetModelRequest __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public bool Full { get { int o = __offset(4); return o != 0 ? 0!=bb.Get(o + bb_pos) : (bool)false; } }
  public Filter Filter { get { return GetFilter(new Filter()); } }
  public Filter GetFilter(Filter obj) { int o = __offset(6); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<GetModelRequest> CreateGetModelRequest(FlatBufferBuilder builder,
      bool full = false,
      Offset<Filter> filterOffset = default(Offset<Filter>)) {
    builder.StartObject(2);
    GetModelRequest.AddFilter(builder, filterOffset);
    GetModelRequest.AddFull(builder, full);
    return GetModelRequest.EndGetModelRequest(builder);
  }

  public static void StartGetModelRequest(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddFull(FlatBufferBuilder builder, bool full) { builder.AddBool(0, full, false); }
  public static void AddFilter(FlatBufferBuilder builder, Offset<Filter> filterOffset) { builder.AddOffset(1, filterOffset.Value, 0); }
  public static Offset<GetModelRequest> EndGetModelRequest(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<GetModelRequest>(o);
  }
};

public sealed class RequestMessage : Table {
  public static RequestMessage GetRootAsRequestMessage(ByteBuffer _bb) { return GetRootAsRequestMessage(_bb, new RequestMessage()); }
  public static RequestMessage GetRootAsRequestMessage(ByteBuffer _bb, RequestMessage obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public RequestMessage __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public Request RequestType { get { int o = __offset(4); return o != 0 ? (Request)bb.Get(o + bb_pos) : Request.NONE; } }
  public TTable GetRequest<TTable>(TTable obj) where TTable : Table { int o = __offset(6); return o != 0 ? __union(obj, o) : null; }

  public static Offset<RequestMessage> CreateRequestMessage(FlatBufferBuilder builder,
      Request request_type = Request.NONE,
      int requestOffset = 0) {
    builder.StartObject(2);
    RequestMessage.AddRequest(builder, requestOffset);
    RequestMessage.AddRequestType(builder, request_type);
    return RequestMessage.EndRequestMessage(builder);
  }

  public static void StartRequestMessage(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddRequestType(FlatBufferBuilder builder, Request requestType) { builder.AddByte(0, (byte)requestType, 0); }
  public static void AddRequest(FlatBufferBuilder builder, int requestOffset) { builder.AddOffset(1, requestOffset, 0); }
  public static Offset<RequestMessage> EndRequestMessage(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<RequestMessage>(o);
  }
  public static void FinishRequestMessageBuffer(FlatBufferBuilder builder, Offset<RequestMessage> offset) { builder.Finish(offset.Value); }
};


}

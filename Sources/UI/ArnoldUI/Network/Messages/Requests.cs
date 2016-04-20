// automatically generated, do not modify

namespace GoodAI.Arnold.Network
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

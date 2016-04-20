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
 Invalid = 4,
};

public enum Response : byte
{
 NONE = 0,
 ErrorResponse = 1,
 StateResponse = 2,
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

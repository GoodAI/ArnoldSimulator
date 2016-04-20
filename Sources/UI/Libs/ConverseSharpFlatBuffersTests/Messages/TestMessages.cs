// automatically generated, do not modify

namespace GoodAI.Net.ConverseSharpFlatBuffers
{

using System;
using FlatBuffers;

public sealed class Command : Table {
  public static Command GetRootAsCommand(ByteBuffer _bb) { return GetRootAsCommand(_bb, new Command()); }
  public static Command GetRootAsCommand(ByteBuffer _bb, Command obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public Command __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public int Code { get { int o = __offset(4); return o != 0 ? bb.GetInt(o + bb_pos) : (int)0; } }
  public string Method { get { int o = __offset(6); return o != 0 ? __string(o + bb_pos) : null; } }
  public ArraySegment<byte>? GetMethodBytes() { return __vector_as_arraysegment(6); }

  public static Offset<Command> CreateCommand(FlatBufferBuilder builder,
      int code = 0,
      StringOffset methodOffset = default(StringOffset)) {
    builder.StartObject(2);
    Command.AddMethod(builder, methodOffset);
    Command.AddCode(builder, code);
    return Command.EndCommand(builder);
  }

  public static void StartCommand(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddCode(FlatBufferBuilder builder, int code) { builder.AddInt(0, code, 0); }
  public static void AddMethod(FlatBufferBuilder builder, StringOffset methodOffset) { builder.AddOffset(1, methodOffset.Value, 0); }
  public static Offset<Command> EndCommand(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Command>(o);
  }
  public static void FinishCommandBuffer(FlatBufferBuilder builder, Offset<Command> offset) { builder.Finish(offset.Value); }
};


}

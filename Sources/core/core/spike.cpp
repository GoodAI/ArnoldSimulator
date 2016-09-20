#include "neuron.h"
#include "log.h"
#include "components.h"

#include "spike.h"
#include "data_utils.h"

Spike::Type Spike::ParseType(const std::string &type)
{
    SpikeEditorCache *editorCache = SpikeEditorCache::GetInstance();
    return editorCache->GetToken(type);
}

const char *Spike::SerializeType(Type type)
{
    SpikeEditorCache *editorCache = SpikeEditorCache::GetInstance();
    return editorCache->GetName(type).c_str();
}

Spike::Data::Data()
{
    type = DefaultType;
    bits8 = 0;
    bits16 = 0;
    sender = 0;
    bits64 = 0;
}

Spike::Data::~Data()
{
    Spike::Release(*this);
}

Spike::Data::Data(const Data &other)
{
    type = other.type;
    bits8 = other.bits8;
    bits16 = other.bits16;
    sender = other.sender;

    Editor *ed = Edit(*this);
    if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
        bits64 = reinterpret_cast<uintptr_t>(ed->AllocateExtra(*this));
        memcpy(reinterpret_cast<unsigned char *>(bits64),
            reinterpret_cast<unsigned char *>(other.bits64), ed->ExtraBytes(*this));
    } else {
        bits64 = other.bits64;
    }
}

Spike::Data::Data(Data &&other)
{
    type = other.type;
    bits8 = other.bits8;
    bits16 = other.bits16;
    sender = other.sender;

    Editor *ed = Edit(*this);
    if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
        bits64 = other.bits64;
        other.bits64 = 0;
    } else {
        bits64 = other.bits64;
    }
}

Spike::Data &Spike::Data::operator=(const Data &other)
{
    if (this != &other)
    {
        type = other.type;
        bits8 = other.bits8;
        bits16 = other.bits16;
        sender = other.sender;

        Editor *ed = Edit(*this);
        if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
            bits64 = reinterpret_cast<uintptr_t>(ed->AllocateExtra(*this));
            memcpy(reinterpret_cast<unsigned char *>(bits64),
                reinterpret_cast<unsigned char *>(other.bits64), ed->ExtraBytes(*this));
        } else {
            bits64 = other.bits64;
        }
    }
    return *this;
}

Spike::Data &Spike::Data::operator=(Data &&other)
{
    if (this != &other)
    {
        type = other.type;
        bits8 = other.bits8;
        bits16 = other.bits16;
        sender = other.sender;

        Editor *ed = Edit(*this);
        if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
            bits64 = other.bits64;
            other.bits64 = 0;
        } else {
            bits64 = other.bits64;
        }
    }
    return *this;
}

void Spike::Data::pup(PUP::er &p)
{
    Type temp;
    if (p.isUnpacking()) {
        p | temp;
        type = static_cast<Type>(temp);
    } else {
        temp = static_cast<Type>(type);
        p | temp;
    }

    p | bits8;
    p | bits16;
    p | sender;

    Editor *ed = Edit(*this);

    if (ed->ExtraBytes(*this) > 0) {
        if (p.isUnpacking()) {
            bits64 = reinterpret_cast<uintptr_t>(ed->AllocateExtra(*this));
        }
        if (bits64 != 0) {
            p(reinterpret_cast<unsigned char *>(bits64), ed->ExtraBytes(*this));
        }
    } else {
        p | bits64;
    }
}

Spike::Type Spike::DefaultType;

size_t Spike::Editor::ExtraBytes(const Data &data) const
{
    return 0;
}

void *Spike::Editor::AllocateExtra(Data &data)
{
    return nullptr;
}

size_t Spike::Editor::AllBytes(const Data &data) const
{
    return 0;
}

void Spike::Editor::ExportAll(Data &data, void *buffer, size_t size) const
{
    // do nothing
}

void Spike::Editor::ImportAll(Data &data, const void *buffer, size_t size)
{
    // do nothing
}

void Spike::Editor::Initialize(Data &data, size_t allocCount)
{
    // do nothing
}

void Spike::Editor::Release(Data &data)
{
    // do nothing
}

Spike::Type Spike::GetType(const Data &data)
{
    return data.type;
}

NeuronId Spike::GetSender(const Data &data)
{
    return data.sender;
}

void Spike::Initialize(Type type, NeuronId sender, Data &data, size_t allocCount)
{
    data.type = type;
    data.sender = sender;

    Edit(data)->Initialize(data, allocCount);
}

Spike::Editor *Spike::Edit(Data &data)
{
    SpikeEditorCache *editorCache = SpikeEditorCache::GetInstance();
    return editorCache->Get(data.type);
}

void Spike::Release(Data &data)
{
    Edit(data)->Release(data);
}

void BinarySpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

void BinarySpike::Initialize(Spike::Data &data, size_t allocCount)
{
    uint8_t value = 1;
    data.bits64 = value;
}

size_t BinarySpike::AllBytes(const Spike::Data &data) const
{
    return 1;
}


void BinarySpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    CheckedMemCopy(buffer, &data.bits64, AllBytes(data), size, __func__);
}

void DiscreteSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    CheckedMemCopy(&data.bits64, buffer, AllBytes(data), size, __func__);
}

void DiscreteSpike::Initialize(Spike::Data &data, size_t allocCount)
{
    SetIntensity(data, 1);
    SetDelay(data, 0);
}

void DiscreteSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

size_t DiscreteSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(uint64_t);
}

void DiscreteSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    CheckedMemCopy(buffer, &data.bits64, AllBytes(data), size, __func__);
}

uint64_t DiscreteSpike::GetIntensity(const Spike::Data &data) const
{
    return data.bits64;
}

void DiscreteSpike::SetIntensity(Spike::Data &data, uint64_t intensity)
{
    data.bits64 = intensity;
}

uint16_t DiscreteSpike::GetDelay(const Spike::Data &data) const
{
    return data.bits16;
}

void DiscreteSpike::SetDelay(Spike::Data &data, uint16_t delay)
{
    data.bits16 = delay;
}

void ContinuousSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

size_t ContinuousSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(double);
}

void ContinuousSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    CheckedMemCopy(buffer, &data.bits64, AllBytes(data), size, __func__);
}

void ContinuousSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    CheckedMemCopy(&data.bits64, buffer, AllBytes(data), size, __func__);
}

void ContinuousSpike::Initialize(Spike::Data &data, size_t allocCount)
{
    SetIntensity(data, 1.0);
    SetDelay(data, 0);
}

double ContinuousSpike::GetIntensity(const Spike::Data &data) const
{
    return *reinterpret_cast<const double *>(&data.bits64);
}

void ContinuousSpike::SetIntensity(Spike::Data &data, double intensity)
{
    data.bits64 = *reinterpret_cast<uint64_t *>(&intensity);
}

uint16_t ContinuousSpike::GetDelay(const Spike::Data &data) const
{
    return data.bits16;
}

void ContinuousSpike::SetDelay(Spike::Data &data, uint16_t delay)
{
    data.bits16 = delay;
}

void VisualSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

size_t VisualSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(uint32_t);
}

void VisualSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    CheckedMemCopy(buffer, &data.bits64, AllBytes(data), size, __func__);
}

void VisualSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    CheckedMemCopy(&data.bits64, buffer, AllBytes(data), size, __func__);
}

void VisualSpike::Initialize(Spike::Data &data, size_t allocCount)
{
    SetPixel(data, 0xFFFFFFFF);
}

uint32_t VisualSpike::GetPixel(const Spike::Data &data) const
{
    return data.bits64;
}

void VisualSpike::SetPixel(Spike::Data &data, uint32_t pixel)
{
    data.bits64 = pixel;
}

void FunctionalSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

size_t FunctionalSpike::ExtraBytes(const Spike::Data &data) const
{
    return data.bits16;
}

void *FunctionalSpike::AllocateExtra(Spike::Data &data)
{
    return std::malloc(ExtraBytes(data));
}

void FunctionalSpike::Initialize(Spike::Data &data, size_t allocCount)
{
    data.bits16 = 0;
    SetFunction(data, 0);
}

void FunctionalSpike::Release(Spike::Data &data)
{
    if (ExtraBytes(data) > 0 && data.bits64 != 0) {
        void *extra = reinterpret_cast<void *>(data.bits64);
        std::free(extra);
        data.bits64 = 0;
        data.bits16 = 0;
    }
}

uint8_t FunctionalSpike::GetFunction(const Spike::Data &data) const
{
    return data.bits8;
}

void FunctionalSpike::SetFunction(Spike::Data &data, uint8_t function)
{
    data.bits8 = function;
}

void FunctionalSpike::GetArguments(const Spike::Data &data, void *arguments, size_t size) const
{
    if (ExtraBytes(data) > 0 && ExtraBytes(data) == size) {
        void *extra = reinterpret_cast<void *>(data.bits64);
        memcpy(arguments, extra, size);
    }
}

void FunctionalSpike::SetArguments(Spike::Data &data, const void *arguments, size_t size)
{
    if (ExtraBytes(data) == 0 && size > 0) {
        data.bits16 = size;
        data.bits64 = reinterpret_cast<uintptr_t>(AllocateExtra(data));
        void *extra = reinterpret_cast<void *>(data.bits64);
        memcpy(extra, arguments, size);
    }
}

void MultiByteSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    receiver.HandleSpike(direction, *this, data);
}

size_t MultiByteSpike::ExtraBytes(const Spike::Data &data) const
{
    return data.bits16; // The spike contains a byte array of size == data.bits16.
}

void *MultiByteSpike::AllocateExtra(Spike::Data &data)
{
    return mAllocator.allocate(data.bits16);
}

void MultiByteSpike::Initialize(Spike::Data &data, size_t allocCount)
{
    if (allocCount > (std::numeric_limits<uint16_t>::max)()) {
        Log(LogLevel::Error, "%s: allocCount argument (%d) does not fit into uint16_t", __func__, allocCount);
        return;
    }

    data.bits16 = allocCount;

    uint8_t * ext = mAllocator.allocate(data.bits16);
    mAllocator.construct(ext);
    data.bits64 = reinterpret_cast<uintptr_t>(ext);
}

void MultiByteSpike::Release(Spike::Data &data)
{
    if (GetValueCount(data) > 0 && data.bits64 != 0) {
        uint8_t *extra = reinterpret_cast<uint8_t*>(data.bits64);
        mAllocator.deallocate(extra, data.bits16);
        data.bits64 = 0;
        data.bits16 = 0;
    }
}

size_t MultiByteSpike::AllBytes(const Spike::Data &data) const
{
    return GetValueCount(data);
}

void MultiByteSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    uint8_t *values = reinterpret_cast<uint8_t*>(data.bits64);
    CheckedMemCopy(buffer, values, AllBytes(data), size, __func__);
}

void MultiByteSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    uint8_t *values = reinterpret_cast<uint8_t*>(data.bits64);
    CheckedMemCopy(values, buffer, AllBytes(data), size, __func__);
}

void MultiByteSpike::GetValues(const Spike::Data &data, uint8_t *values, size_t count) const
{
    size_t size = count * sizeof(uint8_t);
    if (ExtraBytes(data) > 0 && ExtraBytes(data) == size) {
        void *extra = reinterpret_cast<void *>(data.bits64);
        memcpy(values, extra, size);
    }
}

const uint8_t * MultiByteSpike::GetValues(const Spike::Data &data) const
{
    return reinterpret_cast<uint8_t*>(data.bits64);
}

void MultiByteSpike::SetValues(Spike::Data &data, const uint8_t *values, size_t count)
{
    size_t size = count * sizeof(uint8_t);
    if (size > (std::numeric_limits<uint16_t>::max)()) {
        Log(LogLevel::Error, "%s: size %d does not fit into uint16_t", __func__, size);
        return;
    }

    if (ExtraBytes(data) == 0 && count > 0) {
        data.bits16 = size;
        data.bits64 = reinterpret_cast<uintptr_t>(AllocateExtra(data));
        void *extra = reinterpret_cast<void *>(data.bits64);
        memcpy(extra, values, size);
    }
}

size_t MultiByteSpike::GetValueCount(const Spike::Data &data) const
{
    return data.bits16;
}

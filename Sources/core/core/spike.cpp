#include "neuron.h"

#include "spike.h"

Spike::Spike()
{
    mEditors.resize(((size_t)UINT8_MAX) + 1);
    mEditors[static_cast<size_t>(Type::Binary)].reset(new BinarySpike());
    mEditors[static_cast<size_t>(Type::Discrete)].reset(new DiscreteSpike());
    mEditors[static_cast<size_t>(Type::Continuous)].reset(new ContinuousSpike());
    mEditors[static_cast<size_t>(Type::Visual)].reset(new VisualSpike());
    mEditors[static_cast<size_t>(Type::Functional)].reset(new FunctionalSpike());
}

Spike::Type Spike::ParseType(const std::string &type)
{
    if (type == "Binary") return Type::Binary;
    if (type == "Discrete") return Type::Discrete;
    if (type == "Continuous") return Type::Continuous;
    if (type == "Visual") return Type::Visual;
    if (type == "Functional") return Type::Functional;

    return Type::Binary;
}

const char *Spike::SerializeType(Type type)
{
    if (type == Type::Binary) return "Binary";
    if (type == Type::Discrete) return "Discrete";
    if (type == Type::Continuous) return "Continuous";
    if (type == Type::Visual) return "Visual";
    if (type == Type::Functional) return "Functional";

    return "Binary";
}

Spike Spike::instance;

Spike::Data::Data()
{
    type = Type::Binary;
}

void Spike::Data::pup(PUP::er &p)
{
    uint8_t temp;
    if (p.isUnpacking()) {
        p | temp;
        type = static_cast<Type>(temp);
    } else {
        temp = static_cast<uint8_t>(type);
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
        p(reinterpret_cast<unsigned char *>(bits64), ed->ExtraBytes(*this));
    } else {
        p | bits64;
    }
}

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

void Spike::Editor::Initialize(Data &data)
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

void Spike::Initialize(Type type, NeuronId sender, Data &data)
{
    data.type = type;
    data.sender = sender;

    Edit(data)->Initialize(data);
}

Spike::Editor *Spike::Edit(Data &data)
{
    Editor *editor = instance.mEditors[static_cast<size_t>(data.type)].get();
    if (editor == nullptr) {
        editor = instance.mEditors[static_cast<size_t>(Type::Binary)].get();
    }

    return editor;
}

void Spike::Release(Data &data)
{
    Edit(data)->Release(data);
}

bool BinarySpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    return receiver.HandleSpike(direction, *this, data);
}

bool DiscreteSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    return receiver.HandleSpike(direction, *this, data);
}

size_t DiscreteSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(uint64_t);
}

void DiscreteSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    if (size == AllBytes(data)) {
        std::memcpy(buffer, &data.bits64, size);
    }
}

void DiscreteSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    if (size == AllBytes(data)) {
        std::memcpy(&data.bits64, buffer, size);
    }
}

void DiscreteSpike::Initialize(Spike::Data &data)
{
    SetIntensity(data, 1);
    SetDelay(data, 0);
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

bool ContinuousSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    return receiver.HandleSpike(direction, *this, data);
}

size_t ContinuousSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(double);
}

void ContinuousSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    if (size == AllBytes(data)) {
        std::memcpy(buffer, &data.bits64, size);
    }
}

void ContinuousSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    if (size == AllBytes(data)) {
        std::memcpy(&data.bits64, buffer, size);
    }
}

void ContinuousSpike::Initialize(Spike::Data &data)
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

bool VisualSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    return receiver.HandleSpike(direction, *this, data);
}

size_t VisualSpike::AllBytes(const Spike::Data &data) const
{
    return sizeof(uint32_t);
}

void VisualSpike::ExportAll(Spike::Data &data, void *buffer, size_t size) const
{
    if (size == AllBytes(data)) {
        std::memcpy(buffer, &data.bits64, size);
    }
}

void VisualSpike::ImportAll(Spike::Data &data, const void *buffer, size_t size)
{
    if (size == AllBytes(data)) {
        std::memcpy(&data.bits64, buffer, size);
    }
}

void VisualSpike::Initialize(Spike::Data &data)
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

bool FunctionalSpike::Accept(Direction direction, Neuron &receiver, Spike::Data &data)
{
    return receiver.HandleSpike(direction, *this, data);
}

size_t FunctionalSpike::ExtraBytes(const Spike::Data &data) const
{
    return data.bits16;
}

void *FunctionalSpike::AllocateExtra(Spike::Data &data)
{
    return scalable_malloc(ExtraBytes(data));
}

void FunctionalSpike::Initialize(Spike::Data &data)
{
    data.bits16 = 0;
    SetFunction(data, 0);
}

void FunctionalSpike::Release(Spike::Data &data)
{
    if (ExtraBytes(data) > 0) {
        void *extra = reinterpret_cast<void *>(data.bits64);
        scalable_free(extra);
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
        std::memcpy(arguments, extra, size);
    }
}

void FunctionalSpike::SetArguments(Spike::Data &data, const void *arguments, size_t size)
{
    if (ExtraBytes(data) == 0 && size > 0) {
        data.bits16 = size;
        data.bits64 = reinterpret_cast<uintptr_t>(AllocateExtra(data));
        void *extra = reinterpret_cast<void *>(data.bits64);
        std::memcpy(extra, arguments, size);
    }
}

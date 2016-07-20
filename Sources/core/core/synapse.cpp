#include "synapse.h"

Synapse::Synapse()
{
    mEditors.resize((static_cast<size_t>(UINT8_MAX)) + 1);
    mEditors[static_cast<size_t>(Type::Empty)].reset(new Editor());
    mEditors[static_cast<size_t>(Type::Weighted)].reset(new WeightedSynapse());
    mEditors[static_cast<size_t>(Type::Lagging)].reset(new LaggingSynapse());
    mEditors[static_cast<size_t>(Type::Conductive)].reset(new ConductiveSynapse());
    mEditors[static_cast<size_t>(Type::Probabilistic)].reset(new ProbabilisticSynapse());
    mEditors[static_cast<size_t>(Type::MultiWeighted)].reset(new MultiWeightedSynapse());
}

Synapse::Type Synapse::ParseType(const std::string &type)
{
    if (type == "Empty") return Type::Empty;
    if (type == "Weighted") return Type::Weighted;
    if (type == "Lagging") return Type::Lagging;
    if (type == "Conductive") return Type::Conductive;
    if (type == "Probabilistic") return Type::Probabilistic;
    if (type == "MultiWeighted") return Type::MultiWeighted;

    return Type::Empty;
}

const char *Synapse::SerializeType(Type type)
{
    if (type == Type::Empty) return "Empty";
    if (type == Type::Weighted) return "Weighted";
    if (type == Type::Lagging) return "Lagging";
    if (type == Type::Conductive) return "Conductive";
    if (type == Type::Probabilistic) return "Probabilistic";
    if (type == Type::MultiWeighted) return "MultiWeighted";

    return "Empty";
}

Synapse Synapse::instance;

Synapse::Data::Data()
{
    type = Type::Empty;
    bits8 = 0;
    bits16 = 0;
    bits64 = 0;
}

Synapse::Data::~Data()
{
    Synapse::Release(*this);
}

Synapse::Data::Data(const Data &other)
{
    type = other.type;
    bits8 = other.bits8;
    bits16 = other.bits16;

    Editor *ed = Edit(*this);
    if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
        bits64 = reinterpret_cast<uintptr_t>(ed->AllocateExtra(*this));
        std::memcpy(reinterpret_cast<unsigned char *>(bits64), 
            reinterpret_cast<unsigned char *>(other.bits64), ed->ExtraBytes(*this));
    } else {
        bits64 = other.bits64;
    }
}

Synapse::Data::Data(Data &&other)
{
    type = other.type;
    bits8 = other.bits8;
    bits16 = other.bits16;
    
    Editor *ed = Edit(*this);
    if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
        bits64 = other.bits64;
        other.bits64 = 0;
    } else {
        bits64 = other.bits64;
    }
}

Synapse::Data &Synapse::Data::operator=(const Data &other)
{
    if (this != &other)
    {
        type = other.type;
        bits8 = other.bits8;
        bits16 = other.bits16;
        
        Editor *ed = Edit(*this);
        if (ed->ExtraBytes(*this) > 0 && other.bits64 != 0) {
            bits64 = reinterpret_cast<uintptr_t>(ed->AllocateExtra(*this));
            std::memcpy(reinterpret_cast<unsigned char *>(bits64),
                reinterpret_cast<unsigned char *>(other.bits64), ed->ExtraBytes(*this));
        } else {
            bits64 = other.bits64;
        }
    }
    return *this;
}

Synapse::Data &Synapse::Data::operator=(Data &&other)
{
    if (this != &other)
    {
        type = other.type;
        bits8 = other.bits8;
        bits16 = other.bits16;
        
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

void Synapse::Data::pup(PUP::er &p)
{
    uint8_t temp;
    if (p.isUnpacking()) {
        p | temp;
        type = static_cast<Type>(temp);
    } else {
        temp = static_cast<uint8_t>(type);
        p | temp;
    }

    p | bits16;

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

size_t Synapse::Editor::ExtraBytes(Data &data) const
{
    return 0;
}

void *Synapse::Editor::AllocateExtra(Data &data)
{
    return nullptr;
}

void Synapse::Editor::Initialize(Data &data, size_t allocCount)
{
    // do nothing
}

void Synapse::Editor::Clone(const Data &original, Data &data)
{
    // do nothing
}

void Synapse::Editor::Release(Data &data)
{
    // do nothing
}

Synapse::Type Synapse::GetType(const Data &data)
{
    return data.type;
}

void Synapse::Initialize(Type type, Data &data, size_t allocCount)
{
    if (type == Type::Empty) return;

    data.type = type;

    Edit(data)->Initialize(data, allocCount);
}

void Synapse::Clone(const Data &original, Data &data)
{
    Type type = Synapse::GetType(original);
    if (type == Type::Empty) return;

    data.type = type;

    Edit(data)->Clone(original, data);
}

Synapse::Editor *Synapse::Edit(Data &data)
{
    Editor *editor = instance.mEditors[static_cast<size_t>(data.type)].get();
    if (editor == nullptr) {
        editor = instance.mEditors[static_cast<size_t>(Type::Empty)].get();
    }

    return editor;
}

void Synapse::Release(Data &data)
{
    if (data.type == Type::Empty) return;
    
    Edit(data)->Release(data);
}

void WeightedSynapse::Initialize(Synapse::Data &data, size_t allocSize)
{
    SetWeight(data, 1.0);
}

void WeightedSynapse::Clone(const Synapse::Data &original, Synapse::Data &data)
{
    SetWeight(data, GetWeight(original));
}

double WeightedSynapse::GetWeight(const Synapse::Data &data) const
{
    return *reinterpret_cast<const double *>(&data.bits64);
}

void WeightedSynapse::SetWeight(Synapse::Data &data, double weight)
{
    data.bits64 = *reinterpret_cast<uint64_t *>(&weight);
}

void LaggingSynapse::Initialize(Synapse::Data &data, size_t allocSize)
{
    SetWeight(data, 1.0);
    SetDelay(data, 0);
}

void LaggingSynapse::Clone(const Synapse::Data &original, Synapse::Data &data)
{
    SetWeight(data, GetWeight(original));
    SetDelay(data, GetDelay(original));
}

double LaggingSynapse::GetWeight(const Synapse::Data &data) const
{
    return *reinterpret_cast<const double *>(&data.bits64);
}

void LaggingSynapse::SetWeight(Synapse::Data &data, double weight)
{
    data.bits64 = *reinterpret_cast<uint64_t *>(&weight);
}

uint16_t LaggingSynapse::GetDelay(const Synapse::Data &data) const
{
    return data.bits16;
}

void LaggingSynapse::SetDelay(Synapse::Data &data, uint16_t delay)
{
    data.bits16 = delay;
}

void ConductiveSynapse::Initialize(Synapse::Data &data, size_t allocSize)
{
    SetWeight(data, 1.0f);
    SetDelay(data, 0);
    SetConductance(data, 1.0f);
}

void ConductiveSynapse::Clone(const Synapse::Data &original, Synapse::Data &data)
{
    SetWeight(data, GetWeight(original));
    SetDelay(data, GetDelay(original));
    SetConductance(data, GetConductance(original));
}

float ConductiveSynapse::GetWeight(const Synapse::Data &data) const
{
    uint32_t temp = static_cast<uint32_t>((data.bits64 & WeightMask) >> WeightOffset);
    return *reinterpret_cast<float *>(&temp);
}

void ConductiveSynapse::SetWeight(Synapse::Data &data, float weight)
{
    uint32_t temp = *reinterpret_cast<uint32_t *>(&weight);
    data.bits64 = ((data.bits64 & ~WeightMask) | (static_cast<uint64_t>(temp) << WeightOffset));
}

uint16_t ConductiveSynapse::GetDelay(const Synapse::Data &data) const
{
    return data.bits16;
}

void ConductiveSynapse::SetDelay(Synapse::Data &data, uint16_t delay)
{
    data.bits16 = delay;
}

float ConductiveSynapse::GetConductance(const Synapse::Data &data) const
{
    uint32_t temp = static_cast<uint32_t>((data.bits64 & ConductanceMask) >> ConductanceOffset);
    return *reinterpret_cast<float *>(&temp);
}

void ConductiveSynapse::SetConductance(Synapse::Data &data, float conductance)
{
    uint32_t temp = *reinterpret_cast<uint32_t *>(&conductance);
    data.bits64 = ((data.bits64 & ~ConductanceMask) | (static_cast<uint64_t>(temp) << ConductanceOffset));
}

ProbabilisticSynapse::DataExtended::DataExtended() :
    mean(0.0f), variance(1.0f), weight(1.0)
{
}

size_t ProbabilisticSynapse::ExtraBytes(Synapse::Data &data) const
{
    return sizeof(DataExtended);
}

void *ProbabilisticSynapse::AllocateExtra(Synapse::Data &data)
{
    return mAllocator.allocate(1);
}

void ProbabilisticSynapse::Initialize(Synapse::Data &data, size_t allocSize)
{
    SetDelay(data, 0);

    DataExtended *ext = mAllocator.allocate(1);
    mAllocator.construct(ext);
    data.bits64 = reinterpret_cast<uintptr_t>(ext);
}

void ProbabilisticSynapse::Clone(const Synapse::Data &original, Synapse::Data &data)
{
    SetDelay(data, GetDelay(original));

    DataExtended *ext = mAllocator.allocate(1);
    data.bits64 = reinterpret_cast<uintptr_t>(ext);

    SetMean(data, GetMean(original));
    SetVariance(data, GetVariance(original));
    SetWeight(data, GetWeight(original));
}

void ProbabilisticSynapse::Release(Synapse::Data &data)
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    if (ext) mAllocator.deallocate(ext, 1);
}

double ProbabilisticSynapse::GetWeight(const Synapse::Data &data) const
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    return ext->weight;
}

void ProbabilisticSynapse::SetWeight(Synapse::Data &data, double weight)
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    ext->weight = weight;
}

uint16_t ProbabilisticSynapse::GetDelay(const Synapse::Data &data) const
{
    return data.bits16;
}

void ProbabilisticSynapse::SetDelay(Synapse::Data &data, uint16_t delay)
{
    data.bits16 = delay;
}

float ProbabilisticSynapse::GetMean(const Synapse::Data &data) const
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    return ext->mean;
}

void ProbabilisticSynapse::SetMean(Synapse::Data &data, float mean)
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    ext->mean = mean;
}

float ProbabilisticSynapse::GetVariance(const Synapse::Data &data) const
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    return ext->variance;
}

void ProbabilisticSynapse::SetVariance(Synapse::Data &data, float variance)
{
    DataExtended *ext = reinterpret_cast<DataExtended *>(data.bits64);
    ext->variance = variance;
}

size_t MultiWeightedSynapse::ExtraBytes(Synapse::Data &data) const
{
    return GetWeightCount(data) * sizeof(float);
}

void *MultiWeightedSynapse::AllocateExtra(Synapse::Data &data)
{
    return mAllocator.allocate(GetWeightCount(data));
}

void MultiWeightedSynapse::Initialize(Synapse::Data &data, size_t allocCount)
{
    data.bits16 = allocCount;

    float* ext = mAllocator.allocate(GetWeightCount(data));
    mAllocator.construct(ext);
    data.bits64 = reinterpret_cast<uintptr_t>(ext);
}

void MultiWeightedSynapse::Clone(const Synapse::Data &original, Synapse::Data &data)
{
    data.bits16 = GetWeightCount(original);

    data.bits64 = reinterpret_cast<uintptr_t>(AllocateExtra(data));
}

void MultiWeightedSynapse::Release(Synapse::Data &data)
{
    mAllocator.deallocate(reinterpret_cast<float*>(data.bits64), GetWeightCount(data));
}


void MultiWeightedSynapse::GetWeights(const Synapse::Data &data, float *weights, size_t count) const
{
    std::memcpy(weights, reinterpret_cast<float*>(data.bits64), count * sizeof(float));
}

void MultiWeightedSynapse::SetWeights(Synapse::Data &data, const float *weights, size_t count)
{
    std::memcpy(reinterpret_cast<float*>(data.bits64), weights, count * sizeof(float));
}

uint16_t MultiWeightedSynapse::GetWeightCount(const Synapse::Data &data) const
{
    return data.bits16;
}

#pragma once

#include <cstdint>
#include <tuple>
#include <vector>

#include <tbb/scalable_allocator.h>

#include <pup.h>

#include "common.h"

class Synapse
{
public:
    enum class Type : std::uint8_t
    {
        Empty = 0,
        Weighted = 1,
        Lagging = 2,
        Conductive = 3,
        Probabilistic = 4,
        MultiWeighted
    };

    static Type ParseType(const std::string &type);
    static const char *SerializeType(Type type);

    struct Data
    {
        Data();
        ~Data();
        Data(const Data &other);
        Data(Data &&other);
        Data &operator=(const Data &other);
        Data &operator=(Data &&other);

        void pup(PUP::er &p);

        Type type;
        uint8_t bits8;
        uint16_t bits16;
        uint64_t bits64;
    };

    typedef std::tuple<Direction, NeuronId, NeuronId, Data> Addition;
    typedef std::tuple<Direction, NeuronId, NeuronId> Removal;
    typedef std::pair<NeuronId, NeuronId> Link;

    typedef std::vector<Addition> Additions;
    typedef std::vector<Removal> Removals;
    typedef std::vector<Link> Links;

    class Editor
    {
    public:
        virtual ~Editor() = default;

        virtual size_t ExtraBytes(Data &data) const;
        virtual void *AllocateExtra(Data &data);

        virtual void Initialize(Data &data, size_t allocSize = 0);
        virtual void Clone(const Data &original, Data &data);
        virtual void Release(Data &data);
    };

    static Type GetType(const Data &data);
    static void Initialize(Type type, Data &data, size_t allocSize = 0);
    static void Clone(const Data &original, Data &data);
    static Editor *Edit(Data &data);
    static void Release(Data &data);

private:
    Synapse();
    Synapse(const Synapse &other) = delete;
    Synapse &operator=(const Synapse &other) = delete;

    static Synapse instance;

    std::vector<std::unique_ptr<Editor>> mEditors;
};

class WeightedSynapse : public Synapse::Editor
{
public:
    virtual void Initialize(Synapse::Data &data, size_t allocSize = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;

    double GetWeight(const Synapse::Data &data) const;
    void SetWeight(Synapse::Data &data, double weight);
};

class MultiWeightedSynapse : public Synapse::Editor
{
public:
    virtual size_t ExtraBytes(Synapse::Data &data) const override;
    virtual void *AllocateExtra(Synapse::Data &data) override;

    virtual void Initialize(Synapse::Data &data, size_t allocSize = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;
    virtual void Release(Synapse::Data &data) override;

    void GetWeights(const Synapse::Data &data, float *weights, size_t count) const;
    void SetWeights(Synapse::Data &data, const float *weights, size_t count);

    size_t GetWeightCount(const Synapse::Data &data) const;
private:
    tbb::scalable_allocator<float> mAllocator;
};

class LaggingSynapse : public Synapse::Editor
{
public:
    virtual void Initialize(Synapse::Data &data, size_t allocCount = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;

    double GetWeight(const Synapse::Data &data) const;
    void SetWeight(Synapse::Data &data, double weight);
    uint16_t GetDelay(const Synapse::Data &data) const;
    void SetDelay(Synapse::Data &data, uint16_t delay);
};

class ConductiveSynapse : public Synapse::Editor
{
public:
    virtual void Initialize(Synapse::Data &data, size_t allocCount = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;

    float GetWeight(const Synapse::Data &data) const;
    void SetWeight(Synapse::Data &data, float weight);
    uint16_t GetDelay(const Synapse::Data &data) const;
    void SetDelay(Synapse::Data &data, uint16_t delay);
    float GetConductance(const Synapse::Data &data) const;
    void SetConductance(Synapse::Data &data, float conductance);

protected:
    static const uint8_t WeightOffset = 0;
    static const uint64_t WeightMask = 0x00000000FFFFFFFF;
    static const uint8_t ConductanceOffset = 32;
    static const uint64_t ConductanceMask = 0xFFFFFFFF00000000;
};

class ProbabilisticSynapse : public Synapse::Editor
{
public:
    virtual size_t ExtraBytes(Synapse::Data &data) const override;
    virtual void *AllocateExtra(Synapse::Data &data) override;

    virtual void Initialize(Synapse::Data &data, size_t allocCount = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;
    virtual void Release(Synapse::Data &data) override;

    double GetWeight(const Synapse::Data &data) const;
    void SetWeight(Synapse::Data &data, double weight);
    uint16_t GetDelay(const Synapse::Data &data) const;
    void SetDelay(Synapse::Data &data, uint16_t delay);
    float GetMean(const Synapse::Data &data) const;
    void SetMean(Synapse::Data &data, float mean);
    float GetVariance(const Synapse::Data &data) const;
    void SetVariance(Synapse::Data &data, float variance);

protected:
    struct DataExtended
    {
        DataExtended();

        float mean;
        float variance;
        double weight;
    };

private:
    tbb::scalable_allocator<DataExtended> mAllocator;
};

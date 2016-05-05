#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include <tbb/scalable_allocator.h>

#include <pup.h>

#include "common.h"

class Neuron;

class Spike
{
public:
    enum class Type : std::uint8_t
    {
        Binary = 0,
        Discrete = 1,
        Continuous = 2,
        Visual = 3,
        Functional = 4
    };

    static Type ParseType(const std::string &type);
    static const char *SerializeType(Type type);

    struct Data
    {
        Data();

        void pup(PUP::er &p);

        Spike::Type type;
        uint8_t bits8;
        uint16_t bits16;
        NeuronId sender;
        uint64_t bits64;
    };

    typedef std::pair<NeuronId, Data> DataWithReceiver;
    typedef std::vector<DataWithReceiver> BrainSink;
    typedef std::vector<Data> BrainSource;

    class Editor
    {
    public:
        virtual ~Editor() = default;
        
        virtual bool Accept(Direction direction, Neuron &receiver, Data &data) = 0;

        virtual size_t ExtraBytes(const Data &data) const;
        virtual void *AllocateExtra(Data &data);

        virtual size_t AllBytes(const Data &data) const;
        virtual void ExportAll(Data &data, void *buffer, size_t size) const;
        virtual void ImportAll(Data &data, const void *buffer, size_t size);

        virtual void Initialize(Data &data);
        virtual void Release(Data &data);
    };

    static Type GetType(const Data &data);
    static NeuronId GetSender(const Data &data);
    static void Initialize(Type type, NeuronId sender, Data &data);
    static Editor *Edit(Data &data);
    static void Release(Data &data);

private:
    Spike();
    Spike(const Spike &other) = delete;
    Spike &operator=(const Spike &other) = delete;

    static Spike instance;

    std::vector<std::unique_ptr<Editor>> mEditors;
};

inline void operator|(PUP::er &p, Spike::Type &spikeType) {
    pup_bytes(&p, (void *)&spikeType, sizeof(Spike::Type));
}

class BinarySpike : public Spike::Editor
{
    virtual bool Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;
};

class DiscreteSpike : public Spike::Editor
{
public:
    virtual bool Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data) override;

    uint64_t GetIntensity(const Spike::Data &data) const;
    void SetIntensity(Spike::Data &data, uint64_t intensity);
    uint16_t GetDelay(const Spike::Data &data) const;
    void SetDelay(Spike::Data &data, uint16_t delay);
};

class ContinuousSpike : public Spike::Editor
{
public:
    virtual bool Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data) override;

    double GetIntensity(const Spike::Data &data) const;
    void SetIntensity(Spike::Data &data, double intensity);
    uint16_t GetDelay(const Spike::Data &data) const;
    void SetDelay(Spike::Data &data, uint16_t delay);
};

class VisualSpike : public Spike::Editor
{
public:
    virtual bool Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data) override;

    uint32_t GetPixel(const Spike::Data &data) const;
    void SetPixel(Spike::Data &data, uint32_t pixel);
};

class FunctionalSpike : public Spike::Editor
{
public:
    virtual bool Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t ExtraBytes(const Spike::Data &data) const override;
    virtual void *AllocateExtra(Spike::Data &data) override;

    virtual void Initialize(Spike::Data &data) override;
    virtual void Release(Spike::Data &data) override;

    uint8_t GetFunction(const Spike::Data &data) const;
    void SetFunction(Spike::Data &data, uint8_t function);
    void GetArguments(const Spike::Data &data, void *arguments, size_t size) const;
    void SetArguments(Spike::Data &data, const void *arguments, size_t size);
};

#pragma once

#include <cstdint>
#include <vector>
#include <string>

#include <pup.h>

#include "common.h"
#include "registration.h"

class Neuron;

class Spike
{
public:
    using Type = Token8;

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
        
        virtual void Accept(Direction direction, Neuron &receiver, Data &data) = 0;

        virtual size_t ExtraBytes(const Data &data) const;
        virtual void *AllocateExtra(Data &data);

        virtual size_t AllBytes(const Data &data) const;
        virtual void ExportAll(Data &data, void *buffer, size_t size) const;
        virtual void ImportAll(Data &data, const void *buffer, size_t size);

        virtual void Initialize(Data &data, size_t allocCount = 0);
        virtual void Release(Data &data);
    };


    static Type GetType(const Data &data);
    static NeuronId GetSender(const Data &data);
    static void Initialize(Type type, NeuronId sender, Data &data, size_t allocCount = 0);
    static Editor *Edit(Data &data);
    static void Release(Data &data);

    static Type DefaultType;

private:
    Spike(const Spike &other) = delete;
    Spike &operator=(const Spike &other) = delete;
};

class BinarySpike : public Spike::Editor
{
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;
};

class DiscreteSpike : public Spike::Editor
{
public:
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;

    uint64_t GetIntensity(const Spike::Data &data) const;
    void SetIntensity(Spike::Data &data, uint64_t intensity);
    uint16_t GetDelay(const Spike::Data &data) const;
    void SetDelay(Spike::Data &data, uint16_t delay);
};

class ContinuousSpike : public Spike::Editor
{
public:
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;

    double GetIntensity(const Spike::Data &data) const;
    void SetIntensity(Spike::Data &data, double intensity);
    uint16_t GetDelay(const Spike::Data &data) const;
    void SetDelay(Spike::Data &data, uint16_t delay);
};

class VisualSpike : public Spike::Editor
{
public:
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;

    uint32_t GetPixel(const Spike::Data &data) const;
    void SetPixel(Spike::Data &data, uint32_t pixel);
};

class FunctionalSpike : public Spike::Editor
{
public:
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t ExtraBytes(const Spike::Data &data) const override;
    virtual void *AllocateExtra(Spike::Data &data) override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;
    virtual void Release(Spike::Data &data) override;

    uint8_t GetFunction(const Spike::Data &data) const;
    void SetFunction(Spike::Data &data, uint8_t function);
    void GetArguments(const Spike::Data &data, void *arguments, size_t size) const;
    void SetArguments(Spike::Data &data, const void *arguments, size_t size);
};

class MultiByteSpike : public Spike::Editor
{
public:
    virtual void Accept(Direction direction, Neuron &receiver, Spike::Data &data) override;

    virtual size_t ExtraBytes(const Spike::Data &data) const override;
    virtual void *AllocateExtra(Spike::Data &data) override;

    virtual void Initialize(Spike::Data &data, size_t allocCount = 0) override;
    virtual void Release(Spike::Data &data) override;

    virtual size_t AllBytes(const Spike::Data &data) const override;
    virtual void ExportAll(Spike::Data &data, void *buffer, size_t size) const override;
    virtual void ImportAll(Spike::Data &data, const void *buffer, size_t size) override;

    void GetValues(const Spike::Data &data, uint8_t *values, size_t count) const;
    const uint8_t * GetValues(const Spike::Data &data) const;
    void SetValues(Spike::Data &data, const uint8_t *values, size_t count);

    size_t GetValueCount(const Spike::Data &data) const;
private:
    CUSTOM_ALLOCATOR<uint8_t> mAllocator;
};

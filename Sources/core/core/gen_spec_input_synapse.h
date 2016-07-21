#pragma once
#include "synapse.h"

class GenSpecInputSynapse : MultiWeightedSynapse
{
public:
    virtual size_t ExtraBytes(Synapse::Data &data) const override;
    virtual void *AllocateExtra(Synapse::Data &data) override;

    virtual void Initialize(Synapse::Data &data, size_t allocSize = 0) override;
    virtual void Clone(const Synapse::Data &original, Synapse::Data &data) override;
    virtual void Release(Synapse::Data &data) override;

    void GetWeights(const Synapse::Data &data, float *weights, size_t count) const;
    void SetWeights(Synapse::Data &data, const float *weights, size_t count);
};

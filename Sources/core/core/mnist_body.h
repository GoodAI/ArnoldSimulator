#pragma once

#include "body.h"
#include "mnist_reader.h"

class MnistBody : public Body
{
protected:
    MnistReader mDigitReader;

public:
    static const char *Type;

    explicit MnistBody(json &params);
    virtual ~MnistBody();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual void Simulate(
        size_t bodyStep,
        std::function<void(const std::string &, std::vector<uint8_t> &)> pushSensoMotoricData,
        std::function<void(const std::string &, std::vector<uint8_t> &)> pullSensoMotoricData
    ) override;
};

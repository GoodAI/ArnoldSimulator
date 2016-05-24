#pragma once

#include <cstdint>
#include <string>
#include <vector>
#include <functional>

#include <json.hpp>

#include <pup.h>
#include <pup_stl.h>

using namespace nlohmann;

class Body
{
public:
    static Body *CreateBody(const std::string &type, json &params);

    explicit Body(json &params);
    virtual ~Body() = default;

    Body(const Body &other) = delete;
    Body &operator=(const Body &other) = delete;

    virtual void pup(PUP::er &p) = 0;

    virtual const char *GetType() = 0;

    virtual void Simulate(
        std::function<void(std::string &, std::vector<uint8_t> &)> pushSensoMotoricData,
        std::function<void(std::string &, std::vector<uint8_t> &)> pullSensoMotoricData
    ) = 0;
};

class RandomBody : public Body
{
public:
    static const char *Type;

    explicit RandomBody(json &params);
    virtual ~RandomBody();

    virtual void pup(PUP::er &p) override;

    virtual const char *GetType() override;

    virtual void Simulate(
        std::function<void(std::string &, std::vector<uint8_t> &)> pushSensoMotoricData,
        std::function<void(std::string &, std::vector<uint8_t> &)> pullSensoMotoricData
    ) override;
};

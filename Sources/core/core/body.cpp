#include "random.h"

#include "body.h"

Body *Body::CreateBody(const std::string &type, json &params)
{
    if (type == RandomBody::Type) {
        return new RandomBody(params);
    } else {
        return nullptr;
    }
}

Body::Body(json &params)
{
}

const char *RandomBody::Type = "RandomBody";

RandomBody::RandomBody(json &params) : Body(params)
{
}

RandomBody::~RandomBody()
{
}

const char *RandomBody::GetType()
{
    return Type;
}

void RandomBody::Simulate(
    std::function<void(std::string &, std::vector<uint8_t> &)> pushSensoMotoricData,
    std::function<void(std::string &, std::vector<uint8_t> &)> pullSensoMotoricData)
{
    /*
    for each actuator in actuators
        pullSensoMotoricData(actuator.Name, actuator.Data);

    // let world react to motoric output, simulate next timestep and prepare sensoric input

    for each sensor in sensors
        pushSensoMotoricData(sensor.Name, sensor.Data);
    */
}

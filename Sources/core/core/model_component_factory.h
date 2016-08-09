#pragma once
#include <unordered_map>
#include "registration.h"
#include <json.hpp>

// A template for factories of neurons, regions and brains.
// The factory functions must receive parameters (TBase &base, json &params).
template<typename TComponent, typename TBase, typename TToken>
using FactoryFunction = TComponent *(*)(TBase &, nlohmann::json &);

template<typename TComponent, typename TBase, typename TToken>
class ModelComponentFactory : public Registration<ModelComponentFactory<TComponent, TBase, TToken>, TToken>
{
    using Base = Registration<ModelComponentFactory<TComponent, TBase, TToken>, TToken>;
    using Function = FactoryFunction<TComponent, TBase, TToken>;
public:
    TToken Register(const std::string &name, Function create)
    {
        TToken token = Base::GetNewToken(name);
        mFactoryFunctions[token] = create;
        return token;
    }

    TComponent *Create(TToken token, TBase &base, nlohmann::json &params) const
    {
        auto functionIt = mFactoryFunctions.find(token);
        if (functionIt == mFactoryFunctions.end()) {
            Log(LogLevel::Error, "Item with token %d was not registered", token);
            throw std::invalid_argument("Token not registered");
        }
        return (functionIt->second)(base, params);
    }

    TComponent *Create(const std::string &name, TBase &base, nlohmann::json &params) const
    {
        return Create(Base::GetToken(name), base, params);
    }

private:
    std::unordered_map<TToken, Function> mFactoryFunctions;
};

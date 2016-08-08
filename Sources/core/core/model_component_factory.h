#pragma once
#include <map>
#include "registration.h"
#include <json.hpp>

// A template for factories of neurons, regions and brains.
// The factory functions must receive parameters (TBase &base, json &params).
template<typename TComponent, typename TBase, typename TToken>
using FactoryMethod = TComponent *(*)(TBase &, nlohmann::json &);

template<typename TComponent, typename TBase, typename TToken>
class ModelComponentFactory : public Registration<ModelComponentFactory<TComponent, TBase, TToken>, TToken>
{
    using Base = Registration<ModelComponentFactory<TComponent, TBase, TToken>, TToken>;
public:
    TToken Register(const std::string &name, FactoryMethod<TComponent, TBase, TToken> create)
    {
        TToken token = Base::GetNewToken(name);
        mFactoryFunctions[token] = create;
        return token;
    }

    TComponent *Create(TToken token, TBase &base, nlohmann::json &params) const
    {
        return mFactoryFunctions.at(token)(base, params);
    }

    TComponent *Create(const std::string &name, TBase &base, nlohmann::json &params) const
    {
        return Create(Base::GetToken(name), base, params);
    }

private:
    std::map<TToken, FactoryMethod<TComponent, TBase, TToken>> mFactoryFunctions;
};

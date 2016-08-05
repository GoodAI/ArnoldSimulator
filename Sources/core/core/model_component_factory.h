#pragma once
#include <map>
#include "registration.h"
#include <json.hpp>

// A template for factories of neurons, regions and brains.
// The factory functions must receive parameters (TBase &base, json &params).
template<typename TComponent, typename TBase>
using FactoryMethod = TComponent *(*)(TBase &, nlohmann::json &);

template<typename TComponent, typename TBase>
class ModelComponentFactory : public Registration<ModelComponentFactory<TComponent, TBase>>
{
public:
    Token Register(const std::string &name, FactoryMethod<TComponent, TBase> create)
    {
        Token token = Registration<ModelComponentFactory<TComponent, TBase>>::GetNewToken(name);
        mFactoryFunctions[token] = create;
        return token;
    }

    TComponent *Create(Token token, TBase &base, nlohmann::json params) const
    {
        return mFactoryFunctions.at(token)(base, params);
    }

private:
    std::map<Token, FactoryMethod<TComponent, TBase>> mFactoryFunctions;
};

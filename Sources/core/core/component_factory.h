#pragma once
#include <map>
#include "registration.h"
#include <json.hpp>

// A template for factories of neurons, regions and brains.
// The factory functions must receive parameters (const BaseType &base, json &params).
template<typename ComponentType, typename BaseType>
class ComponentFactory : public Registration<ComponentFactory<ComponentType, BaseType>>
{
public:
    Token Register(const std::string &name, ComponentType *(*create)(BaseType &, nlohmann::json &))
    {
        Token token = Registration<ComponentFactory<ComponentType, BaseType>>::GetNewToken(name);
        mFactoryFunctions[token] = create;
        return token;
    }

    ComponentType *Create(Token token, BaseType &base, nlohmann::json params) const
    {
        return mFactoryFunctions.at(token)(base, params);
    }

private:
    std::map<Token, ComponentType *(*)(BaseType &, nlohmann::json &)> mFactoryFunctions;
};

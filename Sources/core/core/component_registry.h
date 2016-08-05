#pragma once
#include <map>
#include "registration.h"

// A template registry that registers an instance for a given name.
// Keep in mind that the instance must not hold any fields as they are
// not distributed to the other nodes. Used for synapse and spike editors.
template<typename ComponentType>
class ComponentRegistry : Registration<ComponentRegistry<ComponentType>>
{
public:
    ~ComponentRegistry()
    {
        for (ComponentType* instance : mInstances) {
            delete instance;
        }
        mInstances.clear();
    }

    Token Register(const std::string &name, ComponentType * component)
    {
        Token token = Registration<ComponentRegistry<ComponentType>>::GetNewToken(name);
        mInstances[token] = component;

        return token;
    }

    ComponentType *Get(Token token) const
    {
        return mInstances.at(token);
    }

private:
    std::map<Token, ComponentType*> mInstances;
};

#pragma once
#include <map>
#include "components.h"

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

    Token Register(const std::string &name, ComponentType *(*create)())
    {
        Token token = Registration<ComponentRegistry<ComponentType>>::GetNewToken(name);
        mInstances[token] = create();

        return token;
    }

    ComponentType *Get(Token token) const
    {
        return mInstances[token];
    }

private:
    std::map<Token, ComponentType*> mInstances;
};

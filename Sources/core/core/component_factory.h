#pragma once
#include <map>
#include "components.h"

template<typename ComponentType>
class ComponentFactory : public Registration<ComponentFactory<ComponentType>>
{
public:
    Token Register(const std::string &name, ComponentType *(*create)())
    {
        Token token = Registration<ComponentFactory<ComponentType>>::GetNewToken(name);
        mFactoryFunctions[token] = create;
        return token;
    }

    ComponentType *Create(Token token) const
    {
        return mFactoryFunctions[token]();
    }

private:
    std::map<Token, ComponentType *(*)()> mFactoryFunctions;
};

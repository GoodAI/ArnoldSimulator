#pragma once
#include <cstdint>
#include <map>
#include "log.h"

typedef uint64_t Token;

template<typename T>
class Registration
{
public:
    Registration() : mNextToken(1) {}

    Token GetNewToken(const std::string &name)
    {
        if (mTokens.find(name) != mTokens.end()) {
            Log(LogLevel::Error, "Name '%s' is already registered", name);
            throw std::invalid_argument("Name already registered");
        }

        Token token = mNextToken++;
        mTokens[name] = token;
        return token;
    }

    Token GetToken(const std::string &name) const
    {
        return mTokens.at(name);
    }

    std::string GetName(Token token) const
    {
        return mNames.at(token);
    }

    static T * GetInstance()
    {
        return &mInstance;
    }
private:
    Token mNextToken;
    std::map<std::string, Token> mTokens;
    std::map<Token, std::string> mNames;

    static T mInstance;
};

template<typename T>
T Registration<T>::mInstance;

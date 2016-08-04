#pragma once
#include <cstdint>
#include <map>

typedef uint64_t Token;

template<typename Type>
class Registration
{
public:
    Registration() : mNextToken(1) {}

    Token GetNewToken(const std::string &name)
    {
        Token token = mNextToken++;
        mTokens[name] = token;
        return token;
    }

    Token GetToken(const std::string &name) const
    {
        return mTokens[name];
    }

    std::string GetName(Token token) const
    {
        return mNames[token];
    }

    static Type * GetInstance()
    {
        return &mInstance;
    }
private:
    Token mNextToken;
    std::map<std::string, Token> mTokens;
    std::map<Token, std::string> mNames;

    static Type mInstance;
};
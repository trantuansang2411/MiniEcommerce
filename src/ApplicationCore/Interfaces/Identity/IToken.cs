
using ApplicationCore.Entities;

namespace ApplicationCore.Interfaces;
public interface IToken
    {
        string CreateAccessToken(User user);
    }

namespace ApplicationCore.Interfaces;

public interface IHashPassword
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

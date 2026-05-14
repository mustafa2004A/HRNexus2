namespace HRNexus.Business.Interfaces;

public interface IPasswordHashingService
{
    string HashPassword(string password);
}

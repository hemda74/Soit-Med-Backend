using System.Threading.Tasks;

namespace SoitMed.Services
{
    public interface IVerificationCodeService
    {
        Task<string> GenerateVerificationCodeAsync(string email);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task<bool> IsCodeValidAsync(string email, string code);
        Task RemoveCodeAsync(string email);
    }
}

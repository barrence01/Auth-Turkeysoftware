using Auth_Turkeysoftware.Test.Repositories.Models;

namespace Auth_Turkeysoftware.Test.Repositories
{
    public interface ITestDataRepository
    {
        Task AddData();
        Task<List<TestDataModel>> ReadData();
    }
}

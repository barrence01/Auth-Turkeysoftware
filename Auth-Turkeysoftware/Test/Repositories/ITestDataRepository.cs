using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Test.Repositories
{
    public interface ITestDataRepository
    {
        Task AddData();
        Task<List<TestDataModel>> ReadData();
    }
}

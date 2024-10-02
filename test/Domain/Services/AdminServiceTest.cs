using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Services;
using minimal_api.Infra.Database;

namespace test.Domain.Services

{
    [TestClass]
    public class AdminServiceTest
    {
        private DatabaseContext CreateDatabaseContext()
        {

            var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            return new DatabaseContext((IConfiguration)options);
        }
        [TestMethod]
        public void TestCreateAdmin()
        {
            // Arrange - Todas variaveis para validações
            var adm = new LoginDTO
            {
                Email = "test@test.com",
                Senha = "123456"
            };

            var context = CreateDatabaseContext();
            var service = new AdminService(context);
            // Act
            var result = service.Login(adm);

            // Assert - Validação dos dados
            Assert.IsNotNull(result);
        }
    }
}
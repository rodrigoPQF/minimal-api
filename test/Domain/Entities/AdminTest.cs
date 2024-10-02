using minimal_api.Domain.Entities;

namespace test.Domain.Entities

{
    [TestClass]
    public class AdminTest
    {
        [TestMethod]
        public void TestGetSetProps()
        {
            // Arrange - Todas variaveis para validações
            var adm = new Admin
            {
                // Act - Açoes que vai executar
                Id = 1,
                Email = "test@test.com",
                Senha = "123456",
                Perfil = "Admin"
            };
            // Assert - Validação dos dados
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("test@test.com", adm.Email);
            Assert.AreEqual("123456", adm.Senha);
            Assert.AreEqual("Admin", adm.Perfil);

        }
    }
}
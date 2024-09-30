using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infra.Database
{
    public class DatabaseContext : DbContext
    {

        private readonly IConfiguration _configuration;
        public DatabaseContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DbSet<Admin> Admins { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().HasData(
                new Admin { Id = 1, Email = "adm@test.com", Senha = "123456", Perfil = "admin" }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                var stringConnection = _configuration.GetConnectionString("postgres")?.ToString();

                if (!string.IsNullOrEmpty(stringConnection))
                {
                    optionsBuilder.UseNpgsql(stringConnection);

                }
            }
        }
    }
}
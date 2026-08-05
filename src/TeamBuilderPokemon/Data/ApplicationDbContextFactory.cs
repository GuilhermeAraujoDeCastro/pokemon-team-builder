using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace TeamBuilderPokemon.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connection=Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if(string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Defina ConnectionStrings__DefaultConnection explicitamente para o banco local de design/teste antes de usar dotnet ef.");
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
    }
}

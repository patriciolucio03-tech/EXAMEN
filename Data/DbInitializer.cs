using BCrypt.Net;
using Microsoft.EntityFrameworkCore;


namespace PEA.Data;


public static class DbInitializer
{
    public static async Task SeedAsync(PayrollDbContext db)
    {
        await db.Database.MigrateAsync();


        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
            new Models.Department { DeptNo = "d001", DeptName = "Gerencia" },
            new Models.Department { DeptNo = "d002", DeptName = "RRHH" },
            new Models.Department { DeptNo = "d003", DeptName = "Tecnología" }
            );
        }


        if (!db.Users.Any())
        {
            db.Users.Add(new Models.User
            {
                Usuario = "admin",
                Clave = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                Rol = "Admin"
            });
        }


        await db.SaveChangesAsync();
    }
}
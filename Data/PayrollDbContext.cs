using Microsoft.EntityFrameworkCore;
using PEA.Models;

namespace PEA.Data;


public class PayrollDbContext : DbContext
{
    private readonly IHttpContextAccessor _http;


    public PayrollDbContext(DbContextOptions<PayrollDbContext> options, IHttpContextAccessor http)
    : base(options) => _http = http;


    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DeptEmp> DeptEmps => Set<DeptEmp>();
    public DbSet<DeptManager> DeptManagers => Set<DeptManager>();
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<Salary> Salaries => Set<Salary>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Log_AuditoriaSalarios> Log_AuditoriaSalarios => Set<Log_AuditoriaSalarios>();


    protected override void OnModelCreating(ModelBuilder mb)
    {
        
        mb.Entity<Employee>().HasIndex(e => e.EmpNo).IsUnique();
        mb.Entity<Employee>().HasIndex(e => e.CI).IsUnique();
        mb.Entity<Employee>().HasIndex(e => e.Correo).IsUnique();


        
        mb.Entity<Department>().HasKey(d => d.DeptNo);
        mb.Entity<Department>().HasIndex(d => d.DeptName).IsUnique();


        
        mb.Entity<DeptEmp>().HasKey(x => new { x.EmpNo, x.DeptNo, x.FromDate });
        mb.Entity<DeptEmp>()
        .HasOne(x => x.Employee).WithMany(e => e.DeptEmps).HasForeignKey(x => x.EmpNo);
        mb.Entity<DeptEmp>()
        .HasOne(x => x.Department).WithMany(d => d.DeptEmps).HasForeignKey(x => x.DeptNo);


        
        mb.Entity<DeptManager>().HasKey(x => new { x.DeptNo, x.FromDate });
        mb.Entity<DeptManager>()
        .HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmpNo);
        mb.Entity<DeptManager>()
        .HasOne(x => x.Department).WithMany(d => d.DeptManagers).HasForeignKey(x => x.DeptNo);


        
        mb.Entity<Title>().HasKey(x => new { x.EmpNo, x.TitleName, x.FromDate });
        mb.Entity<Title>()
        .HasOne(x => x.Employee).WithMany(e => e.Titles).HasForeignKey(x => x.EmpNo);


        
        mb.Entity<Salary>().HasKey(x => new { x.EmpNo, x.FromDate });
        mb.Entity<Salary>()
        .HasOne(x => x.Employee).WithMany(e => e.Salaries).HasForeignKey(x => x.EmpNo);


        
        mb.Entity<User>().HasIndex(u => u.Usuario).IsUnique();

        
        mb.Entity<Log_AuditoriaSalarios>().ToTable("Log_AuditoriaSalarios");
        mb.Entity<Log_AuditoriaSalarios>().HasKey(l => l.Id);
        mb.Entity<Log_AuditoriaSalarios>().Property(l => l.Id).HasColumnName("LogId"); // <—
        mb.Entity<Log_AuditoriaSalarios>().Property(l => l.Salario).HasColumnName("Salario");
    }
   
    
    private void SetDbSessionUser(string usuario)
    {
        
        Database.ExecuteSqlInterpolated(
            $"EXEC sys.sp_set_session_context @key=N'AppUser', @value={usuario};");
    }

    public override int SaveChanges()
    {
        var usuario = _http.HttpContext?.User?.Identity?.IsAuthenticated == true
            ? _http.HttpContext!.User.Identity!.Name!
            : "sistema";
        SetDbSessionUser(usuario);

        
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var usuario = _http.HttpContext?.User?.Identity?.IsAuthenticated == true
            ? _http.HttpContext!.User.Identity!.Name!
            : "sistema";
        SetDbSessionUser(usuario);

        
        return base.SaveChangesAsync(cancellationToken);
    }

}
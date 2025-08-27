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
        // Employees
        mb.Entity<Employee>().HasIndex(e => e.EmpNo).IsUnique();
        mb.Entity<Employee>().HasIndex(e => e.CI).IsUnique();
        mb.Entity<Employee>().HasIndex(e => e.Correo).IsUnique();


        // Departments
        mb.Entity<Department>().HasKey(d => d.DeptNo);
        mb.Entity<Department>().HasIndex(d => d.DeptName).IsUnique();


        // DeptEmp
        mb.Entity<DeptEmp>().HasKey(x => new { x.EmpNo, x.DeptNo, x.FromDate });
        mb.Entity<DeptEmp>()
        .HasOne(x => x.Employee).WithMany(e => e.DeptEmps).HasForeignKey(x => x.EmpNo);
        mb.Entity<DeptEmp>()
        .HasOne(x => x.Department).WithMany(d => d.DeptEmps).HasForeignKey(x => x.DeptNo);


        // DeptManager
        mb.Entity<DeptManager>().HasKey(x => new { x.DeptNo, x.FromDate });
        mb.Entity<DeptManager>()
        .HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmpNo);
        mb.Entity<DeptManager>()
        .HasOne(x => x.Department).WithMany(d => d.DeptManagers).HasForeignKey(x => x.DeptNo);


        // Title
        mb.Entity<Title>().HasKey(x => new { x.EmpNo, x.TitleName, x.FromDate });
        mb.Entity<Title>()
        .HasOne(x => x.Employee).WithMany(e => e.Titles).HasForeignKey(x => x.EmpNo);


        // Salary
        mb.Entity<Salary>().HasKey(x => new { x.EmpNo, x.FromDate });
        mb.Entity<Salary>()
        .HasOne(x => x.Employee).WithMany(e => e.Salaries).HasForeignKey(x => x.EmpNo);


        // Users
        mb.Entity<User>().HasIndex(u => u.Usuario).IsUnique();

        // Log_AuditoriaSalarios  Id -> LogId
        mb.Entity<Log_AuditoriaSalarios>().ToTable("Log_AuditoriaSalarios");
        mb.Entity<Log_AuditoriaSalarios>().HasKey(l => l.Id);
        mb.Entity<Log_AuditoriaSalarios>().Property(l => l.Id).HasColumnName("LogId"); // <—
        mb.Entity<Log_AuditoriaSalarios>().Property(l => l.Salario).HasColumnName("Salario");
    }
    public override int SaveChanges()
    {
        AuditarSalarios();
        return base.SaveChanges();
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AuditarSalarios();
        return await base.SaveChangesAsync(cancellationToken);
    }


    private void AuditarSalarios()
    {
        var usuario = _http.HttpContext?.User?.Identity?.IsAuthenticated == true
        ? _http.HttpContext!.User.Identity!.Name!
        : "sistema";


        var entradas = ChangeTracker.Entries<Salary>()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
        .ToList();


        foreach (var e in entradas)
        {
            Log_AuditoriaSalarios.Add(new Log_AuditoriaSalarios
            {
                Usuario = usuario,
                FechaActualizacion = DateTime.UtcNow,
                DetalleCambio = e.State == EntityState.Added ? "Alta de salario" : "Cambio de salario",
                Salario = e.Entity.Amount,
                EmpNo = e.Entity.EmpNo
            });
        }
    }
}
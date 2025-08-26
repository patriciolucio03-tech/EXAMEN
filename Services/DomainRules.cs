using Microsoft.EntityFrameworkCore;
using PEA.Data;
using PEA.Models;


namespace PEA.Services;


public class DomainRules
{
    private readonly PayrollDbContext _db;
    public DomainRules(PayrollDbContext db) => _db = db;


    public async Task<bool> SalaryOverlapsAsync(Salary s)
    {
        return await _db.Salaries
        .Where(x => x.EmpNo == s.EmpNo && !(x.ToDate < s.FromDate || (s.ToDate != null && s.ToDate < x.FromDate)))
        .AnyAsync();
    }


    public async Task<bool> DeptEmpOverlapsAsync(DeptEmp de)
    {
        return await _db.DeptEmps
        .Where(x => x.EmpNo == de.EmpNo && !(x.ToDate < de.FromDate || (de.ToDate != null && de.ToDate < x.FromDate)))
        .AnyAsync();
    }


    public async Task<bool> ManagerOverlapsAsync(DeptManager dm)
    {
        return await _db.DeptManagers
        .Where(x => x.DeptNo == dm.DeptNo && !(x.ToDate < dm.FromDate || (dm.ToDate != null && dm.ToDate < x.FromDate)))
        .AnyAsync();
    }
}
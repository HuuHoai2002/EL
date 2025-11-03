using Microsoft.EntityFrameworkCore;

namespace KHCN.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
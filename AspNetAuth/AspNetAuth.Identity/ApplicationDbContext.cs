using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetAuth.Identity;

// DbContext继承IdentityDbContext, 里面自带用户、角色等表，可通过泛型指定自定义的用户、角色类等
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 必须调用基类的OnModelCreating方法以创建默认的用户、角色等表
        base.OnModelCreating(builder);
    }
}

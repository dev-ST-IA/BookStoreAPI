using BulkyBookBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace BulkyBookBackEnd.Data
{
    public class BookDbContext:DbContext
    {
        public BookDbContext(DbContextOptions options):base(options)
        {
        }


        //DbSet
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Book> Books { get; set; } = default!;
        public DbSet<FeedBack> FeedBacks { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;

        public DbSet<Credentials> Credentials { get; set; } = default!;

        public DbSet<Cart> Carts { get; set; } = default!;

        public DbSet<CartProduct> CartProducts { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.EmailAddress, e.Role })
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.EmailAddress })
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.UserName })
                .IsUnique(true);

            modelBuilder.Entity<User>().Property(b => b.Feedbacks)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<IDictionary<Book, FeedBack>>(v));

            modelBuilder.Entity<Category>()
                .HasIndex(e => new { e.Name })
                .IsUnique(true);

            modelBuilder.Entity<Book>()
                .HasIndex(e=>new {e.Title,e.Publisher})
                .IsUnique(true);

            //modelBuilder.Entity<Cart>()
            //    .HasMany(e => e.Products)
            //    .WithOne()
            //    .OnDelete(DeleteBehavior.NoAction);
            //modelBuilder.Entity<Order>()
            //    .HasOne(e => e.Cart)
            //    .WithOne()
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity(typeof(Order))
            //    .HasOne(typeof(Cart),"Cart")
            //    .WithOne()
            //    .HasForeignKey("CartId")
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity(typeof(Cart))
            //    .HasOne(typeof(Order), "Order")
            //    .WithOne()
            //    .HasForeignKey("OrderId")
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Cart>()
            //    .HasMany(c => c.Products)
            //    .WithOne()
            //    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>()
                .HasMany(c => c.CartProducts)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Credentials>()
            //    .HasIndex(e => new { e.User.Id })
            //    .IsUnique(true);
        }


    }
}

using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Req.Bodies
{
    public class CreateBook : IValidatableObject
    {
        [Required]
        public string? CategoryName { get; set; } = default!;

        [Required]
        public string AuthorName { get; set; }= default!;

        [Required]
        public string? Title { get; set; } = default!;

        [Required]
        public string? Description { get; set; } = default!;

        [Required]
        public string? Publisher { get; set; } = default!;

        [Required]
        public float? Price { get; set; } = default!;

        [Required]
        public float? Cost { get; set; } = default!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Price < Cost)
            {
                yield return new ValidationResult("Invalid Pricing");
            }
        }

        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Must be greater than or equal to {0}")]
        public int? Units { get; set; } = 0;

        public IFormFile? Image { get; set; }

        public async static Task<Book> CreateAsync(BookDbContext db, CreateBook body)
        {
            try
            {
                var book = await db.Books.FirstOrDefaultAsync(b => b.Title == body.Title && b.Publisher == body.Publisher);
                if (book != null)
                {
                    return null;
                }
                var category = await db.Categories.FirstOrDefaultAsync(b => b.Name == body.CategoryName);
                if (category == default(Category))
                {
                    category = new Category()
                    {
                        Name = body.CategoryName
                    };
                    await db.Categories.AddAsync(category);
                    await db.SaveChangesAsync();

                }
                var author = await db.Author.FirstOrDefaultAsync(b => b.Name == body.AuthorName);
                if (author == default(Author))
                {
                    author =  new Author()
                    {
                        Name = body.AuthorName
                    };
                    await db.Author.AddAsync(author);
                    await db.SaveChangesAsync();

                }
                var newBook = new Book()
                {
                    Title = body.Title,
                    Publisher = body.Publisher,
                    Description = body.Description,
                    Units = (int)body.Units,
                    Cost = (float)body.Cost,
                    Price = (float)body.Price,
                    Category = category,
                    Author=author

                };
                await db.AddAsync(newBook);
                await db.SaveChangesAsync();
                if (body.Image != null)
                {
                    var file = body.Image;
                    var fileName = file.FileName.Trim('"');
                    var fileExt = Path.GetExtension(fileName);  
                    var newFileName = $"book-{newBook.Id}";
                    var newFileNameExt = newFileName+fileExt;
                    var filePath = Path.GetTempPath();
                    var abosolutePath = Path.Combine(filePath, newFileNameExt);
                    using (var stream = File.Create(abosolutePath))
                    {
                        stream.Flush();
                        await file.CopyToAsync(stream);
                        stream.Position = 0;
                        stream.Close();
                        var cloudinary = new CloudinaryClass();
                        var imageUrl = await cloudinary.BookImageUpload(
                                            abosolutePath,
                                            newFileName
                                        );
                        newBook.ImageUrl = imageUrl;
                        newBook.ImageName = newFileName;
                        File.Delete(abosolutePath);
                    }
                }
                db.Entry(newBook).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return newBook;
            }
            catch (Exception)
            {
                return null;
            }


        }
    }

    
}

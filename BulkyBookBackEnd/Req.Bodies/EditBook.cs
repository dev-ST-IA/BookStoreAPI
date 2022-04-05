using BulkyBookBackEnd.Data;
using BulkyBookBackEnd.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookBackEnd.Req.Bodies
{
    public class EditBook
    {
        private readonly BookDbContext db;
        [Required]
        public int Id { get; set; }

        public string? CategoryName { get; set; } = default!;

        public string? Title { get; set; } = default!;

        public string? Description { get; set; } = default!;

        public float? Price { get; set; } = default!;

        public float? Cost { get; set; } = default!;

        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Must be greater than or equal to {0}")]
        public int? Units { get; set; } = 0;

        public IFormFile? Image { get; set; } = null;

        public string ImageName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; } = string.Empty;
        public string? Publisher { get; set; } = string.Empty;

        public bool isBook;

        public EditBook(int bookId)
        {
            this.Id = bookId;
            isBook = BookExists(bookId);
        }

        public async static Task<Book> EditAsync(BookDbContext db,EditBook editables)
        {
            try
            {
                var book = await db.Books.FindAsync(editables.Id);
                if (book == null)
                {
                    return null;
                }
                book.Cost = (float)editables.Cost;
                book.Price = (float)editables.Price;
                book.Title = editables.Title;
                book.Description = editables.Description;
                book.Units = (int)editables.Units;
                book.Publisher = editables.Publisher;
                var category = await db.Categories.FindAsync(editables.CategoryName);
                if (category == null)
                {
                    return null;
                }
                book.Category = category;
                if(string.IsNullOrEmpty(editables.ImageUrl))
                {
                    book.ImageName = editables.ImageName;
                    if (editables.Image.Length > 0)
                    {
                        var file = editables.Image;
                        var fileName = $"book-{book.Id}";
                        var filePath = Path.GetTempFileName();
                        using(var stream = File.Create(filePath))
                        {
                            await file.CopyToAsync(stream); 
                            var cloudinary = new CloudinaryClass();
                            var filePathFromServer = $"{filePath}/{fileName}";
                            var imageUrl = await cloudinary.BookImageUpload(
                                                filePathFromServer,
                                                fileName
                                            );
                            book.ImageUrl = imageUrl;
                            book.ImageName = fileName;
                            File.Delete(filePathFromServer);
                        }
                    }
                }
                db.Entry(book).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return book;
            }
            catch (Exception)
            {
                return null;
            }


        }

        private bool BookExists(int id)
        {
            return db.Books.Any(e => e.Id == id);
        }

    }
}

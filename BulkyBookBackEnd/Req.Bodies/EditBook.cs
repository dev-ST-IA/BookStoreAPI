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

        public string? AuthorName { get; set; } = default!;

        public string? Title { get; set; } = default!;

        public string? Description { get; set; } = default!;

        public float? Price { get; set; } = default!;

        public float? Cost { get; set; } = default!;

        [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Must be greater than or equal to {0}")]
        public int? Units { get; set; } = 0;

        public IFormFile? Image { get; set; } = null;

        public string? Publisher { get; set; } = string.Empty;

        public bool isBook;

        public EditBook(int bookId)
        {
            this.Id = bookId;
            isBook = BookExists(bookId);
        }

        public EditBook()
        {

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
                if (editables.Cost != null)
                {
                    book.Cost = (float)editables.Cost;
                }
                if(editables.Price != null)
                {
                    book.Price = (float)editables.Price;
                }
                if(editables.Title != null)
                {
                    book.Title = editables.Title;
                }
                if (editables.Description != null)
                {
                    book.Description = editables.Description;
                }
                if (editables.Units != null)
                {
                    book.Units = (int)editables.Units;
                }
                if (editables.Publisher != null)
                {
                    book.Publisher = editables.Publisher;
                }
                var category = await db.Categories.Where(i=>i.Name==editables.CategoryName).FirstOrDefaultAsync();
                if (category == null&&editables.CategoryName!=null)
                {
                    category = new Category
                    {
                        Name = editables.CategoryName
                    };
                    await db.Categories.AddAsync(category);
                    category.Books.Add(book);
                    db.Entry(category).State = EntityState.Modified;
                }
                var author = await db.Author.Where(t=>t.Name==editables.AuthorName).FirstOrDefaultAsync();
                if (author == null&&editables.AuthorName!=null)
                {
                    author = new Author
                    {
                        Name = editables.AuthorName
                    };
                    await db.Author.AddAsync(author);
                    author.Books.Add(book);
                    db.Entry(category).State = EntityState.Modified;
                }
                if(editables.CategoryName != null)
                {
                    book.Category = category;
                }
                if(editables.AuthorName != null)
                {
                    book.Author = author;
                }
                if(editables.Image!= null)
                {
                    if (editables.Image.Length > 0)
                    {
                        var file = editables.Image;
                        var fileName = file.FileName.Trim('"');
                        var fileExt = Path.GetExtension(fileName);
                        var newFileName = book.ImageName;
                        var newFileNameExt = newFileName + fileExt;
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
                            book.ImageUrl = imageUrl;
                            File.Delete(abosolutePath);
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

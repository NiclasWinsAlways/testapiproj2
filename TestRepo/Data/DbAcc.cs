using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TestRepo.DTO;
using TestRepo.Interface;



namespace TestRepo.Data
{
    public class DbAccess
    {
        private readonly Dbcontext _dbContext;
        //FOR PASSWORD HASHING
        //private readonly PasswordHasher<Account> _passwordHasher = new PasswordHasher<Account>();


        public DbAccess(Dbcontext dbContext)
        {
            this._dbContext = dbContext;
        }


        public class BookWithProgress
        {
            public BookProgress Progress { get; set; }
            public Book Book { get; set; }
        }
        #region
        public List<Book> GetAllBooks()
        {
            return _dbContext.Books.ToList();
        }
        public void AddBook(Book book)
        {
            // Your logic to add a book to the database
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();
        }
        public bool DeleteBook(int bookId)
        {
            var book = _dbContext.Books.Find(bookId);
            if (book != null)
            {
                _dbContext.Books.Remove(book);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public List<BookWithProgress> GetBookList(int accountId)
        {
            var bookList = _dbContext.BooksProgress
                .Where(b => b.AccountId == accountId)
                .Join(_dbContext.Books,
                      progress => progress.BookId,
                      book => book.Id,
                      (progress, book) => new BookWithProgress { Progress = progress, Book = book })
                .ToList();

            return bookList;
        }
        public Book GetBookById(int bookId)
        {
            // Correcting _context to _dbContext
            return _dbContext.Books.FirstOrDefault(b => b.Id == bookId);
        }
        public void AddBookProgress(BookProgress bookProgress)
        {
            try
            {
                _dbContext.BooksProgress.Add(bookProgress);
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Log the exception and handle it appropriately
                throw new ApplicationException("Failed to add book progress, possibly because the book does not exist.", ex);
            }
        }
        //loan book
        public bool LoanBook(int bookId, DateTime dueDate)
        {
            var book = _dbContext.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                return false; // Book not found
            }

            if (book.IsLoaned)
            {
                return false; // Book is already loaned out
            }

            book.IsLoaned = true;
            book.DueDate = dueDate;
            _dbContext.Entry(book).State = EntityState.Modified;
            _dbContext.SaveChanges();

            return true; // Loan operation successful
        }

        public Book GetBookByTitle(string title)
        {
            // Assuming the title is a string property of the Book class
            return _dbContext.Books.FirstOrDefault(b => b.Title == title);
        }
        public BookProgress GetBookProgressById(int progressId)
        {
            // Using .Include() to fetch related entities
            return _dbContext.BooksProgress
                .Include(bp => bp.Book)
                .Include(bp => bp.Account)
                .FirstOrDefault(bp => bp.Id == progressId);
        }
        #endregion
        #region
        public void UpdPageCount(updpage newpage)
        {
            // Update read page count for user
            var book = _dbContext.volProgress.Where(b => b.BookId == newpage.BookId && b.volId == newpage.volId && b.AccountId == newpage.AccountId).SingleOrDefault();
            if (book != null)
            {
                book.pagesRead = newpage.PagesRead;
                _dbContext.SaveChanges();
            }
        }
        public void UpdTotalPages(int BookId, int newPageCount, int volId)
        {
            // Update read page count for user
            var Book = _dbContext.Vols.Where(b => b.BookId == BookId && b.VolumeId == volId).SingleOrDefault();
            if (Book != null)
            {
                Book.totalPages = newPageCount;
                _dbContext.SaveChanges();
            }
        }

        public void UpdVolCount(int bookProgressId, int newVolCount)
        {
            // Update read volume count for user
            var bookProgress = _dbContext.BooksProgress.Find(bookProgressId);
            if (bookProgress != null)
            {
                bookProgress.volumesRead = newVolCount;
                _dbContext.SaveChanges();
            }
        }
        #endregion
        #region
        public List<Volume> GetBookVol(int bookid)
        {
            var bookfound = _dbContext.Books.Find(bookid);
            if (bookfound == null)
            {
                return null;
            }
            else
            {
                var volList = _dbContext.Vols
                .Where(b => b.BookId == bookid)
                .ToList();

                return volList;
            }
        }
        public Volume GetNewVol(Ivol vol)
        {
            var dbvol = _dbContext.Vols.Where(b => b.BookId == vol.BookId && b.VolNumber == vol.VolNumber).SingleOrDefault();
            return dbvol;
        }
        public void AddBook(int accountId, Book book)
        {
            // Check if the account exists
            var account = _dbContext.Acc.Find(accountId);
            if (account == null)
            {
                throw new ArgumentException("Account does not exist.", nameof(accountId));
            }

            // Check if a book with the same ID already exists (assuming ID is auto-generated, this might not be necessary)
            if (book.Id != 0 && _dbContext.Books.Any(b => b.Id == book.Id))
            {
                throw new ArgumentException("A book with the same ID already exists.");
            }

            // Add the new book to the database
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();

            // Optionally, create a default BookProgress record if needed
            var bookProgress = new BookProgress
            {
                BookId = book.Id,
                AccountId = accountId,
                volumesRead = 0
            };
            _dbContext.BooksProgress.Add(bookProgress);
            _dbContext.SaveChanges();
        }

        public void LibDelBook(int accountId, int bookId)
        {
            // Find the Book
            var bookProgress = _dbContext.BooksProgress
                .SingleOrDefault(bp => bp.AccountId == accountId && bp.BookId == bookId);

            // If the book exists, remove it from the database
            if (bookProgress != null)
            {
                _dbContext.BooksProgress.Remove(bookProgress);
                _dbContext.SaveChanges();
            }
        }
        public VolProgress GetVolProgress(int accountId, int volId)
        {
            var volProgress = _dbContext.volProgress
                .SingleOrDefault(vp => vp.volId == volId && vp.AccountId == accountId);

            return volProgress;
        }
        public void CreateVolProgress(CVolProgress volProgress)
        {
            int bookid = volProgress.bookId;
            var account = _dbContext.Acc.Find(volProgress.accountId);

            if (account != null)
            {
                // Check if a VolProgress record with the given accountId, bookId, and volId already exists
                var existingVolProgress = _dbContext.volProgress
                    .SingleOrDefault(vp => vp.AccountId == volProgress.accountId && vp.BookId == bookid && vp.volId == volProgress.volId);

                // If it does not exist, create a new VolProgress record
                if (existingVolProgress == null)
                {
                    var newVolProgress = new VolProgress
                    {
                        BookId = bookid,
                        AccountId = volProgress.accountId,
                        volId = volProgress.volId,
                        pagesRead = volProgress.PagesRead,  // Set pagesRead to the value from volProgress
                    };

                    // Add the new VolProgress object to the volProgress DbSet
                    _dbContext.volProgress.Add(newVolProgress);

                    // Save the changes
                    _dbContext.SaveChanges();
                }
            }
        }


        public string CreateVol(int bookId, int volNumber)
        {
            var book = _dbContext.Books.Find(bookId);
            if (book == null)
            {
                return "Book doesn't exist";
            }

            var volume = new Volume
            {
                BookId = bookId,
                VolNumber = volNumber
            };

            _dbContext.Vols.Add(volume);
            _dbContext.SaveChanges();

            return "Volume created successfully";
        }
        #endregion


        //public string CreateVol(int BookId, int volNumber)
        //{
        //    var book = _dbContext.Books.Find(BookId);
        //    if (book == null)
        //    {
        //        return "Book doesn't exist";
        //    }

        //    var volume = new Volume
        //    {
        //        BookId = BookId,
        //        VolNumber = volNumber
        //    };
        //    _dbContext.Vols.Add(volume);
        //    _dbContext.SaveChanges();

        //    return "Volume created successfully";
        //}
        #region
        //password hash example
        //public object CheckLogin(string userName, string password)
        //{
        //    // Check if input matches what is in the database
        //    var account = _dbContext.Acc
        //        .SingleOrDefault(a => a.UserName == userName);

        //    if (account != null)
        //    {
        //        // Verify the password
        //        if (PasswordHasher.VerifyHashedPassword(account.PasswordHash, password + account.Salt))
        //        {
        //            // Change account "IsLoggedIn" to true
        //            account.IsLoggedin = true;  // Optionally track login status
        //            _dbContext.SaveChanges();

        //            // Return the AccountId and IsAdmin when login is successful
        //            return new { AccountId = account.Id, IsAdmin = account.IsAdmin };
        //        }
        //    }

        //    return null;
        //}

        public object CheckLogin(string userName, string password)
        {
            // Check if input matches what is in the database
            var account = _dbContext.Acc
                .SingleOrDefault(a => a.UserName == userName && a.Password == password);

            if (account != null)
            {
                // Change account "IsLoggedIn" to true
                account.IsLoggedin = true;
                _dbContext.SaveChanges();

                // Return the AccountId and IsAdmin when login is successful
                return new { AccountId = account.Id, IsAdmin = account.IsAdmin };
            }

            return null;
        }


        public Account GetAccInfo(int accountId)
        {
            // Retrieve the account for the user based on the accountId
            var account = _dbContext.Acc
                .Where(b => b.Id == accountId)
                .SingleOrDefault();

            return account;
        }

        //passwordhash example
        ////public void CreateAcc(Account newAccount)
        ////{
        ////    // Generate a random salt
        ////    newAccount.Salt = GenerateSalt();

        ////    // Hash the password
        ////    newAccount.PasswordHash = PasswordHasher.HashPassword(newAccount.Password + newAccount.Salt);

        ////    // Create the account
        ////    _dbContext.Acc.Add(newAccount);
        ////    _dbContext.SaveChanges();
        ////}

        //private string GenerateSalt()
        //{
        //    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        //    {
        //        byte[] saltBytes = new byte[16];
        //        rng.GetBytes(saltBytes);
        //        return Convert.ToBase64String(saltBytes);
        //    }
        //}
        public void CreateAcc(Account newAccount)
        {
            // Create an account with data from the user
            _dbContext.Acc.Add(newAccount);
            _dbContext.SaveChanges();
        }
        public void DeleteAcc(int accountId)
        {
            // find acc
            var account = _dbContext.Acc
                .Where(b => b.Id == accountId)
                .SingleOrDefault();

            // If exists remove it from the db
            if (account != null)
            {
                _dbContext.Acc.Remove(account);
                _dbContext.SaveChanges();
            }
        }
        public void ChangeEmail(int accountId, string newEmail)
        {
            // Find the account
            var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);

            // If exists, update
            if (account != null)
            {
                account.Email = newEmail;
                _dbContext.SaveChanges();
            }
        }

        //HASH ALL EXSISTING EXAMPLE
        //public void HashExistingPasswords()
        //{
        //    // Get all accounts with unhashed passwords
        //    var accounts = _dbContext.Acc.Where(a => string.IsNullOrEmpty(a.PasswordHash)).ToList();

        //    // Hash the passwords and update the database
        //    foreach (var account in accounts)
        //    {
        //        account.PasswordHash = PasswordHasher.HashPassword(account.Password);
        //        _dbContext.Acc.Update(account);
        //    }

        //    _dbContext.SaveChanges();
        //}

        //password hash example
        //public void ChangePassword(int accountId, string newPassword)
        //{
        //    // Find the account
        //    var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);

        //    // If exists, update
        //    if (account != null)
        //    {
        //        // Generate a new salt
        //        account.Salt = GenerateSalt();

        //        // Hash the new password
        //        account.PasswordHash = PasswordHasher.HashPassword(newPassword + account.Salt);

        //        _dbContext.SaveChanges();
        //    }
        //}
        public void ChangePassword(int accountId, string newPassword)
        {
            // Find the account
            var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);

            // If exists, update
            if (account != null)
            {
                account.Password = newPassword;
                _dbContext.SaveChanges();
            }
        }
        public void ChangeUsername(int accountId, string newUsername)
        {
            // Find the account
            var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);

            // If exists, update
            if (account != null)
            {
                account.UserName = newUsername;
                _dbContext.SaveChanges();
            }

        }
        public Account GetAccountById(int accountId)
        {
            // Assuming you have a DbContext object to interact with your database
            return _dbContext.Acc.FirstOrDefault(a => a.Id == accountId);
        }
        public void UpdateAccount(Account account)
        {
            // Update the existing account entity and save changes
            _dbContext.Acc.Update(account);
            _dbContext.SaveChanges();
        }

        public List<Account> GetAllAccounts()
        {
            return _dbContext.Acc.ToList();
        }
        #endregion
    }
}

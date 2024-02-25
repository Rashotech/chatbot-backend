using System;
using System.Threading.Tasks;
using ChatBot.Database;
using ChatBot.Repositories.Interfaces;

namespace ChatBot.Repositories
{
	public class UnitOfWork: IUnitOfWork
    {
        private readonly BankDbContext _context;

        public UnitOfWork(BankDbContext context)
        {
           _context = context;
           Customers = new CustomerRepository(_context);
           Accounts = new AccountRepository(_context);
           Transactions = new TransactionRepository(_context);
        }

        public ICustomerRepository Customers { get; private set; }

        public IAccountRepository Accounts { get; private set; }

        public ITransactionRepository Transactions { get; private set; }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}


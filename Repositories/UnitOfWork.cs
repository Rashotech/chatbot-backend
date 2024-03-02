﻿using ChatBot.Database;
using ChatBot.Database.Models;
using ChatBot.Repositories.Interfaces;
using ChatBot.Services.Interfaces;
using System.Threading.Tasks;

namespace ChatBot.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankDbContext _context;

        public UnitOfWork(BankDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(_context);
            Accounts = new AccountRepository(_context);
            Transactions = new TransactionRepository(_context);
            //Complaints = new ComplaintRepository(_context);
        }

        public ICustomerRepository Customers { get; private set; }

        public IAccountRepository Accounts { get; private set; }

        public ITransactionRepository Transactions { get; private set; }
        //public IComplaintRepository Complaints { get; private set; }

        public IComplaintService Complaints { get; private set; }

        public UnitOfWork(ICustomerRepository customers, IAccountRepository accounts, IComplaintService complaints, ITransactionRepository transactions)
        {
            Customers = customers;
            Accounts = accounts;
            Complaints = complaints;  // Set the Complaints property
            Transactions = transactions;
        }


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

        public async Task AddComplaintAsync(Complaint complaint)
        {
            await _context.Complaints.AddAsync(complaint);
        }
    }
}




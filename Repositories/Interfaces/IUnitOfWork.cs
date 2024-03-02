using ChatBot.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace ChatBot.Repositories.Interfaces
{
	public interface IUnitOfWork: IDisposable
    {
        ICustomerRepository Customers { get; }
        IAccountRepository Accounts { get; }
        IComplaintService Complaints { get; }
        ITransactionRepository Transactions { get; }

        Task CommitAsync();
        void Commit();
    }
}


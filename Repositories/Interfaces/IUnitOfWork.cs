using System;
using System.Threading.Tasks;

namespace ChatBot.Repositories.Interfaces
{
	public interface IUnitOfWork: IDisposable
    {
        ICustomerRepository Customers { get; }
        IAccountRepository Accounts { get; }
        ITransactionRepository Transactions { get; }

        Task CommitAsync();
        void Commit();
    }
}


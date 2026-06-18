using Fubaza.Application.Core.Entities;
using Fubaza.Application.DTO.DTO;
using System.Linq.Expressions;

namespace Fubaza.Application.Core.Contracts.Services
{
	public interface IJobService
	{
		string Enqueue(Expression<Func<Task>> methodCall);

		Task PublishToFacebookAsyncV3(string Id);


        Task PublishToInstagramAsync(string Id);

        Task PublishToInstagramLoginAsync(string Id);
    }
}



using TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;
using TechSpherex.CleanArchitecture.Application.Features.Todos.Get;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Features.Todos.GetAll;

public sealed record GetAllTodosQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<TodoDetailResponse>>>;

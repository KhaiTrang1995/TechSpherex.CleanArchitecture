
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;

public interface ICommand : ICommand<Result>;

public interface ICommand<TResponse>;

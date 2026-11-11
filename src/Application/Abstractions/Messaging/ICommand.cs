
using System.Diagnostics.CodeAnalysis;
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;

public interface ICommand : ICommand<Result>;

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "Phantom type: ties a command to its response type so ICommandHandler<TCommand, TResponse> is checked at compile time.")]
public interface ICommand<TResponse>;

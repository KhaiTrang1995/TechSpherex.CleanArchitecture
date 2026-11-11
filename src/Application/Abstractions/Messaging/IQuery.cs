using System.Diagnostics.CodeAnalysis;
namespace TechSpherex.CleanArchitecture.Application.Abstractions.Messaging;

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "Phantom type: ties a query to its response type so IQueryHandler<TQuery, TResponse> is checked at compile time.")]
public interface IQuery<TResponse>;

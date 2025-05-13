using System;

namespace BLL.Exceptions
{
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException() : base() { }
        public BusinessLogicException(string message) : base(message) { }
        public BusinessLogicException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EntityNotFoundException : BusinessLogicException
    {
        public EntityNotFoundException(string entityName, int id)
            : base($"{entityName} with ID {id} was not found.") { }

        public EntityNotFoundException(string message) : base(message) { }
    }

    public class InvalidOperationBusinessException : BusinessLogicException
    {
        public InvalidOperationBusinessException(string message) : base(message) { }
    }

    public class ValidationException : BusinessLogicException
    {
        public ValidationException(string message) : base(message) { }
    }

    public class InsufficientStockException : BusinessLogicException
    {
        public InsufficientStockException(string productName, int requested, int available)
            : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}") { }
    }

    public class UnauthorizedOperationException : BusinessLogicException
    {
        public UnauthorizedOperationException(string message) : base(message) { }

        public UnauthorizedOperationException(string operation, string role)
            : base($"User with role '{role}' is not authorized to perform operation: {operation}") { }
    }
}
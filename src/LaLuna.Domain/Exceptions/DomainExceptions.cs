namespace LaLuna.Domain.Exceptions;

public class NotFoundException(string name, object key)
    : Exception($"Entity '{name}' with key '{key}' was not found.");

public class ValidationException(string message) : Exception(message);

public class InsufficientStockException(string productName, int requested, int available)
    : Exception($"Insufficient stock for '{productName}': requested {requested}, available {available}.");

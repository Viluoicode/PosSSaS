namespace PosSSaS.Application.Common.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string ingredient, decimal required, decimal available)
        : base($"Insufficient stock for ingredient '{ingredient}'. Required: {required}, Available: {available}.") { }
}

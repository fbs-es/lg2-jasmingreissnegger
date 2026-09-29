using System;

namespace Fbs.Lg2;

public abstract class SparkontoException : Exception
{
    protected SparkontoException(string message) : base(message) { }
    protected SparkontoException(string message, Exception inner) : base(message, inner) { }
}

namespace Emata.Exercise.LoansManagement.Contracts.Exceptions
{
    public class LoansManagementValueException : LoansManagementException
    {
        public LoansManagementValueException() { }

        public LoansManagementValueException(string message) : base(message) { }
    }
}

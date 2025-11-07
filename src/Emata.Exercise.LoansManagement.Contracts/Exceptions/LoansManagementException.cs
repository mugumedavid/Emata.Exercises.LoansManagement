namespace Emata.Exercise.LoansManagement.Contracts.Exceptions
{
    public class LoansManagementException : Exception
    {
        public LoansManagementException() { }

        public LoansManagementException(string message) : base(message) { }
    }
}

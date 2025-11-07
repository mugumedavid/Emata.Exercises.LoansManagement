namespace Emata.Exercise.LoansManagement.Contracts.Exceptions
{
    public class LoansManagementNotFoundException : LoansManagementException
    {
        public LoansManagementNotFoundException() { }

        public LoansManagementNotFoundException(string message) : base(message) { }
    }
}

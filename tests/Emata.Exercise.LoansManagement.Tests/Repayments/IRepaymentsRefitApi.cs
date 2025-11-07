using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emata.Exercise.LoansManagement.Contracts.Repayments;
using Emata.Exercise.LoansManagement.Contracts.Repayments.DTOs;
using Refit;

namespace Emata.Exercise.LoansManagement.Tests.Repayments;

public interface IRepaymentsRefitApi
{
    [Post("/repayments")]
    Task<ApiResponse<PaymentSummaryDTO>> AddPaymentAsync([Body] AddPaymentCommand payment);

    [Get("/repayments/loan/{loanId}")]
    Task<ApiResponse<List<PaymentSummaryDTO>>> GetRepaymentsByLoanAsync(Guid loanId);

    [Get("/repayments/{id}")]
    Task<ApiResponse<PaymentSummaryDTO>> GetRepaymentByIdAsync(Guid id);
}
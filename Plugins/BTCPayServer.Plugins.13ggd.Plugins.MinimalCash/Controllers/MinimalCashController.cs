using System;
using System.Threading.Tasks;
using BTCPayServer.Client.Models;
using BTCPayServer.Data;
using BTCPayServer.Payments;
using BTCPayServer.Services.Invoices;
using BTCPayServer.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTCPayServer.Plugins.13ggd.Plugins.MinimalCash
{
    [Route("stores/{storeId}/minimalcash")]
    [Authorize(Policy = Policies.CanModifyStoreSettings, AuthenticationSchemes = AuthenticationSchemes.Cookie)]
    public class MinimalCashController : Controller
    {
        private readonly InvoiceRepository _invoiceRepository;

        public MinimalCashController(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        [HttpPost("MarkAsPaid")]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> MarkAsPaid(string invoiceId, string storeId, string returnUrl)
        {
            if (Request.Headers["X-Requested-With"] != "MinimalCashHttpRequester")
                return BadRequest();

            var invoice = await _invoiceRepository.GetInvoice(invoiceId, true);
            if (invoice.StoreId != storeId || invoice.Status != InvoiceStatus.New)
                return Json(new { success = false, error = "Invoice not found or already paid" });

            var pmid = new PaymentMethodId("MINIMAL_CASH");
            var paymentData = new PaymentData
            {
                Id = Guid.NewGuid().ToString(),
                Created = DateTimeOffset.UtcNow,
                Status = PaymentStatus.Settled,
                Currency = invoice.Currency,
                InvoiceDataId = invoiceId,
                Amount = invoice.Price,
                PaymentMethodId = pmid.ToString()
            };

            var payment = await _invoiceRepository.AddPayment(paymentData);
            if (payment != null) 
                await _invoiceRepository.MarkInvoiceStatus(invoice.Id, InvoiceStatus.Settled);

            return Json(new { success = true, status = invoice.Status.ToString() });
        }
    }
}

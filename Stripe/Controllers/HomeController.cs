using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Stripe;
using Stripe.Checkout;

namespace StripeIntegration.Controllers
{
    public class HomeController : Controller
    {
        private static string StripeSecretKey = "sk_test_51Owfp3SE9u5I1bSOo0Rz2i93YLXyQiV5c6aiLeP8drKtqkIdFO42I3hKFvSeoOXmQQhjTzY2pleLi4mEASJ0zlnJ00chXoREMJ";
        private static string StripePublishableKey = "pk_test_51Owfp3SE9u5I1bSOLkQF58GdxA6Nocttsy8f5C0FVGn0uunb7mJBa4YsyFV5fFjKURRyhAhlPUbgTTbx8Btuyz5j00zjzLuyEv";
        private static string LatestPaymentIntentId;

        public ActionResult Index()
        {
            ViewBag.StripePublishableKey = StripePublishableKey;
            return View();
        }

        public async Task<JsonResult> CreateCheckoutSession()
        {
            try
            {
                StripeConfiguration.ApiKey = StripeSecretKey;

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = 1000, // Amount in cents
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Sample Product",
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = Url.Action("Success", "Home", null, Request.Url.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = Url.Action("Cancel", "Home", null, Request.Url.Scheme),
                    AutomaticTax = new SessionAutomaticTaxOptions
                    {
                        Enabled = true,
                    },
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return Json(new { id = session.Id }, JsonRequestBehavior.AllowGet);
            }
            catch (StripeException ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Success(string session_id)
        {
            try
            {
                StripeConfiguration.ApiKey = StripeSecretKey;

                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(session_id);

                var invoiceService = new InvoiceService();
                var invoice = await invoiceService.GetAsync(session.InvoiceId);

                var taxAmount = invoice.Tax.HasValue ? invoice.Tax.Value / 100.0m : 0m;
                var totalAmount = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100.0m : 0m;
                var netAmount = totalAmount - taxAmount;

                var taxDetails = new
                {
                    Amount = totalAmount,
                    Tax = taxAmount,
                    NetAmount = netAmount
                };

                ViewBag.TaxDetails = taxDetails;
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "Home", "Success"));
            }
        }

        public ActionResult Cancel()
        {
            return View();
        }

        public JsonResult GetLatestPaymentIntentId()
        {
            return Json(new { paymentIntentId = LatestPaymentIntentId }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CreatePaymentIntent()
        {
            try
            {
                StripeConfiguration.ApiKey = StripeSecretKey;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = 1000, // Amount in cents (i.e., $10.00)
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Description = "Description of the goods or services being exported",
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                LatestPaymentIntentId = intent.Id;

                return Json(new { clientSecret = intent.ClientSecret }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> ChargeDetails(string paymentIntentId)
        {
            try
            {
                var chargeService = new ChargeService();
                var chargeListOptions = new ChargeListOptions
                {
                    PaymentIntent = paymentIntentId,
                    Limit = 1
                };
                var charges = await chargeService.ListAsync(chargeListOptions);

                if (charges.Data.Count == 0)
                {
                    return Json(new { error = "No charges found for this PaymentIntent." }, JsonRequestBehavior.AllowGet);
                }

                var charge = charges.Data.First();

                if (string.IsNullOrEmpty(charge.BalanceTransactionId))
                {
                    return Json(new { error = "No balance transaction ID found for this charge." }, JsonRequestBehavior.AllowGet);
                }

                var balanceTransactionService = new BalanceTransactionService();
                var balanceTransaction = await balanceTransactionService.GetAsync(charge.BalanceTransactionId);

                var totalFees = balanceTransaction.Fee / 100.0m;
                var taxAmount = 4.44m; // Example tax amount in USD

                decimal conversionRate = 82.3024m;
                var amountInINR = (charge.Amount / 100.0m) * conversionRate;
                var feesInINR = totalFees;
                var taxInINR = taxAmount;
                var netAmountInINR = amountInINR - feesInINR;

                return Json(new
                {
                    amount = amountInINR,
                    currency = "INR",
                    fees = feesInINR,
                    tax = taxInINR,
                    netAmount = netAmountInINR,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

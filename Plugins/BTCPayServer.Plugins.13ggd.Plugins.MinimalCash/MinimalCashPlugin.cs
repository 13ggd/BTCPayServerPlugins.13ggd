using System;
using System.Threading.Tasks;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Hosting;
using BTCPayServer.Payments;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BTCPayServer.Plugins.MinimalCash
{
    public class MinimalCashPlugin : BTCPayServerPlugin
    {
        public const string PluginNavKey = nameof(MinimalCashPlugin) + "Nav";
        public const string SettingKey = "13ggd.MinimalCash.v5";

        public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
        [
            new() { Identifier = nameof(BTCPayServer), Condition = ">=2.1.5" }
        ];

        public override void Execute(IServiceCollection services)
        {
            var pmid = new PaymentMethodId("MINIMAL_CASH");
            services.AddSingleton<IPaymentMethodHandler>(provider => new MinimalCashPaymentMethodHandler(pmid));
            services.AddSingleton<ICheckoutModelExtension>(provider => new MinimalCashCheckoutModelExtension(pmid));
            services.AddDefaultPrettyName(pmid, "Minimal Cash (13ggd)");
            
            base.Execute(services);
        }
    }

    public class MinimalCashPaymentMethodHandler : IPaymentMethodHandler
    {
        public PaymentMethodId PaymentMethodId { get; }

        public MinimalCashPaymentMethodHandler(PaymentMethodId pmid)
        {
            PaymentMethodId = pmid;
        }

        public Task ConfigurePrompt(PaymentMethodContext context)
        {
            context.Prompt.Currency = "USD";
            context.Prompt.Divisibility = 2;
            context.Prompt.RateDivisibility = 2;
            return Task.CompletedTask;
        }

        public Task BeforeFetchingRates(PaymentMethodContext context)
        {
            return Task.CompletedTask;
        }

        public JsonSerializer Serializer => new JsonSerializer();

        public object ParsePaymentPromptDetails(JToken details)
        {
            return new MinimalCashPaymentDetails();
        }

        public object ParsePaymentMethodConfig(JToken config)
        {
            return new MinimalCashPaymentMethodConfig();
        }

        public object ParsePaymentDetails(JToken details)
        {
            return new MinimalCashPaymentData();
        }
    }

    public class MinimalCashCheckoutModelExtension : ICheckoutModelExtension
    {
        public PaymentMethodId PaymentMethodId { get; }

        public MinimalCashCheckoutModelExtension(PaymentMethodId pmid)
        {
            PaymentMethodId = pmid;
        }

        public string Image => "";
        public string Badge => "";

        public void ModifyCheckoutModel(CheckoutModelContext context)
        {
            if (context.Handler.PaymentMethodId != PaymentMethodId)
                return;
                
            context.Model.CheckoutBodyComponentName = "MinimalCashCheckout";
            context.Model.InvoiceBitcoinUrlQR = null;
            context.Model.ExpirationSeconds = int.MaxValue;
            context.Model.Activated = true;
            context.Model.ShowPayInWalletButton = true;
        }
    }

    public class MinimalCashPaymentDetails { }
    public class MinimalCashPaymentMethodConfig { }
    public class MinimalCashPaymentData { }
}

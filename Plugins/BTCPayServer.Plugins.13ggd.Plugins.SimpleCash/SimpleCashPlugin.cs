using System;
using System.Linq;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Hosting;
using BTCPayServer.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.13ggd.Plugins.SimpleCash;

public class SimpleCashPlugin : BaseBTCPayServerPlugin
{
    public const string PluginNavKey = nameof(SimpleCashPlugin) + "Nav";
    public const string SettingKey = "13ggd.SimpleCash";

    public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
    [
        new() { Identifier = nameof(BTCPayServer), Condition = ">=2.1.5" }
    ];

    public override void Execute(IServiceCollection services)
    {
        var pmid = new PaymentMethodId("CASH_13GGD");
        services.AddTransactionLinkProvider(pmid, new DefaultTransactionLinkProvider(null));
        services.AddSingleton<IPaymentMethodHandler>(provider => new SimpleCashPaymentMethodHandler(pmid));
        services.AddSingleton<ICheckoutModelExtension>(provider => new SimpleCashCheckoutModelExtension(pmid));
        services.AddDefaultPrettyName(pmid, "Cash (13ggd)");
        
        base.Execute(services);
    }
}

public class SimpleCashPaymentMethodHandler : IPaymentMethodHandler
{
    public PaymentMethodId PaymentMethodId { get; }

    public SimpleCashPaymentMethodHandler(PaymentMethodId pmid)
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

    public JsonSerializer Serializer { get; } = BlobSerializer.CreateSerializer().Serializer;

    public object ParsePaymentPromptDetails(JToken details)
    {
        return new SimpleCashPaymentDetails();
    }

    public object ParsePaymentMethodConfig(JToken config)
    {
        return new SimpleCashPaymentMethodConfig();
    }

    public object ParsePaymentDetails(JToken details)
    {
        return new SimpleCashPaymentData();
    }
}

public class SimpleCashCheckoutModelExtension : ICheckoutModelExtension
{
    public PaymentMethodId PaymentMethodId { get; }

    public SimpleCashCheckoutModelExtension(PaymentMethodId pmid)
    {
        PaymentMethodId = pmid;
    }

    public string Image => "";
    public string Badge => "";

    public void ModifyCheckoutModel(CheckoutModelContext context)
    {
        if (context.Handler.PaymentMethodId != PaymentMethodId)
            return;
            
        context.Model.CheckoutBodyComponentName = "SimpleCashCheckout";
        context.Model.InvoiceBitcoinUrlQR = null;
        context.Model.ExpirationSeconds = int.MaxValue;
        context.Model.Activated = true;
        context.Model.InvoiceBitcoinUrl = $"/stores/{context.Model.StoreId}/simplecash/MarkAsPaid?invoiceId={context.Model.InvoiceId}&returnUrl=/i/{context.Model.InvoiceId}";
        context.Model.ShowPayInWalletButton = true;
    }
}

public class SimpleCashPaymentDetails { }
public class SimpleCashPaymentMethodConfig { }
public class SimpleCashPaymentData { }

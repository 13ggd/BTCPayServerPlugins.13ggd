using System;
using System.Linq;
using BTCPayServer.Abstractions.Contracts;
using BTCPayServer.Hosting;
using BTCPayServer.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace BTCPayServer.Plugins.13ggd.Plugins.TestCash;

public class TestCashPlugin : BaseBTCPayServerPlugin
{
    public const string PluginNavKey = nameof(TestCashPlugin) + "Nav";
    public const string SettingKey = "13ggd.TestCash";

    public override IBTCPayServerPlugin.PluginDependency[] Dependencies { get; } =
    [
        new() { Identifier = nameof(BTCPayServer), Condition = ">=2.1.5" }
    ];

    public override void Execute(IServiceCollection services)
    {
        var pmid = new PaymentMethodId("TESTCASH");
        services.AddTransactionLinkProvider(pmid, new DefaultTransactionLinkProvider(null));
        services.AddSingleton<IPaymentMethodHandler>(provider => new TestCashPaymentMethodHandler(pmid));
        services.AddSingleton<ICheckoutModelExtension>(provider => new TestCashCheckoutModelExtension(pmid));
        services.AddDefaultPrettyName(pmid, "Test Cash (13ggd)");
        
        base.Execute(services);
    }
}

public class TestCashPaymentMethodHandler : IPaymentMethodHandler
{
    public PaymentMethodId PaymentMethodId { get; }

    public TestCashPaymentMethodHandler(PaymentMethodId pmid)
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
        return new TestCashPaymentDetails();
    }

    public object ParsePaymentMethodConfig(JToken config)
    {
        return new TestCashPaymentMethodConfig();
    }

    public object ParsePaymentDetails(JToken details)
    {
        return new TestCashPaymentData();
    }
}

public class TestCashCheckoutModelExtension : ICheckoutModelExtension
{
    public PaymentMethodId PaymentMethodId { get; }

    public TestCashCheckoutModelExtension(PaymentMethodId pmid)
    {
        PaymentMethodId = pmid;
    }

    public string Image => "";
    public string Badge => "";

    public void ModifyCheckoutModel(CheckoutModelContext context)
    {
        if (context.Handler.PaymentMethodId != PaymentMethodId)
            return;
            
        context.Model.CheckoutBodyComponentName = "TestCashCheckout";
        context.Model.InvoiceBitcoinUrlQR = null;
        context.Model.ExpirationSeconds = int.MaxValue;
        context.Model.Activated = true;
        context.Model.ShowPayInWalletButton = true;
    }
}

public class TestCashPaymentDetails { }
public class TestCashPaymentMethodConfig { }
public class TestCashPaymentData { }

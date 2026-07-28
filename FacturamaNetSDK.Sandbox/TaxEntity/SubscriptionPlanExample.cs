using FacturamaNetSDK.Client;

namespace FacturamaNetSDK.Sandbox.TaxEntity;

public class SubscriptionPlanExample
{
    private readonly FacturamaClient _client;

    public SubscriptionPlanExample(FacturamaClient client)
    {
        _client = client;
    }

    public async Task Run()
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("  Subscription Plan");
        Console.WriteLine("========================================\n");
        try
        {
            await GetSubscriptionPlanAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[Error inesperado] {ex.Message}");
        }
    }

    private async Task GetSubscriptionPlanAsync()
    {
        var subscriptionPlan = await _client.SubscriptionPlan.GetAsync();
        Console.WriteLine($"Subscription Plan: {subscriptionPlan.Plan}, Folios: {subscriptionPlan.CurrentFolios}");
    }
}

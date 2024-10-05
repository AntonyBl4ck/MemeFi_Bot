using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var clickingTask = ClickHandler.StartClickingAsync();
        var upgradingTask = StartUpgradeLoopAsync();

        await Task.WhenAll(clickingTask, upgradingTask); 
    }

    // Функция для запуска цикла обновлений раз в минуту
    private static async Task StartUpgradeLoopAsync()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Starting upgrade process...");

                await RunUpgradesAsync(); 

                Console.WriteLine("Upgrade process completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during upgrade: {ex.Message}");
            }

            Console.WriteLine("Waiting for 1 minute before next upgrade...");
            await Task.Delay(TimeSpan.FromMinutes(1)); 
        }
    }

    private static async Task RunUpgradesAsync()
    {
        try
        {
            await UpgradeHandler.PurchaseUpgradeAsync("EnergyRechargeRate");
            await UpgradeHandler.PurchaseUpgradeAsync("Damage");
            await UpgradeHandler.PurchaseUpgradeAsync("EnergyCap");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during upgrade: {ex.Message}");
        }
    }
}

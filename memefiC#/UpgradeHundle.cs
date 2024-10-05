using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class UpgradeHandler
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string Url = "https://api-gw-tg.memefi.club/graphql";
    private static readonly string BearerToken = GetBearerTokenFromFile("bearer_token.txt"); 

    private static string GetBearerTokenFromFile(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath).Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading bearer token from file: {ex.Message}");
            throw;
        }
    }

    public static async Task PurchaseUpgradeAsync(string upgradeType)
    {
        var payload = new
        {
            operationName = "telegramGamePurchaseUpgrade",
            variables = new
            {
                upgradeType = upgradeType
            },
            query = @"
            mutation telegramGamePurchaseUpgrade($upgradeType: UpgradeType!) {
                telegramGamePurchaseUpgrade(type: $upgradeType) {
                    ...FragmentBossFightConfig
                    __typename
                }
            }

            fragment FragmentBossFightConfig on TelegramGameConfigOutput {
                _id
                coinsAmount
                currentEnergy
                maxEnergy
                weaponLevel
                zonesCount
                tapsReward
                energyLimitLevel
                energyRechargeLevel
                tapBotLevel
                currentBoss {
                    _id
                    level
                    currentHealth
                    maxHealth
                    __typename
                }
                freeBoosts {
                    _id
                    currentTurboAmount
                    maxTurboAmount
                    turboLastActivatedAt
                    turboAmountLastRechargeDate
                    currentRefillEnergyAmount
                    maxRefillEnergyAmount
                    refillEnergyLastActivatedAt
                    refillEnergyAmountLastRechargeDate
                    __typename
                }
                bonusLeaderDamageEndAt
                bonusLeaderDamageStartAt
                bonusLeaderDamageMultiplier
                nonce
                spinEnergyNextRechargeAt
                spinEnergyNonRefillable
                spinEnergyRefillable
                spinEnergyTotal
                spinEnergyStaticLimit
                __typename
            }"
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);

        var request = new HttpRequestMessage(HttpMethod.Post, Url)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        request.Headers.Add("Origin", "https://tg-app.memefi.club");

        try
        {
            HttpResponseMessage response = await httpClient.SendAsync(request);

            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response status code: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine("Upgrade response: " + responseContent);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
    }
}

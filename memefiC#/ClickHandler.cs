using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ClickHandler
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
    public static async Task StartClickingAsync()
    {
        while (true)
        {
            int? currentTurboAmount = await GameConfigHandler.GetGameConfigAsync();
            Console.WriteLine($"Current Turbo Amount: {currentTurboAmount}");

            if (currentTurboAmount > 0)
            {
                await ActivateBoosterAsync(currentTurboAmount.Value);
            }

            int clickCount = 0;
            int tapsCount = currentTurboAmount > 0 ? 20 : 1;
            DateTime endTime = currentTurboAmount > 0 ? DateTime.Now.AddSeconds(20) : DateTime.MaxValue;

            while (DateTime.Now < endTime)
            {
                var response = await PerformClickAsync();
                if (response == null) continue;

                clickCount++;
                Console.WriteLine($"Click #{clickCount} - Current Energy: {response.currentEnergy}");

                if (response.currentEnergy < 10)
                {
                    Console.WriteLine("Energy is less than 10. Pausing for 1000 seconds...");
                    Thread.Sleep(1000 * 1000);
                    clickCount = 0;
                    Console.WriteLine("Resuming clicks...");
                }
                else
                {
                    int delay = new Random().Next(500, 2000);
                    Console.WriteLine($"Waiting for {delay / 1000.0} seconds before the next click...");
                    Thread.Sleep(delay);
                }
            }

            Console.WriteLine("Clicking completed. Restarting the process...");
        }
    }

    private static async Task<TelegramGameProcessTapsBatch> PerformClickAsync()
    {
        var payload = new
        {
            operationName = "MutationGameProcessTapsBatch",
            variables = new
            {
                payload = new
                {
                    nonce = Guid.NewGuid().ToString(),
                    tapsCount = 5,
                    vector = "2"
                }
            },
            query = @"
            mutation MutationGameProcessTapsBatch($payload: TelegramGameTapsBatchInput!) {
                telegramGameProcessTapsBatch(payload: $payload) {
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
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7");
        request.Headers.Add("Origin", "https://tg-app.memefi.club");

        try
        {
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<ResponseWrapper>(responseContent);
            Console.WriteLine(responseContent);

            var currentBoss = responseData.data.telegramGameProcessTapsBatch.currentBoss;

            if (currentBoss != null && currentBoss.currentHealth == 0)
            {
                Console.WriteLine("Current boss defeated, summoning the next boss...");
                await SetNextBossAsync();
            }

            return responseData.data.telegramGameProcessTapsBatch;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            return null;
        }
    }


    private static async Task SetNextBossAsync()
    {
        var payload = new
        {
            operationName = "telegramGameSetNextBoss",
            variables = new { },
            query = @"
        mutation telegramGameSetNextBoss {
            telegramGameSetNextBoss {
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
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7");
        request.Headers.Add("Origin", "https://tg-app.memefi.club");

        try
        {
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Next boss activated: " + responseContent);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error setting next boss: {e.Message}");
        }
    }

    private static async Task ActivateBoosterAsync(int tapsCount)
    {
        var payload = new
        {
            operationName = "telegramGameActivateBooster",
            variables = new
            {
                boosterType = "Turbo",
                tapsCount = tapsCount
            },
            query = @"
    mutation telegramGameActivateBooster($boosterType: BoosterType!, $tapsCount: Int!) {
        telegramGameActivateBooster(boosterType: $boosterType, tapsCount: $tapsCount) {
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

        Console.WriteLine("Request Headers:");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
        Console.WriteLine($"Authorization: {request.Headers.Authorization}");

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Console.WriteLine($"Accept: {string.Join(", ", request.Headers.Accept)}");

        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7");
        request.Headers.Add("Origin", "https://tg-app.memefi.club");

        foreach (var header in request.Headers)
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        try
        {
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Booster activated: " + responseContent);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
    }

    public class ResponseWrapper
    {
        public ResponseData data { get; set; }
    }

    public class ResponseData
    {
        public TelegramGameProcessTapsBatch telegramGameProcessTapsBatch { get; set; }
    }

    public class TelegramGameProcessTapsBatch
    {
        public string _id { get; set; }
        public int currentEnergy { get; set; }
        public int coinsAmount { get; set; }
        public CurrentBoss currentBoss { get; set; }
    }
}

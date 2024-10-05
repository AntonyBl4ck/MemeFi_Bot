using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class GameConfigHandler
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
    public static async Task<int?> GetGameConfigAsync()
    {
        try
        {
            var requestBody = new[]
            {
                new
                {
                    operationName = "QUERY_GAME_CONFIG",
                    variables = new { },
                    query = @"
                        query QUERY_GAME_CONFIG {
                            telegramGameGetConfig {
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
                }
            };

            string jsonRequestBody = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, Url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7");
            request.Headers.Add("Origin", "https://tg-app.memefi.club");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Game Config Response:");
            Console.WriteLine(responseContent);

            var responseWrapperList = JsonConvert.DeserializeObject<List<GameResponseWrapper>>(responseContent);

            if (responseWrapperList != null && responseWrapperList.Count > 0 &&
                responseWrapperList[0].data != null &&
                responseWrapperList[0].data.telegramGameGetConfig != null)
            {
                return responseWrapperList[0].data.telegramGameGetConfig.freeBoosts.currentTurboAmount;
            }

            Console.WriteLine("No game configuration data found.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving game configuration: {ex.Message}");
            return null;
        }
    }
}

public class GameResponseWrapper
{
    public GameResponseData data { get; set; }
}

public class GameResponseData
{
    public TelegramGameConfig telegramGameGetConfig { get; set; }
}

public class TelegramGameConfig
{
    public string _id { get; set; }
    public int coinsAmount { get; set; }
    public int currentEnergy { get; set; }
    public int maxEnergy { get; set; }
    public int weaponLevel { get; set; }
    public int zonesCount { get; set; }
    public object tapsReward { get; set; }
    public int energyLimitLevel { get; set; }
    public int energyRechargeLevel { get; set; }
    public int tapBotLevel { get; set; }
    public CurrentBoss currentBoss { get; set; }
    public FreeBoost freeBoosts { get; set; }
    public object bonusLeaderDamageEndAt { get; set; }
    public object bonusLeaderDamageStartAt { get; set; }
    public object bonusLeaderDamageMultiplier { get; set; }
    public string nonce { get; set; }
    public object spinEnergyNextRechargeAt { get; set; }
    public int spinEnergyNonRefillable { get; set; }
    public int spinEnergyRefillable { get; set; }
    public int spinEnergyTotal { get; set; }
    public int spinEnergyStaticLimit { get; set; }
}

public class CurrentBoss
{
    public string _id { get; set; }
    public int level { get; set; }
    public int currentHealth { get; set; }
    public int maxHealth { get; set; }
}

public class FreeBoost
{
    public string _id { get; set; }
    public int currentTurboAmount { get; set; }
    public int maxTurboAmount { get; set; }
    public object turboLastActivatedAt { get; set; }
    public DateTime turboAmountLastRechargeDate { get; set; }
    public int currentRefillEnergyAmount { get; set; }
    public int maxRefillEnergyAmount { get; set; }
    public object refillEnergyLastActivatedAt { get; set; }
    public DateTime refillEnergyAmountLastRechargeDate { get; set; }
}

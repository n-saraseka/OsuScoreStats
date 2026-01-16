using Newtonsoft.Json;
using OsuScoreStats.ApiClasses;
namespace OsuScoreStats;

public class TokenService
{
    private static readonly string tokenFile = "tokendata.json";
    private TokenInfo? _tokendata;

    /// <summary>
    /// Get TokenInfo from the token data file
    /// </summary>
    /// <returns>Populated TokenInfo (or null in case token is not set)</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TokenInfo GetToken()
    {
        if (_tokendata != null)
            return _tokendata;
        
        if (File.Exists(tokenFile))
        {
            _tokendata = JsonConvert.DeserializeObject<TokenInfo>(File.ReadAllText(tokenFile));
        }
        
        return _tokendata;
    }

    /// <summary>
    /// Sets new TokenInfo and writes to the token data file
    /// </summary>
    /// <param name="token">Populated TokenInfo object</param>
    public void SetToken(TokenInfo token)
    {
        _tokendata = token;
        File.WriteAllText(tokenFile, JsonConvert.SerializeObject(token));
    }
}
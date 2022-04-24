namespace ReflectionExamples.Part2.Services
{
    public interface IGithubTokenCache
    {
        Task<string> GetTokenAsync();
    }

    public class GithubTokenCache : IGithubTokenCache
    {
        private string AccessToken { get; set; }

        private int ExpirationTime { get; set; }

        private readonly IGithubAuthClient _githubAuthClient;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public GithubTokenCache(IGithubAuthClient githubAuthClient)
        {
            _githubAuthClient = githubAuthClient;
        }

        public async Task<string> GetTokenAsync()
        {
            await semaphore.WaitAsync();

            try
            {
                if (IsValid())
                    return AccessToken;

                await RefreshTokenAsync();

                return AccessToken;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private bool IsValid()
        {
            if (string.IsNullOrEmpty(AccessToken))
                return false;

            var timeSpan = DateTime.Now - new DateTime(1970, 1, 1);
            var epochTime = (int)timeSpan.TotalSeconds;

            var expTime = 60 * 10; //10 minutes

            return epochTime <= ExpirationTime - expTime; //change token 10 minutes before expiration
        }

        private async Task RefreshTokenAsync()
        {
            var tokenResponse= await _githubAuthClient.GetTokenAsync();
            AccessToken = tokenResponse.AccessToken;
            SetExpirationTime(tokenResponse.ExpiresIn);
        }

        private void SetExpirationTime(int timeTillExpirationSeconds)
        {
            var timeSpan = DateTime.Now - new DateTime(1970, 1, 1);
            var epochTime = (int)timeSpan.TotalSeconds;
            ExpirationTime = epochTime + timeTillExpirationSeconds;
        }
    }

}

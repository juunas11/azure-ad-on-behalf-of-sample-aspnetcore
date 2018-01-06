using ApiOnBehalfSample.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Claims;

namespace ApiOnBehalfSample.Services
{
    public class DistributedTokenCache : TokenCache
    {
        private ClaimsPrincipal _claimsPrincipal;
        private ILogger _logger;
        private IDistributedCache _distributedCache;
        private IDataProtector _protector;
        private string _cacheKey;

        public DistributedTokenCache(
            ClaimsPrincipal claimsPrincipal,
            IDistributedCache distributedCache,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
            : base()
        {
            _claimsPrincipal = claimsPrincipal;
            _cacheKey = BuildCacheKey(_claimsPrincipal);
            _distributedCache = distributedCache;
            _logger = loggerFactory.CreateLogger<DistributedTokenCache>();
            _protector = dataProtectionProvider.CreateProtector(typeof(DistributedTokenCache).FullName);
            AfterAccess = AfterAccessNotification;
            LoadFromCache();
        }

        private static string BuildCacheKey(ClaimsPrincipal claimsPrincipal)
        {
            string clientId = claimsPrincipal.FindFirstValue("aud", true);
            return string.Format(
                "UserId:{0}::ClientId:{1}",
                claimsPrincipal.GetObjectIdentifierValue(),
                clientId);
        }

        /// <summary>
        /// Attempts to load tokens from distributed cache.
        /// </summary>
        private void LoadFromCache()
        {
            byte[] cacheData = _distributedCache.Get(_cacheKey);
            if (cacheData != null)
            {
                Deserialize(_protector.Unprotect(cacheData));
                _logger.TokensRetrievedFromStore(_cacheKey);
            }
        }

        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                try
                {
                    if (Count > 0)
                    {
                        _distributedCache.Set(_cacheKey, _protector.Protect(Serialize()));
                        _logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    }
                    else
                    {
                        // There are no tokens for this user/client, so remove them from the cache.
                        // This was previously handled in an overridden Clear() method, but the built-in Clear() calls this
                        // after the dictionary is cleared.
                        _distributedCache.Remove(_cacheKey);
                        _logger.TokenCacheCleared(_claimsPrincipal.GetObjectIdentifierValue(false) ?? "<none>");
                    }
                    HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    _logger.WriteToCacheFailed(exp);
                    throw;
                }
            }
        }
    }
}
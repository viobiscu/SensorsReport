using NLog;
using System;
using System.Collections.Generic;

public class OrionldTenantCache
{
    private const int MAX_CACHE_SIZE = 10000;
    private readonly Dictionary<string, string> _cache;
    private readonly LinkedList<string> _order;
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();
    public OrionldTenantCache()
    {
        _cache = new Dictionary<string, string>(MAX_CACHE_SIZE);
        _order = new LinkedList<string>();
    }

    public void AddOrUpdate(string apiKey, string tenantId)
    {
        if (_cache.ContainsKey(apiKey))
        {
            // Update the tenantId and move the key to the end of the order list
            _cache[apiKey] = tenantId;
            _order.Remove(apiKey);
            _order.AddLast(apiKey);
            Logger.Trace($"Updated entry: {apiKey} -> {tenantId}");
        }
        else
        {
            if (_cache.Count >= MAX_CACHE_SIZE)
            {
                // Remove the oldest entry
                string oldestKey = _order.First.Value;
                _order.RemoveFirst();
                _cache.Remove(oldestKey);
                Logger.Warn($"Cache size exceeded. Removed oldest entry: {oldestKey}");
            }
            Logger.Trace($"Adding new entry: {apiKey} -> {tenantId}");
            // Add the new entry
            _cache[apiKey] = tenantId;
            _order.AddLast(apiKey);
        }
    }

    public bool TryGetTenantId(string apiKey, out string tenantId)
    {
        //if aliKey is nothing
        if (string.IsNullOrEmpty(apiKey))
        {
            tenantId = string.Empty;
            Logger.Warn("API key is null or empty.");
            return false;
        }

        Logger.Trace($"Trying to get tenantId for API key: {apiKey}");
        return _cache.TryGetValue(apiKey, out tenantId);
    }
}

using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TalentaReceiver.Models;

namespace jpk3service.Repositories.Cache
{
    public interface IJobCache	{
	    Task<bool> SetCache(Job o);
	    Task<bool> SetCaches(List<Job> o);
	    Task<Job> GetCache(string id);
	    Task<List<Job>> GetCaches();
	    Task<bool> Clear();
	}

public class JobCache : IJobCache
{
	private readonly IDistributedCache _cache;
	public JobCache(IDistributedCache cache)
	{
	    _cache = cache ?? throw new ArgumentNullException(nameof(cache));
	}

	public async Task<bool> Clear()
	{
	    await _cache.RemoveAsync("l-Job-cache");
	    await _cache.RemoveAsync("Job.*");
	    return true;
	}

	public async Task<Job> GetCache(string id)
	{
	    try
	    {
	        var o = await _cache.GetStringAsync($"Job.{id}");
	        if (o != null)
	        {
	            return JsonConvert.DeserializeObject<Job>(o);
	        }
	        return null;
	    }
	    catch
	    {
	        throw;
	    }
	}

	public async Task<List<Job>> GetCaches()
	{
	    try
	    {
	        var o = await _cache.GetStringAsync($"l-Job-cache");
	        if (o != null)
	        {
	            var lCurr = JsonConvert.DeserializeObject<List<Job>>(o);
	            return lCurr;
	        }

	        return null;
	    }
	    catch
	    {
	        throw;
	    }
	}

	public async Task<bool> SetCache(Job o)
	{
	    await _cache.SetStringAsync($"Job.{o.Id}", JsonConvert.SerializeObject(o), new DistributedCacheEntryOptions
	    {
	        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
	    });
	    return true;
	}

	public async Task<bool> SetCaches(List<Job> o)
	{
	    await _cache.SetStringAsync($"l-Job-cache", JsonConvert.SerializeObject(o), new DistributedCacheEntryOptions
	    {
	        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
	    });
	    return true;
	}
}
}

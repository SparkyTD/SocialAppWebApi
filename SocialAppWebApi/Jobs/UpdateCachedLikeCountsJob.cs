using Quartz;
using SocialAppWebApi.Services;

namespace SocialAppWebApi.Jobs;

public class UpdateCachedLikeCountsJob(LikesService likesService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await likesService.UpdateAllCachedLikeCountsAsync();
    }
}
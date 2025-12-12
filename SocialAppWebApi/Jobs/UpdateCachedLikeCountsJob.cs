using Quartz;
using SocialAppWebApi.Services.Interfaces;

namespace SocialAppWebApi.Jobs;

public class UpdateCachedLikeCountsJob(ILikesService likesService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await likesService.UpdateAllCachedLikeCountsAsync();
    }
}
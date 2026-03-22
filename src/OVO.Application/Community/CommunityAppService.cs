using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO.Internal;
using OVO.Permissions;
using OVO.Social;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.Community;

[Authorize(OVOPermissions.Community.Default)]
public class CommunityAppService : OVOAppService, ICommunityAppService
{
    private readonly IPollRepository _pollRepository;
    private readonly IPollVoteRepository _pollVoteRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IUserFollowRepository _userFollowRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public CommunityAppService(
        IPollRepository pollRepository,
        IPollVoteRepository pollVoteRepository,
        IUserBlockRepository userBlockRepository,
        IUserFollowRepository userFollowRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _pollRepository = pollRepository;
        _pollVoteRepository = pollVoteRepository;
        _userBlockRepository = userBlockRepository;
        _userFollowRepository = userFollowRepository;
        _asyncExecuter = asyncExecuter;
    }

    [AllowAnonymous]
    public virtual async Task<PagedResultDto<PollListItemDto>> GetPollsAsync(GetPollListInput input)
    {
        var query = (await _pollRepository.GetQueryableAsync()).Where(p => p.IsPublished);
        var me = CurrentUser.Id;

        if (me.HasValue)
        {
            var blocked = await GetRelatedBlockedUserIdsAsync(me.Value);
            query = query.Where(p => !blocked.Contains(p.CreatorUserId));
        }

        switch (input.Filter)
        {
            case PollFeedFilter.Popular:
                query = query.OrderByDescending(p => p.TotalVotes);
                break;
            case PollFeedFilter.Following when me.HasValue:
            {
                var fq = await _userFollowRepository.GetQueryableAsync();
                var followingIds = fq.Where(f => f.FollowerId == me.Value).Select(f => f.FollowingId);
                query = query
                    .Where(p => followingIds.Contains(p.CreatorUserId))
                    .OrderByDescending(p => p.CreationTime);
                break;
            }
            default:
                query = query.OrderByDescending(p => p.CreationTime);
                break;
        }

        var total = await _asyncExecuter.CountAsync(query);
        var polls = await _asyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<PollListItemDto>(total, polls.Select(MapListItem).ToList());
    }

    public virtual async Task<PollDto> CreatePollAsync(CreatePollDto input)
    {
        if (input.Options.Count is < 2 or > 3)
        {
            throw new UserFriendlyException("Oylama 2 veya 3 seçenek içermelidir.");
        }

        var counts = new int[input.Options.Count];
        var poll = new Poll(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.Question,
            OvoSerialization.PollOptionsToJson(input.Options),
            OvoSerialization.IntArrayToJson(counts),
            input.Publish,
            CurrentTenant.Id);

        await _pollRepository.InsertAsync(poll, autoSave: true);
        return MapPollDto(poll);
    }

    public virtual async Task<PollDto> VoteAsync(Guid pollId, VotePollInput input)
    {
        var poll = await _pollRepository.GetAsync(pollId);
        var options = OvoSerialization.PollOptionsFromJson(poll.OptionsJson);
        if (input.OptionIndex < 0 || input.OptionIndex >= options.Count)
        {
            throw new UserFriendlyException("Geçersiz seçenek.");
        }

        var counts = OvoSerialization.IntArrayFromJson(poll.OptionVoteCountsJson);
        if (counts.Length != options.Count)
        {
            counts = new int[options.Count];
        }

        var userId = CurrentUser.GetId();
        var voteQ = (await _pollVoteRepository.GetQueryableAsync())
            .Where(v => v.PollId == pollId && v.UserId == userId);
        var existingList = await _asyncExecuter.ToListAsync(voteQ.Take(1));
        var existing = existingList.FirstOrDefault();

        if (existing == null)
        {
            var vote = new PollVote(GuidGenerator.Create(), pollId, userId, input.OptionIndex, CurrentTenant.Id);
            await _pollVoteRepository.InsertAsync(vote, autoSave: true);
            counts[input.OptionIndex]++;
            poll.TotalVotes++;
        }
        else if (existing.OptionIndex != input.OptionIndex)
        {
            counts[existing.OptionIndex]--;
            counts[input.OptionIndex]++;
            existing.OptionIndex = input.OptionIndex;
            await _pollVoteRepository.UpdateAsync(existing, autoSave: true);
        }

        poll.OptionVoteCountsJson = OvoSerialization.IntArrayToJson(counts);
        await _pollRepository.UpdateAsync(poll, autoSave: true);
        return MapPollDto(poll);
    }

    private async Task<HashSet<Guid>> GetRelatedBlockedUserIdsAsync(Guid me)
    {
        var q = await _userBlockRepository.GetQueryableAsync();
        var asBlocker = await _asyncExecuter.ToListAsync(q.Where(b => b.BlockerId == me).Select(b => b.BlockedId));
        var asBlocked = await _asyncExecuter.ToListAsync(q.Where(b => b.BlockedId == me).Select(b => b.BlockerId));
        return asBlocker.Concat(asBlocked).ToHashSet();
    }

    private static PollListItemDto MapListItem(Poll p)
    {
        var opts = OvoSerialization.PollOptionsFromJson(p.OptionsJson);
        return new PollListItemDto
        {
            Id = p.Id,
            CreatorUserId = p.CreatorUserId,
            Question = p.Question,
            OptionCount = opts.Count,
            TotalVotes = p.TotalVotes,
            CreationTime = p.CreationTime,
            CreatorId = p.CreatorId
        };
    }

    private static PollDto MapPollDto(Poll p)
    {
        return new PollDto
        {
            Id = p.Id,
            CreatorUserId = p.CreatorUserId,
            Question = p.Question,
            Options = OvoSerialization.PollOptionsFromJson(p.OptionsJson),
            OptionVoteCounts = OvoSerialization.IntArrayFromJson(p.OptionVoteCountsJson).ToList(),
            TotalVotes = p.TotalVotes,
            IsPublished = p.IsPublished,
            CreationTime = p.CreationTime,
            CreatorId = p.CreatorId,
            LastModificationTime = p.LastModificationTime,
            LastModifierId = p.LastModifierId
        };
    }
}

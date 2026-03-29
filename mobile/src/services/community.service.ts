import { apiClient } from '@/services/api-client';
import type { PagedResultDto, PollFeedFilter, PollListItemDto, VotePollInput } from '@/types/ovo-api';

export interface GetPollsParams {
  filter?: PollFeedFilter;
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export async function getPolls(params: GetPollsParams = {}): Promise<PagedResultDto<PollListItemDto>> {
  const { data } = await apiClient.get<PagedResultDto<PollListItemDto>>('/api/app/community/polls', {
    params: {
      Filter: params.filter ?? 0,
      Sorting: params.sorting,
      SkipCount: params.skipCount ?? 0,
      MaxResultCount: params.maxResultCount ?? 20,
    },
  });
  return data;
}

export async function votePoll(pollId: string, input: VotePollInput): Promise<void> {
  await apiClient.post(`/api/app/community/vote/${pollId}`, input);
}

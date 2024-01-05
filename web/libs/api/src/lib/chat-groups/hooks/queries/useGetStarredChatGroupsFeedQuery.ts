import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
import {
   ChatGroupFeedEntryListApiResponse,
   ChatGroupFeedEntry,
   // @ts-ignore
} from "@openapi";
import { sleep } from "../../../utils";
import { DEFAULT_CACHE_TIME } from "../../../constants";

const getStarredChatGroupsFeedQuery = async (): Promise<
   ChatGroupFeedEntry[]
> => {
   const { status, data } =
      await chatGroupsClient.get<ChatGroupFeedEntryListApiResponse>(
         `starred/feed`,
         {
            headers: {},
         }
      );

   await sleep(2000);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const STARRED_FEED_KEY = [`feed`, `starred`];

export const useGetStarredChatGroupsFeedQuery = (
   options?: Omit<
      UseQueryOptions<
         ChatGroupFeedEntry[],
         unknown,
         ChatGroupFeedEntry[],
         string[]
      >,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useQuery({
      queryKey: STARRED_FEED_KEY,
      queryFn: getStarredChatGroupsFeedQuery,
      refetchOnWindowFocus: false,
      refetchInterval: 60 * 5 * 1000,
      gcTime: DEFAULT_CACHE_TIME,
      ...options,
   });
};

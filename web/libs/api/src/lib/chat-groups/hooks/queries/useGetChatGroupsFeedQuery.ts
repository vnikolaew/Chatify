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
} from "@openapi";
import { sleep } from "../../../utils";

export interface GetChatGroupsFeedModel {
   limit: number;
   offset: number;
}

const getChatGroupsFeed = async (
   model: GetChatGroupsFeedModel
): Promise<ChatGroupFeedEntry[]> => {
   const params = new URLSearchParams({
      limit: `${model.limit}`,
      offset: model.offset.toString(),
   });

   const { status, data } =
      await chatGroupsClient.get<ChatGroupFeedEntryListApiResponse>(`feed`, {
         headers: {},
         data: model,
         params,
      });
   await sleep(2000);

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export const useGetChatGroupsFeedQuery = (
   model: GetChatGroupsFeedModel = { limit: 10, offset: 0 },
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
      queryKey: [`feed`],
      queryFn: () => getChatGroupsFeed(model),
      refetchOnWindowFocus: false,
      refetchInterval: 60 * 5 * 1000,
      cacheTime: 60 * 60 * 1000,
      ...options,
   });
};

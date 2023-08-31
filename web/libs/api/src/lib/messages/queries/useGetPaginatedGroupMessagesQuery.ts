import { messagesClient } from "../client";
import {
   UseQueryOptions,
   useQueryClient,
   useInfiniteQuery,
   QueryKey,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupMessageEntry,
   ChatGroupMessageEntryCursorPagedApiResponse,
   CursorPaged,
   // @ts-ignore
} from "@openapi";
import { sleep } from "../../utils";
import { DEFAULT_CACHE_TIME, DEFAULT_STALE_TIME } from "../../constants";

export interface GetPaginatedGroupMessagesModel {
   groupId: string;
   pageSize: number;
   pagingCursor?: string;
}

const getPaginatedGroupMessages = async (
   model: GetPaginatedGroupMessagesModel
): Promise<CursorPaged<ChatGroupMessageEntry>> => {
   const { groupId, pageSize, pagingCursor } = model;
   const params = new URLSearchParams({
      pageSize: pageSize.toString(),
   });
   if (!!pagingCursor) params.set("pagingCursor", pagingCursor);

   const { status, data } =
      await messagesClient.get<ChatGroupMessageEntryCursorPagedApiResponse>(
         `${groupId}`,
         {
            headers: {},
            params,
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }
   await sleep(2000);

   return data.data as CursorPaged<ChatGroupMessageEntry>;
};

export const useGetPaginatedGroupMessagesQuery = (
   { pageSize, groupId, pagingCursor }: GetPaginatedGroupMessagesModel,
   options?: Omit<
      UseQueryOptions<any, unknown, any, (string | number)[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();
   return useInfiniteQuery<
      CursorPaged<ChatGroupMessageEntry>,
      Error,
      CursorPaged<ChatGroupMessageEntry>,
      QueryKey
   >({
      queryKey: [`chat-group`, groupId, `messages`],
      getNextPageParam: (lastPage) => {
         console.log(lastPage);
         return lastPage.pagingCursor;
      },
      getPreviousPageParam: (_, allPages) => allPages.at(-1)?.pagingCursor,
      queryFn: ({ queryKey: [_, groupId, __], pageParam = null! }) => {
         return getPaginatedGroupMessages({
            groupId: (groupId as string).toString(),
            pagingCursor: pageParam,
            pageSize: pageSize,
         });
      },
      enabled: !!groupId,
      // ...options,
   });
};

import { chatGroupsClient } from "../../client";
import { useQuery, useQueryClient, UseQueryOptions } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
//@ts-ignore
import { ChatGroup, ChatGroupListApiResponse } from "@openapi";


const getStarredChatGroups = async () => {
   const { status, data } = await chatGroupsClient.get<ChatGroupListApiResponse>(`starred`, {
      headers: {},
   });

   if (status !== HttpStatusCode.Ok) {
      throw new Error("error");
   }

   return data?.data;
};

export const STARRED_CHAT_GROUPS_KEY = [`starred`, `chat-groups`];

export const useGetStarredChatGroups = (options?: (Omit<UseQueryOptions<ChatGroup[], Error, ChatGroup[], any>, "initialData" | "queryKey"> & {
   initialData?: (() => undefined) | undefined
}) | undefined) => {
   const client = useQueryClient();
   return useQuery<ChatGroup[], Error, ChatGroup[], any>(
      STARRED_CHAT_GROUPS_KEY,
      {
         queryFn: getStarredChatGroups,
         onError: console.error,
         onSuccess: (_) => {
         },
         ...options
      },
   );
};

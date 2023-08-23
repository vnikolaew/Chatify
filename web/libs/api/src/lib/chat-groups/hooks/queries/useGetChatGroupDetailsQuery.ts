import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
// @ts-ignore
import { useMemo } from "react";
import { ChatGroup, User, UserStatus } from "@openapi/index";

export interface GetChatGroupDetailsModel {
   chatGroupId: string;
}

export interface GetChatGroupDetailsResponse {
   chatGroup: ChatGroup;
   creator: User;
   members: User[];
}

export const getChatGroupDetails = async (
   model: GetChatGroupDetailsModel
): Promise<GetChatGroupDetailsResponse> => {
   const { status, data } = await chatGroupsClient.get(`${model.chatGroupId}`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useMembersByCategory = (members?: User[], adminIds?: string[]) =>
   useMemo<Record<string, User[]>>(
      () =>
         (members ?? []).reduce(
            (acc, member: User) => {
               if (adminIds?.some((id) => id === member.id)) {
                  acc.admins.push(member);
                  return acc;
               } else
                  switch (member.status) {
                     case UserStatus.ONLINE:
                        acc.online.push(member);
                        break;
                     case UserStatus.OFFLINE:
                        acc.offline.push(member);
                        break;
                     case UserStatus.AWAY:
                        acc.away.push(member);
                        break;
                  }
               return acc;
            },
            { admins: [], online: [], offline: [], away: [] }
         ),
      [members]
   );

export const useGetChatGroupDetailsQuery = (
   chatGroupId: string,
   options?: Omit<
      UseQueryOptions<any, unknown, any, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useQuery({
      queryKey: [`chat-group`, chatGroupId],
      queryFn: ({ queryKey: [_, id] }) =>
         getChatGroupDetails({ chatGroupId: id }),
      cacheTime: 60 * 60 * 1000,
      enabled: !!chatGroupId,
      staleTime: 30 * 60 * 1000,
      ...options,
   });
};

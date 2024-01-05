import { chatGroupsClient } from "../../client";
import { UseQueryOptions, useQuery } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { useMemo } from "react";
// @ts-ignore
import {
   ChatGroupDetailsEntry,
   ChatGroupDetailsEntryApiResponse,
   User,
   UserStatus,
   // @ts-ignore
} from "@openapi";

export interface GetChatGroupDetailsModel {
   chatGroupId: string;
}

export const getChatGroupDetails = async (
   model: GetChatGroupDetailsModel
): Promise<ChatGroupDetailsEntry> => {
   const { status, data } =
      await chatGroupsClient.get<ChatGroupDetailsEntryApiResponse>(
         `${model.chatGroupId}`,
         {
            headers: {},
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data!;
};

export type MemberCategory = "admins" | "online" | "offline" | "away";

export const useMembersByCategory = (members?: User[], adminIds?: string[]) =>
   useMemo<Record<MemberCategory, User[]>>(
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
      [members, adminIds]
   );

export const useGetChatGroupDetailsQuery = (
   chatGroupId: string,
   options?: Omit<
      UseQueryOptions<
         ChatGroupDetailsEntry,
         Error,
         ChatGroupDetailsEntry,
         string[]
      >,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   return useQuery<
      ChatGroupDetailsEntry,
      Error,
      ChatGroupDetailsEntry,
      string[]
   >({
      queryKey: [`chat-group`, chatGroupId],
      queryFn: ({ queryKey: [_, chatGroupId] }) =>
         getChatGroupDetails({ chatGroupId }),
      enabled: !!chatGroupId,
      staleTime: 30 * 60 * 1000,
      ...options,
   });
};

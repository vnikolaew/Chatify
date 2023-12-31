import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
// @ts-ignore
import { ChatGroupMember, ChatGroupMemberApiResponse } from "@openapi";

export interface GetUserMembershipDetailsModel {
   chatGroupId: string;
   userId: string;
}

const getUserMembershipDetails = async (
   model: GetUserMembershipDetailsModel
): Promise<ChatGroupMember> => {
   const { status, data } =
      await chatGroupsClient.get<ChatGroupMemberApiResponse>(
         `members/${model.chatGroupId}/${model.userId}`,
         {
            headers: {},
         }
      );

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }
   if (status === HttpStatusCode.NotFound) {
      throw new Error("Not found");
   }

   return data.data!;
};

export const GET_USER_MEMBERSHIP_DETAILS_KEY = (
   groupId: string,
   userId: string
) => [`chat-group`, groupId, `members`, userId];

export const useGetUserMembershipDetailsQuery = (
   model: GetUserMembershipDetailsModel,
   options?: Omit<
      UseQueryOptions<ChatGroupMember, Error, ChatGroupMember, string[]>,
      "initialData"
   > & {
      initialData?: (() => undefined) | undefined;
   }
) => {
   const client = useQueryClient();

   return useQuery<ChatGroupMember, Error, ChatGroupMember, string[]>({
      queryKey: GET_USER_MEMBERSHIP_DETAILS_KEY(
         model.chatGroupId,
         model.userId
      ),
      queryFn: ({ queryKey: [_, id] }) => getUserMembershipDetails(model),
      gcTime: 60 * 60 * 1000,
      ...options,
   });
};

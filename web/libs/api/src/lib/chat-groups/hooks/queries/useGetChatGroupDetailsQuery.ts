import { chatGroupsClient } from "../../client";
import {
   UseQueryOptions,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import { User } from "@openapi/models/User";
import { ChatGroup } from "@openapi/models/ChatGroup";

export interface GetChatGroupDetailsModel {
   chatGroupId: string;
}

export interface GetChatGroupDetailsResponse {
   chatGroup: ChatGroup;
   creator: User;
   members: User[];
}

const getChatGroupDetails = async (
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
      ...options,
   });
};

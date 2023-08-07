import { chatGroupsClient } from "../../client";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HttpStatusCode } from "axios";

export interface GetChatGroupDetailsModel {
   chatGroupId: string;
}

const getChatGroupDetails = async (model: GetChatGroupDetailsModel) => {
   const { status, data } = await chatGroupsClient.get(`${model.chatGroupId}`, {
      headers: {},
   });

   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data;
};

export const useGetChatGroupDetailsQuery = (chatGroupId: string) => {
   const client = useQueryClient();

   useQuery({
      queryKey: [`chat-group`, chatGroupId],
      queryFn: ({ queryKey: [_, id] }) =>
         getChatGroupDetails({ chatGroupId: id }),
      cacheTime: 60 * 60 * 1000,
   });
};

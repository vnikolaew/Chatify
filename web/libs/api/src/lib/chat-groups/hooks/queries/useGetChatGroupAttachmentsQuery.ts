import { chatGroupsClient } from "../../client";
import {
   useInfiniteQuery,
   useQuery,
   useQueryClient,
} from "@tanstack/react-query";
import { HttpStatusCode } from "axios";
import {
   ChatGroupAttachment,
   ChatGroupAttachmentCursorPagedApiResponse,
   CursorPaged,
   UserNotification,
} from "@openapi";
import addNewFriendButton from "../../../../../../../apps/app/components/navbar/AddNewFriendButton";

export interface GetChatGroupAttachmentsModel {
   groupId: string;
   pageSize: number;
   pagingCursor: string;
}

const getChatGroupAttachments = async (
   model: GetChatGroupAttachmentsModel
): Promise<CursorPaged<ChatGroupAttachment>> => {
   const { groupId, pagingCursor, pageSize } = model;

   const params = new URLSearchParams({ pageSize: pageSize.toString() });
   if (pagingCursor) params.append(`pagingCursor`, pagingCursor);

   const { status, data } =
      await chatGroupsClient.get<ChatGroupAttachmentCursorPagedApiResponse>(
         `${groupId}/attachments`,
         {
            headers: {},
            params,
         }
      );
   if (status === HttpStatusCode.BadRequest) {
      throw new Error("error");
   }

   return data.data as CursorPaged<ChatGroupAttachment>;
};
export const useGetChatGroupAttachmentsQuery = (
   model: GetChatGroupAttachmentsModel
) => {
   const client = useQueryClient();

   return useInfiniteQuery<
      CursorPaged<ChatGroupAttachment>,
      Error,
      CursorPaged<ChatGroupAttachment>,
      any
   >({
      queryKey: [`chat-groups`, model.groupId, `attachments`],
      queryFn: () => getChatGroupAttachments(model),
      getNextPageParam: (lastPage: CursorPaged<ChatGroupAttachment>) => {
         return lastPage.pagingCursor;
      },
      getPreviousPageParam: (_, allPages: any) => allPages.at(-1)?.pagingCursor,
      refetchInterval: 5 * 60 * 1000,
   });
};

import React from "react";
import { useGetChatGroupAttachmentsQuery } from "@web/api";

export interface ChatGroupAttachmentsTabProps {
   chatGroupId: string;
}

const ChatGroupAttachmentsTab = ({
   chatGroupId,
}: ChatGroupAttachmentsTabProps) => {
   const {
      data: attachments,
      isLoading,
      error,
   } = useGetChatGroupAttachmentsQuery({
      groupId: chatGroupId,
      pageSize: 10,
      pagingCursor: null!,
   });

   console.log(attachments);
   return <div></div>;
};
export default ChatGroupAttachmentsTab;

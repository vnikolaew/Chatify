import React, { useMemo } from "react";
import { useGetChatGroupAttachmentsQuery } from "@web/api";
import { Link, Skeleton } from "@nextui-org/react";
import { MediaAttachment } from "./MediaAttachment";

export interface ChatGroupAttachmentsTabProps {
   chatGroupId: string;
}

const ChatGroupAttachmentsTab = ({
   chatGroupId,
}: ChatGroupAttachmentsTabProps) => {
   const {
      data: attachments,
      isLoading,
      isFetching,
      error,
   } = useGetChatGroupAttachmentsQuery({
      groupId: chatGroupId,
      pageSize: 10,
      pagingCursor: null!,
   });
   const noGroupAttachments = useMemo(
      () => attachments?.pages?.[0]?.total === 0,
      [attachments?.pages]
   );
   console.log(noGroupAttachments);

   return (
      <div>
         <div className={`mx-auto  my-2 grid max-w-fit grid-cols-2 gap-2`}>
            {isLoading && isFetching
               ? Array.from({ length: 10 }).map((_, i) => (
                    <div
                       className={`m-2 flex flex-col items-start gap-2`}
                       key={i}
                    >
                       <Skeleton
                          className={`rounded-medium h-36 w-28`}
                          key={i}
                       />
                       <div className={`w-full`}>
                          <Skeleton className={`h-3 w-3/5 rounded-full`} />
                          <Skeleton className={`mt-1 h-2 w-4/5 rounded-full`} />
                       </div>
                    </div>
                 ))
               : attachments?.pages
                    ?.flatMap((p) => p.items)
                    .map((attachment, i) => (
                       <MediaAttachment
                          key={attachment.attachmentId}
                          attachment={attachment}
                       />
                    ))}
            {noGroupAttachments && (
               <div className={`text-md col-span-2 mt-4 text-gray-300`}>
                  No attachments sent yet.
               </div>
            )}
         </div>
         {attachments?.pages?.at(-1)?.hasMore && (
            <div className={`mt-4 w-full text-center`}>
               <Link
                  className={`text-small mx-auto cursor-pointer`}
                  underline={`hover`}
                  color={`primary`}
               >
                  Show more
               </Link>
            </div>
         )}
      </div>
   );
};

export default ChatGroupAttachmentsTab;

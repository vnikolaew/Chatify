import React from "react";
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

   return (
      <div>
         <div className={`my-2 px-4 mx-auto grid gap-1 max-w-fit grid-cols-2`}>
            {isLoading && isFetching
               ? Array.from({ length: 10 }).map((_, i) => (
                    <div
                       className={`flex flex-col items-start m-2 gap-2`}
                       key={i}
                    >
                       <Skeleton
                          className={`w-28 h-36 rounded-medium`}
                          key={i}
                       />
                       <div className={`w-full`}>
                          <Skeleton className={`w-3/5 h-3 rounded-full`} />
                          <Skeleton className={`w-4/5 mt-1 h-2 rounded-full`} />
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
         </div>
         {attachments?.pages?.at(-1)?.hasMore ||
            (true && (
               <div className={`w-full mt-4 text-center`}>
                  <Link
                     className={`cursor-pointer text-small mx-auto`}
                     underline={`hover`}
                     color={`primary`}
                  >
                     Show more
                  </Link>
               </div>
            ))}
      </div>
   );
};

export default ChatGroupAttachmentsTab;

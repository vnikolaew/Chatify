import React from "react";
import { getMediaUrl, useGetChatGroupAttachmentsQuery } from "@web/api";
import {
   Button,
   Card,
   CardHeader,
   Image,
   Link,
   Skeleton,
   Tooltip,
} from "@nextui-org/react";
import moment from "moment";
import { ChatGroupAttachment } from "@openapi";
import { downloadImage } from "../../utils";
import { DownloadIcon } from "lucide-react";
import { useHover } from "@hooks";

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
            {(isLoading && isFetching) || true
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
               : attachments.pages
                    .flatMap((p) => p.items)
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

const MediaAttachment = ({
   attachment,
}: {
   attachment: ChatGroupAttachment;
}) => {
   const [cardRef, isHovered] = useHover<HTMLDivElement>();

   return (
      <div
         className={`m-2 max-w-[150px] min-w-fit w-full flex flex-col gap-2`}
         key={attachment.attachmentId}
      >
         <Card radius={"md"} className={`w-full h-full`} ref={cardRef}>
            <CardHeader
               className={`absolute flex-col items-end w-full p-0 z-20 top-1 right-1`}
            >
               {isHovered && (
                  <Tooltip
                     closeDelay={100}
                     delay={100}
                     placement={"bottom"}
                     offset={-2}
                     showArrow
                     shadow={"md"}
                     radius={"sm"}
                     size={"sm"}
                     classNames={{
                        base: `px-4 py-1 text-[.6rem]`,
                     }}
                     content={"Download"}
                  >
                     <Button
                        size={"sm"}
                        onPress={(_) => {
                           downloadImage(
                              attachment.mediaInfo.mediaUrl,
                              attachment.mediaInfo.fileName ?? "Untitled"
                           );
                        }}
                        radius={"full"}
                        className={`hover:bg-zinc-900 gap-0 !h-6 !w-6 !px-0 transition-background duration-100 bg-zinc-900 bg-opacity-20 group`}
                        startContent={
                           <DownloadIcon
                              className={`fill-foreground stroke-foreground group-hover:fill-foreground group-hover:stroke-foreground`}
                              size={12}
                           />
                        }
                        isIconOnly
                     />
                  </Tooltip>
               )}
            </CardHeader>
            <Image
               alt={attachment.mediaInfo.fileName}
               loading={`lazy`}
               shadow={"md"}
               radius={"md"}
               className={`relative w-full h-full object-cover`}
               removeWrapper
               src={getMediaUrl(attachment.mediaInfo.mediaUrl)}
            />
         </Card>
         <div className={`flex flex-col gap-0 items-start`}>
            <span className={`text-foreground text-medium`}>
               {attachment.mediaInfo.fileName ?? `Unnamed`}
            </span>
            <p className={`text-default-300 text-xs`}>
               Shared by{" "}
               <span className={`text-foreground inline-block mr-2`}>
                  {attachment.username}
               </span>
               on {moment(new Date(attachment.createdAt)).format("DD/MM/YYYY")}
            </p>
         </div>
      </div>
   );
};

export default ChatGroupAttachmentsTab;

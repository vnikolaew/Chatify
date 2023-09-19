"use client";
import React from "react";
import { Media } from "@openapi";
import {
   Button,
   Card,
   CardHeader,
   Chip,
   Image,
   Tooltip,
} from "@nextui-org/react";
import { DownloadIcon } from "lucide-react";
import { useHover } from "@hooks";
import { downloadImage, normalizeFileName } from "apps/app/utils";
import { getMediaUrl } from "@web/api";

export interface MessageAttachmentsSectionProps {
   messageId: string;
   attachments: Media[];
}

export const MessageAttachmentsSection = ({
   attachments,
   messageId,
}: MessageAttachmentsSectionProps) => {
   return (
      <div className={`text-xs mx-4 my-2`}>
         {attachments.map((a, id) => (
            <MessageAttachment key={id} attachment={a} />
         ))}
      </div>
   );
};

const MessageAttachment = ({ attachment }: { attachment: Media }) => {
   const [cardRef, isHovered] = useHover<HTMLDivElement>();

   return (
      <div className={`flex flex-col items-start gap-2`}>
         <Card ref={cardRef}>
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
                              attachment.mediaUrl,
                              attachment.fileName ?? "Untitled"
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
               alt={attachment.fileName}
               shadow={"md"}
               radius={"md"}
               className={`relative object-cover max-w-96`}
               removeWrapper
               src={getMediaUrl(attachment.mediaUrl)}
            />
         </Card>
         <Chip
            variant={"light"}
            color={"default"}
            size={"sm"}
            classNames={{
               base: `px-1 h-4`,
            }}
            className={`text-xs text-default-400 px-1`}
         >
            {attachment.fileName
               ? normalizeFileName(attachment.fileName)
               : "Untitled"}
         </Chip>
      </div>
   );
};

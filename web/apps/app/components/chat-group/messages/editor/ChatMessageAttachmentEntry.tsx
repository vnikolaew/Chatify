"use client";
import React from "react";
import { ChatifyFile } from "@components/chat-group/messages/editor/MessageTextEditor";
import { Badge, Button, Chip, Image, Tooltip } from "@nextui-org/react";
import CrossIcon from "@components/icons/CrossIcon";
import { normalizeFileName } from "apps/app/utils";

export interface ChatMessageAttachmentEntryProps {
   attachment: ChatifyFile;
   url: string;
   onRemove: () => void;
}

const ChatMessageAttachmentEntry = ({
   attachment,
   url,
   onRemove,
}: ChatMessageAttachmentEntryProps) => {
   return (
      <div key={attachment.id} className={`flex flex-col items-center gap-2`}>
         <Badge
            size={"sm"}
            classNames={{
               badge: `w-3 h-3 m-0 p-0`,
            }}
            content={
               <Tooltip
                  closeDelay={100}
                  disableAnimation
                  delay={100}
                  color={"default"}
                  size={"sm"}
                  classNames={{
                     base: `px-2 py-0`,
                  }}
                  showArrow
                  content={<span className={`text-[.6rem]`}>Remove file</span>}
               >
                  <Button
                     variant={"shadow"}
                     color={"default"}
                     onPress={onRemove}
                     className={`!w-fit hover:bg-zinc-900 !min-w-fit m-0 px-1 h-4`}
                     type={"button"}
                     size={"sm"}
                     radius={"full"}
                     startContent={
                        <CrossIcon
                           className={`stroke-foreground fill-transparent `}
                           size={10}
                        />
                     }
                     isIconOnly
                  />
               </Tooltip>
            }
            color={"default"}
         >
            <Image
               height={40}
               width={40}
               shadow={"md"}
               radius={"md"}
               src={url}
            />
         </Badge>
         <Chip
            variant={"flat"}
            color={"warning"}
            size={"sm"}
            classNames={{
               base: `px-1 h-4`,
            }}
            className={`text-[.6rem] px-1`}
         >
            {normalizeFileName(attachment.file.name)}
         </Chip>
      </div>
   );
};

export default ChatMessageAttachmentEntry;

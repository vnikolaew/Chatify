"use client";
import React, { useMemo } from "react";
import { useStarChatGroup, useUnstarChatGroup } from "@web/api";
import { Spinner, Tooltip } from "@nextui-org/react";
import { Star } from "lucide-react";
import { useIsGroupStarred } from "@web/hooks";

export interface ChatGroupStarSectionProps {
   chatGroupId: string;
}

const ChatGroupStarSection = ({ chatGroupId }: ChatGroupStarSectionProps) => {
   const { mutateAsync: starChatGroup, isLoading: starLoading } = useStarChatGroup();
   const { mutateAsync: unstarChatGroup, isLoading: unstarLoading } = useUnstarChatGroup();

   const isGroupStarred = useIsGroupStarred(chatGroupId);

   const starTooltipContent = useMemo(() =>
         isGroupStarred ? `Unstar` : `Star`,
      [isGroupStarred]);

   const showSpinner = useMemo(() => starLoading || unstarLoading, [starLoading, unstarLoading]);

   async function handleClickStar() {
      if (isGroupStarred) {
         await unstarChatGroup({ chatGroupId });
      } else await starChatGroup({ chatGroupId });
   }

   return showSpinner ? (
      <Spinner className={`self-end`} classNames={{
         circle1: `h-4 w-4`,
         circle2: `h-4 w-4`,
      }} size={`sm`} color={`primary`} />
   ) : (
      <Tooltip showArrow
               classNames={{
                  content: `h-5`,
               }}
               size={`sm`} placement={`right`}
               content={<span
                  className={`text-xxs`}>{starTooltipContent} </span>}>
         <Star
            onClick={handleClickStar}
            className={`stroke-primary-500 transition-colors self-end cursor-pointer hover:fill-primary-500 ${isGroupStarred ? `fill-primary-500` : ``}`}
            size={20} />
      </Tooltip>
   );
};

export default ChatGroupStarSection;

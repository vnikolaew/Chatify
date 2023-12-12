"use client";
import React, { useCallback, useMemo, useState } from "react";
import {
   Badge,
   Button,
   Spinner,
   Tooltip,
   useDisclosure,
} from "@nextui-org/react";
import { ReactionsSummaryTooltipContent } from "@components/chat-group/messages";
import {
   useGetAllReactionsForMessage,
   useReactToGroupMessageMutation, useReactToGroupMessageReplyMutation,
   useUnreactToGroupMessageMutation, useUnreactToGroupMessageReplyMutation,
} from "@web/api";
import { ChatMessageReaction, UserMessageReaction } from "@openapi";
import { useCurrentChatGroup, useCurrentUserId } from "@hooks";
import { PlusIcon } from "@icons";
import HappyFaceIcon from "@components/icons/HappyFaceIcon";
import Picker from "@emoji-mart/react";
import data from "@emoji-mart/data";
import { hexToDecimal } from "apps/app/utils";

export interface ChatMessageReactionSectionProps {
   messageId: string;
   reactionCounts: Record<string, number>;
   userReaction: UserMessageReaction;
   isReply?: boolean;
}

export const ChatMessageReactionSection = ({
                                              reactionCounts,
                                              userReaction,
                                              messageId,
                                              isReply = false,
                                           }: ChatMessageReactionSectionProps) => {
   const [fetchReactions, setFetchReactions] = useState(false);
   const {
      isOpen: reactTooltipOpen,
      onOpenChange: onReactTooltipOpen,
      onClose: onReactTooltipClose,
   } = useDisclosure({ defaultOpen: false });
   const {
      data: reactions,
      isLoading: reactionsLoading,
      error,
   } = useGetAllReactionsForMessage(messageId, {
      enabled: fetchReactions,
   });
   const { mutateAsync: reactToMessage } = useReactToGroupMessageMutation();
   const { mutateAsync: reactToMessageReply } = useReactToGroupMessageReplyMutation();
   const { mutateAsync: unreactToMessageReply } = useUnreactToGroupMessageReplyMutation();
   const { mutateAsync: unreactToMessage } = useUnreactToGroupMessageMutation();

   const groupId = useCurrentChatGroup();
   const meId = useCurrentUserId();

   const hasUserReacted = useCallback(
      (reactionCode: number) =>
         reactions?.some(
            (r) => r.userId === meId && r.reactionCode === reactionCode,
         ) || userReaction?.reactionCode === reactionCode,
      [meId, reactions, userReaction?.reactionCode],
   );

   const reactionsByCode = useMemo<Record<string, ChatMessageReaction[]>>(
      () =>
         reactions?.reduce<Record<string, ChatMessageReaction[]>>((acc, r) => {
            const key = r.reactionCode.toString();
            return {
               ...acc,
               [key]: [...(acc[key] ?? []), r],
            };
         }, {}) ?? {},
      [reactions],
   );

   const handleFetchReactions = () => {
      if (!fetchReactions) setFetchReactions(true);
   };

   const handleReactToMessage = async (reactionCode: number) => {
      if (hasUserReacted(reactionCode)) {
         const reactionId = reactions.find((r) => r.userId === meId).id!;
         console.log(reactionId, reactions);

         if (isReply) {
            await unreactToMessageReply({ messageId, groupId, messageReactionId: reactionId });
         } else await unreactToMessage({
            groupId,
            messageId,
            messageReactionId: reactionId,
         });
      } else {
         if (isReply) {
            await reactToMessageReply({ messageId, groupId, reactionType: reactionCode });
         } else await reactToMessage({
            messageId,
            groupId,
            reactionType: reactionCode,
         });
      }
   };
   return (
      <div className={`items-center mt-2 flex gap-1`}>
         {Object.entries(reactionCounts ?? {}).map(
            ([reactionCode, count], i) => (
               <Tooltip
                  showArrow
                  delay={100}
                  closeDelay={100}
                  offset={2}
                  size={"sm"}
                  placement={"top"}
                  key={i}
                  content={
                     reactionsLoading ? (
                        <Spinner size={"sm"} color={"danger"} />
                     ) : (
                        <ReactionsSummaryTooltipContent
                           reactions={reactionsByCode[reactionCode]}
                        />
                     )
                  }
               >
                  <Button
                     className={`px-2 min-w-fit max-w-fit w-fit items-center justify-center h-4 py-0`}
                     onMouseEnter={handleFetchReactions}
                     onPress={(_) => handleReactToMessage(Number(reactionCode))}
                     size={"sm"}
                     color={
                        hasUserReacted(Number(reactionCode))
                           ? "primary"
                           : "default"
                     }
                     variant={
                        hasUserReacted(Number(reactionCode))
                           ? "solid"
                           : `bordered`
                     }
                  >
                     <span
                        className={`text-xs text-foreground-700`}
                        dangerouslySetInnerHTML={{
                           __html: `&#${reactionCode}; ${count}`,
                        }}
                     />
                  </Button>
               </Tooltip>
            ),
         )}
         <Tooltip
            showArrow
            delay={200}
            isOpen={reactTooltipOpen}
            onOpenChange={onReactTooltipOpen}
            closeDelay={200}
            offset={2}
            size={"sm"}
            placement={"top"}
            content={
               <div className={`max-h-[300px] overflow-y-scroll`}>
                  <Picker
                     data={data}
                     onEmojiSelect={async (e: any) => {
                        await handleReactToMessage(hexToDecimal(e.unified));
                        setTimeout(onReactTooltipClose, 100);
                     }}
                  />
               </div>
            }
         >
            <Button
               className={`px-2 min-w-fit max-w-fit w-fit items-center justify-center h-4 py-0`}
               isIconOnly
               onMouseEnter={handleFetchReactions}
               startContent={
                  <Badge
                     size={"sm"}
                     classNames={{
                        badge: `bg-default px-0`,
                     }}
                     disableOutline
                     shape={"circle"}
                     content={
                        <PlusIcon className={`fill-foreground`} size={8} />
                     }
                     color={`default`}
                  >
                     <HappyFaceIcon className={`fill-foreground`} size={14} />
                  </Badge>
               }
               size={"sm"}
               color={`default`}
               variant={`ghost`}
            />
         </Tooltip>
      </div>
   );
};

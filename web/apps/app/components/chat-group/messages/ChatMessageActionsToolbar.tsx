"use client";
import React, { useMemo } from "react";
import TooltipButton from "@components/common/TooltipButton";
import { PinIcon } from "@icons";
import CommentIcon from "@components/icons/CommentIcon";
import ForwardIcon from "@components/icons/ForwardIcon";
import { Spinner } from "@nextui-org/react";
import { usePinGroupChatMessage } from "@web/api";
import { useCurrentChatGroup } from "@hooks";
import ChatMessageMoreActionsButton from "@components/chat-group/messages/ChatMessageMoreActionsButton";
import { EditIcon } from "lucide-react";
import { useTranslations } from "next-intl";
import { useReplyToMessageContext } from "@components/chat-group";

export interface ChatMessageActionsToolbarProps {
   showMoreActions: boolean;
   onOpenForwardMessageModal: () => void;
   messageId: string;
   onEditMessageChange?: (editing: boolean) => void;
}

const ChatMessageActionsToolbar = ({
                                      showMoreActions,
                                      onOpenForwardMessageModal,
   onEditMessageChange,
                                      messageId,
                                   }: ChatMessageActionsToolbarProps) => {
   const groupId = useCurrentChatGroup();
   const {
      mutateAsync: pinMessage,
      isLoading: pinLoading,
      error,
   } = usePinGroupChatMessage();
   const t = useTranslations(`MainArea.ChatMessages.Popups`);
   const [_, setReplyToMessage] = useReplyToMessageContext();

   const messageActions = useMemo(() => {
      return [
         {
            label: t(`PinForThisGroup`),
            action: async () => {
               await pinMessage({ messageId, groupId });
            },
            loading: pinLoading,
            Icon: <PinIcon className={`fill-foreground`} size={14} />,
         },
         {
            label: t(`ReplyToThisMessage`),
            action: async () => {
               console.log(`Replying ...`);
               setReplyToMessage(messageId)
            },
            loading: false,
            Icon: <CommentIcon className={`fill-foreground`} size={14} />,
         },
         {
            label: t(`ForwardThisMessage`),
            action: async () => {
               console.log(`Opening modal ...`);
               onOpenForwardMessageModal();
            },
            loading: false,
            Icon: <ForwardIcon className={`fill-foreground`} size={14} />,
         },
         {
            label: t(`Edit`),
            action: async () => {
               console.log(`Editing message ...`);
               onEditMessageChange(true)
            },
            loading: false,
            Icon: <EditIcon className={`stroke-foreground`} size={14} />,
         },
      ];
   }, [pinMessage, messageId, groupId, pinLoading, t, onOpenForwardMessageModal, onEditMessageChange]);

   return (
      <div
         className={`absolute px-1 border-1 border-default-300 z-10 flex items-center gap-1 text-xs min-h-fit max-h-fit rounded-md bg-zinc-900 -translate-y-1/2 top-0 right-10`}
      >
         {messageActions.map(({ label, Icon, action, loading }, i) => (
            <TooltipButton
               radius={"full"}
               key={label}
               placement={"top"}
               onClick={action}
               chipProps={{
                  color: `default`,
                  variant: `light`,
                  radius: "sm",
                  classNames: {
                     base: "h-7 w-7 p-0",
                  },
               }}
               size={"sm"}
               classNames={{ base: `p-0 text-[.7rem]` }}
               offset={2}
               icon={
                  loading ? (
                     <Spinner
                        className={`w-4 h-4`}
                        classNames={{
                           base: `w-4 h-4 `,
                           wrapper: `w-4 h-4`,
                        }}
                        color={`white`}
                     />
                  ) : (
                     Icon
                  )
               }
               content={label}
            />
         ))}
         {showMoreActions && (
            <ChatMessageMoreActionsButton  />
         )}
      </div>
   );
};


export default ChatMessageActionsToolbar;

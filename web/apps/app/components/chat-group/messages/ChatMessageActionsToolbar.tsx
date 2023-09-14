"use client";
import React, { Fragment, useMemo, useState } from "react";
import TooltipButton from "@components/TooltipButton";
import { PinIcon } from "@icons";
import CommentIcon from "@components/icons/CommentIcon";
import ForwardIcon from "@components/icons/ForwardIcon";
import {
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   Spinner,
   useDisclosure,
} from "@nextui-org/react";
import VerticalDotsIcon from "@components/icons/VerticalDotsIcon";
import TooltipWithPopoverActionButton from "@components/TooltipWithPopoverActionButton";
import { EditIcon } from "lucide-react";
import ThrashIcon from "@components/icons/ThrashIcon";
import { usePinGroupChatMessage } from "@web/api";
import { useCurrentChatGroup } from "@hooks";

export interface ChatMessageActionsToolbarProps {
   showMoreActions: boolean;
   messageId: string;
}

const ChatMessageActionsToolbar = ({
   showMoreActions,
   messageId,
}: ChatMessageActionsToolbarProps) => {
   const {
      isOpen: isMoreActionsDropdownMenuOpen,
      onOpenChange: onnMoreActionsDropdownMenuOpenChange,
   } = useDisclosure({ defaultOpen: false });
   const groupId = useCurrentChatGroup();
   const {
      mutateAsync: pinMessage,
      isLoading: pinLoading,
      error,
   } = usePinGroupChatMessage();
   const {
      isOpen: forwardMessageModalOpen,
      onOpenChange: onForwardMessageModalOpenChange,
      onOpen,
   } = useDisclosure();

   const messageActions = useMemo(() => {
      return [
         {
            label: "Pin for this group",
            action: async () => {
               await pinMessage({ messageId, groupId });
            },
            loading: pinLoading,
            Icon: PinIcon,
         },
         {
            label: "Reply to this message",
            action: async () => {},
            loading: false,
            Icon: CommentIcon,
         },
         {
            label: "Forward this message",
            action: async () => onOpen(),
            loading: false,
            Icon: ForwardIcon,
         },
      ];
   }, [pinMessage, messageId, groupId, pinLoading, onOpen]);

   return (
      <div
         className={`absolute px-1 border-1 border-default-300 z-10 flex items-center gap-1 text-xs min-h-fit max-h-fit rounded-md bg-zinc-900 -translate-y-1/2 top-0 right-10`}
      >
         <Modal
            shadow={"md"}
            radius={"sm"}
            placement={`center`}
            size={`sm`}
            onOpenChange={onForwardMessageModalOpenChange}
            isOpen={forwardMessageModalOpen}
         >
            <ModalContent>
               {(onClose) => (
                  <Fragment>
                     <ModalHeader></ModalHeader>
                     <ModalBody></ModalBody>
                     <ModalFooter></ModalFooter>
                  </Fragment>
               )}
            </ModalContent>
         </Modal>
         {messageActions.map(({ label, Icon, action, loading }, i) => (
            <TooltipButton
               radius={"full"}
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
                     <Icon className={`fill-foreground`} size={14} />
                  )
               }
               content={label}
            />
         ))}
         {showMoreActions && (
            <TooltipWithPopoverActionButton
               popoverContent={<MoreMessageActionsDropdown />}
               tooltipContent={"More actions"}
               icon={
                  <VerticalDotsIcon className={`fill-foreground`} size={14} />
               }
               isOpen={isMoreActionsDropdownMenuOpen}
               popoverProps={{
                  offset: 0,
                  classNames: {
                     base: `p-0 rounded-md`,
                  },
               }}
               tooltipProps={{
                  placement: "top",
                  classNames: { base: `p-0 px-2 text-[.7rem]` },
               }}
               chipProps={{
                  color: `default`,
                  variant: `light`,
                  radius: "sm",
                  classNames: {
                     base: "h-7 w-7 px-0",
                  },
               }}
               onOpenChange={onnMoreActionsDropdownMenuOpenChange}
            />
         )}
      </div>
   );
};

const MoreMessageActionsDropdown = () => {
   return (
      <div
         className={`flex z-30 cursor-pointer my-2 min-w-[150px] py-1 w-full text-xs flex-col items-start gap-1`}
      >
         <div
            className={`text-foreground py-1 group flex items-center justify-between px-2 w-full hover:bg-primary`}
         >
            <span>Edit message</span>
            <EditIcon className={`invisible group-hover:visible`} size={12} />
         </div>
         <div
            className={`text-danger flex items-center font-light group py-1 transition-all duration-[50ms] hover:text-foreground justify-between px-2 w-full hover:bg-danger`}
         >
            <span className={`group-hover:font-normal`}>Delete message</span>
            <ThrashIcon
               className={`invisible fill-foreground group-hover:visible`}
               size={14}
            />
         </div>
      </div>
   );
};

export default ChatMessageActionsToolbar;

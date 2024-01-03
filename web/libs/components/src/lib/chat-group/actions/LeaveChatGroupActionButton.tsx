"use client";
import { TooltipButton } from "@web/components";
import React, { Fragment, useState } from "react";
import {
   Button,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader, Spinner,
   useDisclosure,
} from "@nextui-org/react";
import { useTranslations } from "next-intl";
import { LogOutIcon } from "lucide-react";
import { useLeaveChatGroupMutation } from "@web/api";
import { useCurrentChatGroup } from "@web/hooks";
import { useRouter } from "next/navigation";

export interface LeaveChatGroupActionButtonProps {

}

export const LeaveChatGroupActionButton = ({}: LeaveChatGroupActionButtonProps) => {
   const { isOpen, onOpenChange, onOpen, onClose } = useDisclosure({
      defaultOpen: false,
   });
   const t = useTranslations("MainArea.TopBar.Popups");
   const { mutateAsync: leaveChatGroup, isLoading, error } = useLeaveChatGroupMutation();
   const chatGroupId = useCurrentChatGroup();
   const router = useRouter();
   const [leaveReason, setLeaveReason] = useState(``);

   async function handleLeaveGroup() {
      await leaveChatGroup({ groupId: chatGroupId, reason: leaveReason },
         {
            onSuccess: (_, { groupId }) => router.push(`/`, {}),
            onError: console.error,
         });
   }

   return (
      <Fragment>
         <TooltipButton
            chipProps={{
               className: `hover:bg-danger-500`,
               classNames: {
                  base: "h-9 w-9 px-0 ",
               },
            }}
            size={`sm`}
            shadow={`sm`}
            classNames={{
               base: `text-xs `,
               content: `text-[10px] h-5`,
            }}
            onClick={onOpenChange}
            // tooltipContent={t(`LeaveChatGroup`)}
            icon={<LogOutIcon className={`stroke-danger-500 group-hover:stroke-white`} size={20} />}
            content={t(`LeaveChatGroup`)}
         />
         <Modal
            classNames={{
               closeButton: `mt-2 mr-2`,
            }}
            motionProps={{
               variants: {
                  enter: {
                     y: 0,
                     opacity: 1,
                     transition: {
                        duration: 0.3,
                        ease: "easeOut",
                     },
                  },
                  exit: {
                     y: -20,
                     opacity: 0,
                     transition: {
                        duration: 0.3,
                        ease: "easeIn",
                     },
                  },
               },
            }} size={`md`}
            onOpenChange={onOpenChange} onClose={onClose} isOpen={isOpen}>
            <ModalContent>
               {(onClose) => (
                  <Fragment>
                     <ModalHeader className={`pt-4 pb-0 text-large`}>Confirm</ModalHeader>
                     <ModalBody className={`text-sm mt-1 flex flex-col gap-1`}>
                        <span className={`!text-medium`}>
                           Are you sure you want to leave? <br />
                        </span>
                        <span className={`text-sm mt-4`}>
                           If so, you can optionally provide a reason below:
                        </span>
                        <Input
                           placeholder={`Type a reason ...`}
                           className={`mt-1 w-2/3`}
                           classNames={{
                              input: `h-4`,
                              inputWrapper: `py-0 !h88 !max-h-8`,
                              innerWrapper: `py-0 !h-7 !max-h-7`,
                           }}
                           onValueChange={setLeaveReason}
                           value={leaveReason}
                           color={`default`} variant={`flat`}
                           size={`sm`}
                           type={`text`} />
                     </ModalBody>
                     <ModalFooter>
                        <Button
                           variant={`shadow`}
                           size={`sm`}
                           color={`default`}
                           className={`px-4`}
                           onPress={onClose}
                        >
                           Cancel
                        </Button>
                        <Button
                           isLoading={isLoading}
                           spinner={<Spinner className={`self-center`} classNames={{
                              circle1: `h-4 w-4`,
                              circle2: `h-4 w-4`
                           } } size={`sm`} color={`white`} />}
                           size={`sm`}
                           startContent={!isLoading && <LogOutIcon className={`stroke-white `} size={8} />}
                           variant={`shadow`}
                           onPress={handleLeaveGroup}
                           color={`danger`}
                           className={`ml-2 px-4 `}
                        >
                           {isLoading ? `Leaving ...` : `Leave group`}
                        </Button>
                     </ModalFooter>
                  </Fragment>
               )}
            </ModalContent>
         </Modal>
      </Fragment>
   );
};

"use client";
import React, { Fragment, useState } from "react";
import { TooltipButton } from "@components/common";
import { Edit } from "lucide-react";
import {
   Button,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader,
   useDisclosure,
} from "@nextui-org/react";
import { ChatGroupDetailsEntry } from "@openapi";
import { useIsChatGroupPrivate } from "@hooks";
import { useTranslations } from "next-intl";
import { useEditChatGroupMutation } from "@web/api";

export interface EditChatGroupActionButtonProps {
   chatGroup: ChatGroupDetailsEntry;
}

const EditChatGroupActionButton = ({
                                      chatGroup,
                                   }: EditChatGroupActionButtonProps) => {
   const { isOpen, onOpen, onOpenChange,onClose } = useDisclosure();
   const isPrivate = useIsChatGroupPrivate(chatGroup);
   const [newAbout, setNewAbout] = useState(chatGroup?.chatGroup?.about ?? ``);

   const [isDirty, setIsDirty] = useState(false);
   const { mutateAsync: editChatGroup, isLoading: editLoading, error: editError } = useEditChatGroupMutation();
   const t = useTranslations("MainArea.TopBar.Popups");

   async function handleSaveChanges(e: any): Promise<any> {
      await editChatGroup({ chatGroupId: chatGroup.chatGroup.id, about: newAbout });
      onClose()
   }

   return (
      <Fragment>
         <TooltipButton
            chipProps={{
               classNames: {
                  base: "h-9 w-9 px-0",
               },
            }}
            size={`sm`}
            shadow={`sm`}
            classNames={{
               base: `text-xs `,
               content: `text-[10px] h-5`,
            }}
            onClick={onOpenChange}
            icon={<Edit className={`stroke-default-400`} size={20} />}
            content={t(`EditChatGroup`)}
         />
         <Modal motionProps={{
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
         }} size={`md`} onOpenChange={onOpenChange} isOpen={isOpen}>
            <ModalContent className={`px-2 py-2`}>
               {(onClose) => (
                  <Fragment>
                     <ModalHeader>
                        Edit{" "}
                        {isPrivate
                           ? `private group`
                           : chatGroup?.chatGroup?.name}
                     </ModalHeader>
                     <ModalBody className={`mt-2`}>
                        <Input
                           placeholder={`Best group ever`}
                           onValueChange={(value) => {
                              setIsDirty(true);
                              setNewAbout(value)
                           }}
                           size={`md`}
                           color={`default`}
                           value={newAbout}
                           variant={`flat`}
                           classNames={{
                              label: `text-xs`,
                           }}
                           labelPlacement={`outside`}
                           label={"Change Description"}
                           type={"text"}
                        />
                     </ModalBody>
                     <ModalFooter>
                        <Button
                           variant={`shadow`}
                           size={`md`}
                           color={`danger`}
                           className={``}
                           onPress={onClose}
                        >
                           Cancel
                        </Button>
                        <Button
                           size={`md`}
                           variant={`shadow`}
                           onPress={handleSaveChanges}
                           isDisabled={!isDirty}
                           color={`default`}
                           className={`ml-2`}
                        >
                           Save changes
                        </Button>
                     </ModalFooter>
                  </Fragment>
               )}
            </ModalContent>
         </Modal>
      </Fragment>
   );
};

export default EditChatGroupActionButton;

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

export interface EditChatGroupActionButtonProps {
   chatGroup: ChatGroupDetailsEntry;
}

const EditChatGroupActionButton = ({
                                      chatGroup,
                                   }: EditChatGroupActionButtonProps) => {
   const { isOpen, onOpen, onOpenChange } = useDisclosure();
   const isPrivate = useIsChatGroupPrivate(chatGroup);
   const t = useTranslations("MainArea.TopBar.Popups");

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
         <EditChatGroupModal
            header={`Edit ${isPrivate ? `private group` : chatGroup?.chatGroup?.name}`}
            isOpen={isOpen}
            onOpenChange={onOpenChange} />
      </Fragment>
   );
};

interface EditChatGroupModalProps {
   isOpen: boolean;
   header: string;
   onOpenChange: () => void;
}

const EditChatGroupModal = ({ isOpen, onOpenChange, header }: EditChatGroupModalProps) => {
   const [isDirty, setIsDirty] = useState(false);

   return (
      <Modal size={`md`} onOpenChange={onOpenChange} isOpen={isOpen}>
         <ModalContent className={`px-2 py-2`}>
            {(onClose) => (
               <Fragment>
                  <ModalHeader>{header}</ModalHeader>
                  <ModalBody className={`mt-2`}>
                     <Input
                        placeholder={`Write some cool description.`}
                        onValueChange={(value) => {
                           setIsDirty(true);
                        }}
                        size={`sm`}
                        radius={`sm`}
                        color={`default`}
                        variant={`flat`}
                        classNames={{
                           input: `text-xs pl-2`,
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
                        size={`sm`}
                        color={`danger`}
                        className={`px-4 h-7`}
                        onPress={onClose}
                     >
                        Cancel
                     </Button>
                     <Button
                        size={`sm`}
                        variant={`shadow`}
                        onPress={onClose}
                        isDisabled={!isDirty}
                        color={`default`}
                        className={`ml-2 px-4 h-7`}
                     >
                        Save changes
                     </Button>
                  </ModalFooter>
               </Fragment>
            )}
         </ModalContent>
      </Modal>
   );
};

export default EditChatGroupActionButton;

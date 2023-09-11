"use client";
import React, { Fragment, useState } from "react";
import TooltipButton from "@components/TooltipButton";
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

export interface EditChatGroupActionButtonProps {
   chatGroup: ChatGroupDetailsEntry;
}

const EditChatGroupActionButton = ({
   chatGroup,
}: EditChatGroupActionButtonProps) => {
   const { isOpen, onOpen, onOpenChange } = useDisclosure();
   const isPrivate = useIsChatGroupPrivate(chatGroup);
   const [isDirty, setIsDirty] = useState(false);

   return (
      <Fragment>
         <TooltipButton
            chipProps={{
               classNames: {
                  base: "h-9 w-9 px-0",
               },
            }}
            onClick={onOpenChange}
            icon={<Edit className={`stroke-default-400`} size={20} />}
            content={`Edit chat group`}
         />
         <Modal size={`md`} onOpenChange={onOpenChange} isOpen={isOpen}>
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
                           }}
                           size={`md`}
                           color={`default`}
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
                           size={`sm`}
                           color={`danger`}
                           className={``}
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
                           className={``}
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

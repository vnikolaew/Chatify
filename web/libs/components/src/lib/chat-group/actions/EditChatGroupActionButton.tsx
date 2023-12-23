"use client";
import React, { Fragment, useEffect, useState } from "react";
import { TooltipButton } from "libs/components/src/lib/common";
import { Edit, X } from "lucide-react";
import {
   Button, Image,
   Input,
   Modal,
   ModalBody,
   ModalContent,
   ModalFooter,
   ModalHeader, Textarea,
   useDisclosure,
} from "@nextui-org/react";
import { ChatGroupDetailsEntry } from "@openapi";
import { useTranslations } from "next-intl";
import { useEditChatGroupMutation } from "@web/api";
import { UploadIcon, ThrashIcon } from "@web/components";
import { useIsChatGroupPrivate, useSingleFileUpload } from "@web/hooks";

export interface EditChatGroupActionButtonProps {
   chatGroup: ChatGroupDetailsEntry;
}

const EditChatGroupActionButton = ({
                                      chatGroup,
                                   }: EditChatGroupActionButtonProps) => {
   const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure();
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
            icon={<Edit className={`stroke-white`} size={20} />}
            content={t(`EditChatGroup`)}
         />
         <EditChatGroupModal isOpen={isOpen} onOpenChange={onOpenChange} chatGroup={chatGroup} onClose={onClose} />
      </Fragment>
   );
};

interface EditChatGroupModalProps {
   isOpen: boolean;
   onOpenChange: () => void;
   chatGroup: ChatGroupDetailsEntry;
   onClose: () => void;
}

const EditChatGroupModal = ({ onClose, onOpenChange, isOpen, chatGroup }: EditChatGroupModalProps) => {
   const isPrivate = useIsChatGroupPrivate(chatGroup);
   const [newAbout, setNewAbout] = useState(() => chatGroup?.chatGroup?.about ?? ``);
   const { selectedFile, setSelectedFile, fileUrl, fileInputRef, normalizedFileName } = useSingleFileUpload();

   useEffect(() => {
      console.log(selectedFile);
   }, [selectedFile]);

   useEffect(() => setNewAbout(chatGroup?.chatGroup?.about!),
      [chatGroup?.chatGroup?.about]);

   const [isDirty, setIsDirty] = useState(false);
   const { mutateAsync: editChatGroup, isLoading: editLoading, error: editError } = useEditChatGroupMutation();

   async function handleSaveChanges(e: any): Promise<any> {
      await editChatGroup({ chatGroupId: chatGroup.chatGroup!.id!, about: newAbout, file: selectedFile ?? undefined });
      onClose();
   }

   return (
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
                        ? `group`
                        : chatGroup?.chatGroup?.name}
                  </ModalHeader>
                  <ModalBody className={`mt-2`}>
                     <Textarea
                        placeholder={`Think of a good one.`}
                        onValueChange={(value) => {
                           setIsDirty(true);
                           setNewAbout(value);
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
                     {!isPrivate && (
                        <div className={`flex items-center flex-col mt-2 gap-4`}>
                           <Input
                              className={`py-2 w-full`}
                              labelPlacement={"outside"}
                              size={`md`}
                              label={"Select a group picture:"}
                              variant={"flat"}
                              color={"primary"}
                              onClick={_ => fileInputRef.current?.click()}
                              classNames={{
                                 inputWrapper: "py-2 px-2",
                                 input: `cursor-pointer`,
                                 label: `text-foreground text-xs`,
                              }}
                              endContent={
                                 selectedFile && (
                                    <span
                                       onClick={_ => setSelectedFile(null)}
                                       className={`z-30 hover:bg-primary-200 p-1 rounded-md mr-1 cursor-pointer text-sm`}>
                                       <ThrashIcon
                                          className={`fill-danger-300 text-danger-500`}
                                          size={14} />
                                    </span>)
                              }
                              startContent={
                                 <Button
                                    onPress={(_) => fileInputRef.current?.click()}
                                    variant={"light"}
                                    color={"primary"}
                                    radius={"full"}
                                    size={"sm"}
                                    isIconOnly
                                 >
                                    <UploadIcon className={`fill-white`} size={20} />
                                 </Button>
                              }
                              type={`text`}
                              // isClearable
                              isReadOnly
                              placeholder={
                                 selectedFile ? normalizedFileName : " Upload an image"
                              }
                           />
                           {selectedFile && (
                              <div
                                 className={`w-full flex gap-4 flex-col items-center justify-center`}
                              >
                                 <Image
                                    shadow={"md"}
                                    className={`mx-auto`}
                                    radius={"md"}
                                    src={fileUrl}
                                    width={120}
                                    height={120}
                                 />
                              </div>
                           )}
                           <input
                              onChange={({ target: { files } }) => {
                                 setSelectedFile(files![0]);
                                 setIsDirty(true);
                              }}
                              name={"file"}
                              ref={fileInputRef}
                              hidden
                              type={"file"}
                           />
                        </div>
                     )}
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
   );
};
export default EditChatGroupActionButton;

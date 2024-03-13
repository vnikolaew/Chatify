"use client";
import React, { Fragment, useState } from "react";
import {
   Button,
   Divider, Input,
   Modal,
   ModalBody,
   ModalContent, ModalFooter,
   ModalHeader,
   Spinner,
   Tooltip,
   useDisclosure,
} from "@nextui-org/react";
import { useSlate } from "slate-react";
import { CustomEditor } from "./editor";
import { useTranslations } from "next-intl";
import BoldIcon from "../../../icons/BoldIcon";
import ItalicIcon from "../../../icons/ItalicIcon";
import StrikethroughIcon from "../../../icons/StrikethroughIcon";
import CodeIcon from "../../../icons/CodeIcon";
import { LinkIcon } from "lucide-react";

export interface MessageTextEditorToolbarProps {
}

const MessageTextEditorToolbar = ({}: MessageTextEditorToolbarProps) => {
   const editor = useSlate();
   const t = useTranslations(`MainArea.ChatMessages.MessageTextEditor.Popups`);

   return (
      <div
         className={`flex flex-col items-start gap-1 absolute left-3 top-3 w-full`}
      >
         <div className={`flex items-center gap-2 `}>
            <ToolbarButton
               onPress={(_) => CustomEditor.toggleBoldMark(editor)}
               isActive={editor.getMarks()?.bold}
               icon={<BoldIcon className={`fill-foreground`} size={16} />}
               text={t(`Bold`)}
            />
            <Divider
               orientation={"vertical"}
               className={`h-3 text-default-300 bg-default-300 rounded-full w-[0.5px]`}
            />
            <ToolbarButton
               isActive={editor.getMarks()?.italic}
               onPress={(_) => CustomEditor.toggleItalicMark(editor)}
               icon={<ItalicIcon className={`fill-foreground`} size={16} />}
               text={t(`Italic`)}
            />
            <Divider
               orientation={"vertical"}
               className={`h-3 text-default-300 bg-default-300 rounded-full w-[0.5px]`}
            />
            <ToolbarButton
               isActive={editor.getMarks()?.strikethrough}
               onPress={(_) => CustomEditor.toggleStrikethroughMark(editor)}
               icon={
                  <StrikethroughIcon className={`fill-foreground`} size={16} />
               }
               text={t(`Strikethrough`)}
            />
            <ToolbarButton
               isActive={editor.getMarks()?.code}
               onPress={(_) => CustomEditor.toggleCodeBlock(editor)}
               icon={<CodeIcon className={`stroke-foreground`} size={16} />}
               text={t(`Code`)}
            />
            <AddLinkToolbarButton onAdd={(text, href) => {
               CustomEditor.toggleLink(editor, href);
               editor.insertText(text);
               CustomEditor.toggleLink(editor, href);
               console.log(editor.children);
            }} />
         </div>
         <Divider
            className={`w-5/6 h-[.5px] rounded-full mx-2 bg-default-300`}
         />
      </div>
   );
};

interface ToolbarButtonProps {
   icon: React.ReactNode;
   text: string;
   onPress?: (e: any) => void;
   isActive?: boolean;
}

interface AddLinkToolbarButtonProps {
   onAdd: (text: string, href: string) => void;
}

const AddLinkToolbarButton = ({ onAdd }: AddLinkToolbarButtonProps) => {
   const { isOpen, onOpen, onOpenChange, onClose } = useDisclosure();
   const t = useTranslations(`MainArea.ChatMessages.MessageTextEditor.Popups`);

   const [text, setText] = useState(``);
   const [href, setHref] = useState(``);

   function handleAddLink() {
      onAdd(text, href);
   }

   function clearFields() {
      setText(``)
      setHref(``)
   }

   return (
      <Fragment>
         <ToolbarButton
            isActive={false}
            onPress={(_) => onOpen()}
            icon={<LinkIcon className={`stroke-foreground`} size={16} />}
            text={t(`Link`)}
         />
         <Modal onClose={onClose} isOpen={isOpen} onOpenChange={onOpenChange}>
            <ModalContent>
               {(onClose) => (
                  <Fragment>
                     <ModalHeader>Add Link</ModalHeader>
                     <ModalBody className={`!mt-2`}>
                        <Input
                           autoFocus
                           value={text}
                           onValueChange={setText}
                           isClearable
                           className={`!mt-4 w-full`}
                           classNames={{
                              label: `text-white`,
                              input: `text-xs`,
                           }}
                           size={`sm`}
                           placeholder={`Enter a text`}
                           variant={`faded`} color={`primary`} labelPlacement={`outside`} label={`Text`}
                           type={`text`} />
                        <Input
                           value={href}
                           onValueChange={setHref}
                           isClearable
                           className={`!mt-8 w-full`}
                           classNames={{
                              label: `text-white`,
                              input: `text-xs`,
                           }}
                           placeholder={`Enter a link`}
                           size={`sm`}
                           variant={`faded`} color={`primary`} labelPlacement={`outside`} label={`Link`}
                           type={`text`} />
                     </ModalBody>
                     <ModalFooter>
                        <Button
                           variant={`shadow`}
                           size={`sm`}
                           color={`default`}
                           className={`px-4`}
                           onPress={_ => {
                              clearFields()
                              onClose();
                           }}
                        >
                           Cancel
                        </Button>
                        <Button
                           isDisabled={!text?.length || !href?.length}
                           spinner={<Spinner className={`self-center`} classNames={{
                              circle1: `h-4 w-4`,
                              circle2: `h-4 w-4`,
                           }} size={`sm`} color={`white`} />}
                           size={`sm`}
                           variant={`shadow`}
                           onPress={() => {
                              handleAddLink();
                              clearFields()
                              onClose();
                           }}
                           color={`primary`}
                           className={`ml-2 px-4 `}
                        >
                           Save
                        </Button>
                     </ModalFooter>
                  </Fragment>
               )}
            </ModalContent>
         </Modal>
      </Fragment>
   );
};

const ToolbarButton = ({
                          icon,
                          text,
                          onPress,
                          isActive,
                       }: ToolbarButtonProps) => {
   return (
      <Tooltip
         delay={100}
         offset={1}
         classNames={{
            base: `px-4 py-0`,
         }}
         closeDelay={100}
         radius={"sm"}
         content={<span className={`text-xs`}>{text} </span>}
         showArrow
         color={"default"}
         size={"sm"}
      >
         <Button
            onPress={onPress}
            startContent={icon}
            className={`h-7 w-7 p-1 ${isActive ? `bg-default-300` : ``}`}
            size={"sm"}
            variant={"light"}
            isIconOnly
         />
      </Tooltip>
   );
};
export default MessageTextEditorToolbar;

"use client";
import React, { useCallback, useMemo, useRef, useState } from "react";
import { DefaultElement, Editable, Slate, withReact } from "slate-react";
import { createEditor, Text, Transforms } from "slate";
import {
   Badge,
   Button,
   Chip,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Image,
   Link,
   Spacer,
   Spinner,
   Tooltip,
} from "@nextui-org/react";
import { RightArrow } from "@icons";
import UploadIcon from "@components/icons/UploadIcon";
import { ChatGroup } from "@openapi";
import MessageTextEditorToolbar from "@components/chat-group/messages/editor/MessageTextEditorToolbar";
import { useSendGroupChatMessageMutation } from "@web/api";
import { useCurrentChatGroup } from "@hooks";
import * as escaper from "html-escaper";
import { plateToMarkdown } from "slate-mark";
import { v4 as uuidv4 } from "uuid";

import { CustomEditor } from "./editor";
import CrossIcon from "@components/icons/CrossIcon";
import { Space } from "lucide-react";

export class ChatifyFile {
   public readonly id: string;
   public readonly file: File;

   constructor(file: File, id: string) {
      this.file = file;
      this.id = id;
   }
}

export interface MessageTextEditorProps {
   chatGroup: ChatGroup;
}

export enum MessageAction {
   FileUpload = "FileUpload ",
}

function serializer(node: any) {
   if (Text.isText(node)) {
      let string = escaper.escape(node.text);
      if (node.bold) {
         string = `<strong>${string}</strong>`;
      }
      if (node.italic) {
         string = `<i>${string}</i>`;
      }
      if (node.strikethrough) {
         string = `<s>${string}</s>`;
      }
      return string;
   }

   const children = node.children.map((n) => serializer(n)).join("");

   switch (node.type) {
      case "quote":
         return `<blockquote><p>${children}</p></blockquote>`;
      case "paragraph":
         return `<p>${children}</p>`;
      case "link":
         return `<a href="${escaper.escape(node.url)}">${children}</a>`;
      default:
         return children;
   }
}

// Define a React component renderer for our code blocks.
const CodeElement = (props) => {
   return (
      <pre {...props.attributes}>
         <code>{props.children}</code>
      </pre>
   );
};

// Define a React component to render leaves with bold text.
const Leaf = (props) => {
   if (props.leaf.type === "link") {
      return (
         <Link
            underline={"hover"}
            color={"primary"}
            className={`cursor-pointer`}
            size={"sm"}
            href={props.leaf.href}
         >
            {props.children}
         </Link>
      );
   }
   return (
      <span
         {...props.attributes}
         style={{
            fontWeight: props.leaf.bold ? "bold" : "normal",
            fontStyle: props.leaf.italic ? "italic" : "normal",
            textDecoration: props.leaf.strikethrough
               ? "line-through"
               : "normal",
         }}
      >
         {props.children}
      </span>
   );
};

function getTextFromNode(node) {
   if (node.children) {
      return node.children
         .filter((n) => n.tag !== "ignore")
         .map((childNode) => getTextFromNode(childNode))
         .join("");
   } else if (node.text) {
      return node.text;
   } else {
      return "";
   }
}

const MessageTextEditor = ({ chatGroup }: MessageTextEditorProps) => {
   const [editor] = useState(() => withReact(createEditor()));
   const groupId = useCurrentChatGroup();
   const [attachedFiles, setAttachedFiles] = useState<ChatifyFile[]>([]);

   // Mapping of File ID -> File Object URL
   const attachedFilesUrls = useMemo<Map<string, string>>(
      () =>
         new Map<string, string>(
            attachedFiles.map((file) => [
               file.id,
               URL.createObjectURL(file.file),
            ])
         ),
      [attachedFiles]
   );
   console.log(attachedFilesUrls);

   const fileUploadRef = useRef<HTMLInputElement>(null!);

   const [disableSendMessageButton, setDisableSendMessageButton] =
      useState(true);
   const {
      mutateAsync: sendMessage,
      isLoading,
      error,
   } = useSendGroupChatMessageMutation();

   // Define a rendering function based on the element passed to `props`. We use
   // `useCallback` here to memoize the function for subsequent renders.
   const renderElement = useCallback((props) => {
      switch (props.element.type) {
         case "code":
            return <CodeElement {...props} />;
         case "link":
            return (
               <Link
                  underline={"hover"}
                  size={"sm"}
                  className={`cursor-pointer inline`}
                  color={"primary"}
                  href={props.element.href}
                  {...props}
               >
                  Link
               </Link>
            );
         default:
            return <DefaultElement {...props} />;
      }
   }, []);

   const initialValue = useMemo(
      () => [
         {
            type: "paragraph",
            children: [{ text: "" }],
         },
      ],
      []
   );
   // Define a leaf rendering function that is memoized with `useCallback`.
   const renderLeaf = useCallback((props) => {
      return <Leaf {...props} />;
   }, []);

   async function handleSendMessage() {
      console.log(getTextFromNode(editor));
      console.log(editor.children);

      const content = plateToMarkdown(editor.children);
      await sendMessage(
         {
            content,
            chatGroupId: groupId,
            files: attachedFiles.map((_) => _.file),
         },
         {
            onSuccess: (data, vars, context) => {
               CustomEditor.clear(editor);
               setAttachedFiles([]);
            },
         }
      );
   }

   const handleFileUpload: React.ChangeEventHandler<HTMLInputElement> = async ({
      target: { files },
   }) => {
      console.log(files);
      const newFiles = Array.from({ length: files.length }).map((_, i) =>
         files.item(i)
      );
      setAttachedFiles((files) => [
         ...files,
         ...newFiles.map((_) => new ChatifyFile(_, uuidv4())),
      ]);
   };

   return (
      <Slate
         onChange={(value) => {
            if (CustomEditor.isVoid(editor)) setDisableSendMessageButton(true);
            else setDisableSendMessageButton(false);
         }}
         editor={editor}
         initialValue={initialValue}
      >
         <div className={`relative h-fit w-5/6 mr-12`}>
            <Editable
               placeholder={
                  chatGroup?.name && `Message in ${chatGroup?.name} ...`
               }
               className={`bg-zinc-900 !break-words !whitespace-nowrap ${
                  attachedFilesUrls?.size ? `min-h-[180px]` : `min-h-[140px]`
               }  h-auto relative text-medium px-6 pt-14 rounded-medium text-white border-default-200 border-1 !active:border-default-300 !focus:border-default-300`}
               renderElement={renderElement}
               renderLeaf={renderLeaf}
               onKeyDown={(e) => {
                  if (e.key === " ") {
                     e.preventDefault();
                     Transforms.insertText(editor, `\u00a0`);
                     return;
                  }

                  if (!e.ctrlKey) return;

                  switch (e.key) {
                     case "`": {
                        e.preventDefault();
                        CustomEditor.toggleCodeBlock(editor);
                        break;
                     }
                     case "b": {
                        e.preventDefault();
                        CustomEditor.toggleBoldMark(editor);
                        break;
                     }
                     case "i": {
                        e.preventDefault();
                        CustomEditor.toggleItalicMark(editor);
                        break;
                     }
                     case "X": {
                        e.preventDefault();
                        CustomEditor.toggleStrikethroughMark(editor);
                        break;
                     }
                  }
               }}
            />
            <MessageTextEditorToolbar />
            <input
               name={"file-upload"}
               onChange={handleFileUpload}
               ref={fileUploadRef}
               hidden
               multiple
               type={"file"}
            />
            <div
               className={`items-end gap-2 flex absolute z-10 left-3 bottom-3`}
            >
               <Dropdown size={`sm`} placement={"top"}>
                  <DropdownTrigger className={` `}>
                     <Button
                        variant={"shadow"}
                        className={`text-foreground p-0`}
                        color={"primary"}
                        radius={"full"}
                        size={"sm"}
                        startContent={
                           <span className={`fill-foreground text-medium`}>
                              +
                           </span>
                        }
                        isIconOnly
                     />
                  </DropdownTrigger>
                  <DropdownMenu
                     onAction={(key) => {
                        if (key === MessageAction.FileUpload) {
                           fileUploadRef.current.click();
                        }
                     }}
                     variant={"flat"}
                     color={"default"}
                  >
                     <DropdownItem
                        key={MessageAction.FileUpload}
                        description={
                           <span>
                              Choose one or more <br /> from your device
                           </span>
                        }
                        startContent={
                           <UploadIcon
                              className={`fill-foreground`}
                              size={20}
                           />
                        }
                     >
                        Upload File
                     </DropdownItem>
                  </DropdownMenu>
               </Dropdown>
               <Spacer y={4} />
               {[...attachedFilesUrls?.entries()].map(([id, url], i) => (
                  <div key={id} className={`flex flex-col items-center gap-2`}>
                     <Badge
                        size={"sm"}
                        classNames={{
                           badge: `w-3 h-3 m-0 p-0`,
                        }}
                        content={
                           <Tooltip
                              closeDelay={100}
                              disableAnimation
                              delay={100}
                              color={"default"}
                              size={"sm"}
                              classNames={{
                                 base: `px-2 py-0`,
                              }}
                              showArrow
                              content={
                                 <span className={`text-[.6rem]`}>
                                    Remove file
                                 </span>
                              }
                           >
                              <Button
                                 variant={"shadow"}
                                 color={"default"}
                                 onPress={(_) => {
                                    setAttachedFiles((files) =>
                                       files.filter((f) => f.id !== id)
                                    );
                                 }}
                                 className={`!w-fit hover:bg-zinc-900 !min-w-fit m-0 px-1 h-4`}
                                 type={"button"}
                                 size={"sm"}
                                 radius={"full"}
                                 startContent={
                                    <CrossIcon
                                       className={`stroke-foreground fill-transparent `}
                                       size={10}
                                    />
                                 }
                                 isIconOnly
                              />
                           </Tooltip>
                        }
                        key={i}
                        color={"default"}
                     >
                        <Image
                           height={40}
                           width={40}
                           shadow={"md"}
                           radius={"md"}
                           src={url}
                        />
                     </Badge>
                     <Chip
                        variant={"flat"}
                        color={"warning"}
                        size={"sm"}
                        classNames={{
                           base: `px-1 h-4`,
                        }}
                        className={`text-[.6rem] px-1`}
                     >
                        {attachedFiles.find((f) => f.id === id)!.file.name}
                     </Chip>
                  </div>
               ))}
            </div>
            <div
               className={`flex z-10 absolute bottom-3 right-4 items-center gap-2`}
            >
               <Tooltip
                  showArrow
                  delay={100}
                  closeDelay={100}
                  offset={5}
                  size={"sm"}
                  placement={"top"}
                  color={"default"}
                  content={""}
               >
                  <Button
                     color={"primary"}
                     variant={`shadow`}
                     isLoading={isLoading}
                     spinner={<Spinner color={"white"} size={"sm"} />}
                     onPress={handleSendMessage}
                     isDisabled={disableSendMessageButton}
                     className={`z-10 items-center !gap-2 pr-2 text-white`}
                     size={"md"}
                     {...(!isLoading
                        ? {
                             endContent: (
                                <RightArrow
                                   className={`fill-white group-hover:fill-white`}
                                   size={20}
                                />
                             ),
                          }
                        : {})}
                  >
                     {isLoading ? "Sending" : "Send"}
                  </Button>
               </Tooltip>
            </div>
         </div>
      </Slate>
   );
};

export default MessageTextEditor;

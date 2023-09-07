"use client";
import React, {
   useCallback,
   useEffect,
   useMemo,
   useRef,
   useState,
} from "react";
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
import { ChatGroup, ChatGroupDetailsEntry } from "@openapi";
import MessageTextEditorToolbar from "@components/chat-group/messages/editor/MessageTextEditorToolbar";
import { useSendGroupChatMessageMutation } from "@web/api";
import { useCurrentChatGroup, useIsChatGroupPrivate } from "@hooks";
import * as escaper from "html-escaper";
import { plateToMarkdown } from "slate-mark";
import slate from "remark-slate";

import { CustomEditor } from "./editor";
import { markdownProcessor } from "../../../../utils";
import { unified } from "unified";
import markdown from "remark-parse";
import { useChatifyClientContext } from "../../../../hub/ChatHubConnection";
import { useFileUpload } from "@hooks";
import ChatMessageAttachmentEntry from "@components/chat-group/messages/editor/ChatMessageAttachmentEntry";

export class ChatifyFile {
   public readonly id: string;
   public readonly file: File;

   constructor(file: File, id: string) {
      this.file = file;
      this.id = id;
   }
}

export interface MessageTextEditorProps {
   chatGroup: ChatGroupDetailsEntry;
   placeholder?: string;
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

const MessageTextEditor = ({
   chatGroup,
   placeholder,
}: MessageTextEditorProps) => {
   const [editor] = useState(() => withReact(createEditor()));
   const hubClient = useChatifyClientContext();
   const [isUserTyping, setIsUserTyping] = useState(false);
   const {
      attachedFilesUrls,
      attachedFiles,
      fileUploadRef,
      handleFileUpload,
      handleRemoveFile,
      clearFiles,
   } = useFileUpload();
   const groupId = useCurrentChatGroup();

   useEffect(() => {
      if (isUserTyping) {
         hubClient
            .startTypingInGroupChat(groupId)
            .then(console.log)
            .catch(console.error);
      } else {
         hubClient
            .stopTypingInGroupChat(groupId)
            .then(console.log)
            .catch(console.error);
      }
   }, [isUserTyping]);
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
      console.log(`Markdown: ${content}`);
      console.log(markdownProcessor.processSync(content).value as string);

      unified()
         .use(markdown as any)
         .use(slate)
         .process(content, (err, data) => {
            console.log({ data: data.result });
         });
      await sendMessage(
         {
            content: markdownProcessor.processSync(content).value as string,
            chatGroupId: groupId,
            files: attachedFiles.map((_) => _.file),
         },
         {
            onSuccess: (data, vars, context) => {
               CustomEditor.clear(editor);
               clearFiles();
            },
         }
      );
   }

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
               placeholder={placeholder}
               className={`bg-zinc-900 !break-words !whitespace-nowrap ${
                  attachedFilesUrls?.size
                     ? `!min-h-[180px] !max-h-[180px]`
                     : `!min-h-[140px] !max-h-[140px]`
               } relative text-medium px-6 pt-14 rounded-medium text-white border-default-200 border-1 !active:border-default-300 !focus:border-default-300`}
               renderElement={renderElement}
               renderLeaf={renderLeaf}
               onKeyDown={(e) => {
                  if (e.key === " ") {
                     e.preventDefault();
                     Transforms.insertText(editor, `\u00a0`);
                     return;
                  }
                  if (CustomEditor.isVoid(editor) && isUserTyping) {
                     setIsUserTyping(false);
                  } else if (!isUserTyping) setIsUserTyping(true);

                  console.log(CustomEditor.isVoid(editor));

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
                  <ChatMessageAttachmentEntry
                     attachment={attachedFiles.find((_) => _.id === id)!}
                     url={url}
                     onRemove={() => handleRemoveFile(id)}
                  />
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

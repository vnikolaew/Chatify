"use client";
import React, { forwardRef, HTMLAttributes, useCallback, useEffect, useMemo, useState } from "react";
import { DefaultElement, Editable, Slate, withReact } from "slate-react";
import { createEditor, Text, Transforms } from "slate";
import {
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Link,
   Spacer,
   Spinner,
   Tooltip,
} from "@nextui-org/react";
import { ChatGroupDetailsEntry } from "@openapi";
import {
   useDraftChatMessage,
   useGetDraftedMessageForGroup,
   useSendGroupChatMessageMutation,
} from "@web/api";
import { useCurrentChatGroup, useCurrentUserId, useIsChatGroupPrivate, useOnWindowLocationChange } from "@web/hooks";
import * as escaper from "html-escaper";
import { plateToMarkdown } from "slate-mark";

import { CodeElement, CustomEditor, Leaf } from "./editor";
import { markdownProcessor } from "@web/utils";
import { useChatifyClientContext } from "apps/app/hub/ChatHubConnection";
import { useFileUpload } from "@web/hooks";
import ChatMessageAttachmentEntry from "./ChatMessageAttachmentEntry";
import { unified } from "unified";
import markdown from "remark-parse";
import slate from "remark-slate";
import { useTranslations } from "next-intl";
import { ChatGroupChangedEvent } from "apps/app/app/[locale]/(c)/MainLayout";
import { RightArrow, UploadIcon } from "@web/components";
import { useReplyToMessageContext } from "../ChatMessagesSection";
import ReplyToMessagePreview from "./ReplyToMessagePreview";
import MessageTextEditorToolbar from "./MessageTextEditorToolbar";

export class ChatifyFile {
   public readonly id: string;
   public readonly file: File;

   constructor(file: File, id: string) {
      this.file = file;
      this.id = id;
   }
}

export interface MessageTextEditorProps extends HTMLAttributes<HTMLDivElement> {
   chatGroup: ChatGroupDetailsEntry;
   initialContent?: string;
   initialAttachments?: Map<string, ChatifyFile>; // File URL -> File instance
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
      if (node.code) {
         string = `<code>${string}</code>`;
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

const MessageTextEditor = forwardRef<HTMLDivElement, MessageTextEditorProps>(({
                                                                                 chatGroup,
                                                                                 initialAttachments,
                                                                                 initialContent,
                                                                                 className,
                                                                                 ...props
                                                                              }, ref) => {
   const [editor] = useState(() => withReact(createEditor()));
   const editorLines = useCallback(() =>
         editor.children
            .filter(n => n.type === `line-break`)
            .length,
      [editor]);

   const [replyTo] = useReplyToMessageContext();
   const hubClient = useChatifyClientContext();
   const [isUserTyping, setIsUserTyping] = useState(false);
   const {
      attachedFilesUrls,
      attachedFiles,
      fileUploadRef,
      handleFileUpload,
      handleRemoveFile,
      clearFiles,
   } = useFileUpload([...(initialAttachments?.values() ?? [])]);

   const attachedFilesMap = useMemo<Record<string, ChatifyFile>>(() =>
      attachedFiles.reduce((acc, file) =>
         ({
            ...acc,
            [file.id]: file,
         }), {}), [attachedFiles]);

   const t = useTranslations(`MainArea.ChatMessages.MessageTextEditor`);
   const isPrivate = useIsChatGroupPrivate(chatGroup);

   const meId = useCurrentUserId();
   const t2 = useTranslations(`MainArea.ChatMessages`);
   const placeholder = useMemo(() => {
      return isPrivate ?
         t2(`MessageTextEditor.MessagePrivatePlaceholder`, {
            name: chatGroup?.members?.find(
               (_) => _.id !== meId,
            )?.username,
         }) : t2(`MessageTextEditor.MessagePlaceholder`, { group: chatGroup?.chatGroup.name });
   }, [isPrivate, t2, chatGroup?.members, meId, chatGroup?.chatGroup.name]);

   const groupId = useCurrentChatGroup();
   const {
      data: draftedMessage,
      error: draftMessageError,
      isLoading: draftMessageLoading,
   } = useGetDraftedMessageForGroup(groupId, {
      suspense: true,
      refetchOnMount: `always`,
      onError: console.error,
   });

   const {
      mutateAsync: draftChatMessage,
      isLoading: draftLoading,
      error: draftError,
   } = useDraftChatMessage();

   useOnWindowLocationChange(async (e: ChatGroupChangedEvent) => {
      await handleDraftMessage(e.from);
   });

   useEffect(() => CustomEditor.placeCursorAtEnd(editor), []);

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
      console.log({ props });
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
                  {props.element.children[0].text}
               </Link>
            );
         default:
            return <DefaultElement {...props} />;
      }
   }, []);

   const initialValue = useMemo(() => {
      if (!draftedMessage && !initialContent) {
         return [
            {
               type: "paragraph",
               children: [{ text: "" }],
            },
         ];
      } else {
         const result = unified()
            //@ts-ignore
            .use(markdown)
            .use(slate)
            .processSync(initialContent ?? draftedMessage.content);
         return result.result;
      }
   }, [draftedMessage, initialContent]);

   // Define a leaf rendering function that is memoized with `useCallback`.
   const renderLeaf = useCallback((props) => {
      return <Leaf {...props} />;
   }, []);

   async function handleSendMessage() {
      const content = plateToMarkdown(editor.children);
      CustomEditor.clear(editor);

      console.log(`Content: ${content}`);
      const result = unified().use(markdown).use(slate).processSync(content);
      const result2 = unified()
         .use(markdown)
         .use(slate)
         .processSync(`**sdfsfsfs**`);

      console.log(result.result);
      console.log(result2);

      await sendMessage(
         {
            content: markdownProcessor.processSync(content).value as string,
            chatGroupId: groupId,
            files: attachedFiles.map((_) => _.file),
         },
         {
            onSuccess: (_, {}) => {
               clearFiles();
               CustomEditor.clear(editor);
            },
            onSettled: () => {
               setIsUserTyping(false);
            },
         },
      );
   }

   const handleDraftMessage = useCallback(async (groupId: string) => {
         const content = plateToMarkdown(editor.children);
         CustomEditor.clear(editor);

         if (content?.trim()?.length === 0) return;

         await draftChatMessage(
            {
               content: markdownProcessor.processSync(content).value as string,
               chatGroupId: groupId,
               files: attachedFiles.map((_) => _.file),
            },
            {
               onSuccess: (_, {}) => clearFiles(),
               onSettled: () => setIsUserTyping(false),
            },
         );
      }
      ,
      [editor, draftChatMessage, attachedFiles, clearFiles],
   );

   const [editorHeight, setEditorHeight]
      = useState(140);

   function handleInsertLineBreak() {
      CustomEditor.insertLineBreak(editor)

      // Re-calculate editor height:
      setEditorHeight(h => h + 20);
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
         <div ref={ref} className={`relative h-fit w-5/6 mr-12 ${className ?? ``}`} {...props}>
            <Editable
               style={{
                  minHeight: `${editorHeight + (attachedFilesUrls?.size ? 60 : 0)}px`,
                  maxHeight: `${editorHeight + (attachedFilesUrls?.size ? 60 : 0)}px`,
               }}
               placeholder={replyTo ? `Reply ...` : placeholder}
               className={`bg-zinc-900 resize-y !break-words !whitespace-nowrap relative text-sm px-6 pt-14 rounded-medium text-white border-default-200 border-1 !active:border-default-300 !focus:border-default-300`}
               autoFocus
               renderElement={renderElement}
               renderLeaf={renderLeaf}
               onKeyDown={async (e) => {
                  if (e.key === "Enter") {
                     e.preventDefault();
                     if (e.shiftKey) {
                        handleInsertLineBreak();
                        return;
                     }

                     await handleSendMessage();
                     return;
                  }

                  if (e.key === " ") {
                     e.preventDefault();
                     Transforms.insertText(editor, `\u00a0`);
                     return;
                  }
                  if (CustomEditor.isVoid(editor) && isUserTyping) {
                     setIsUserTyping(false);
                  } else if (!isUserTyping) setIsUserTyping(true);

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
            {replyTo && (
               <ReplyToMessagePreview replyToId={replyTo} />
            )}
            <MessageTextEditorToolbar />
            <input
               name={"file-upload"}
               onChange={handleFileUpload}
               ref={fileUploadRef}
               hidden
               multiple
               type={"file"}
               accept={`*/*`}
            />
            <div
               className={`items-end gap-2 flex absolute z-10 mt-2 left-3 bottom-3`}
            >
               <Dropdown size={`sm`} placement={"top"}>
                  <DropdownTrigger className={` `}>
                     <Button
                        variant={"shadow"}
                        className={`text-foreground !min-w-fit !max-w-fit m-0 !p-2 h-6 w-6`}
                        color={"primary"}
                        radius={"full"}
                        startContent={
                           <span className={`fill-foreground text-sm`}>
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
                           <span dangerouslySetInnerHTML={{ __html: t(`FileUpload.Description`) }}
                                 className={`text-[.6rem] mt-1 leading-3`}>
                           </span>
                        }
                        classNames={{
                           description: `mt-1`,
                           title: `text-md`,
                        }}
                        startContent={
                           <UploadIcon
                              className={`fill-foreground`}
                              size={20}
                           />
                        }
                     >
                        {t(`FileUpload.Title`)}
                     </DropdownItem>
                  </DropdownMenu>
               </Dropdown>
               <Spacer y={4} />
               {[...attachedFilesUrls?.entries()].map(([id, url], i) => (
                  <ChatMessageAttachmentEntry
                     attachment={attachedFilesMap[id]!}
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
                     className={`z-10 items-center !gap-2 pr-2 px-3 text-white`}
                     size={"sm"}
                     endContent={!isLoading ? <RightArrow
                        className={`fill-white group-hover:fill-white`}
                        size={20}
                     /> : undefined}
                  >
                     {isLoading ? t("Sending") : t("Send")}
                  </Button>
               </Tooltip>
            </div>
         </div>
      </Slate>
   );
});

export default MessageTextEditor;

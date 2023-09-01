"use client";
import React, { useCallback, useMemo, useState } from "react";
import {
   DefaultElement,
   Editable,
   ReactEditor,
   Slate,
   useSlate,
   withReact,
} from "slate-react";
import {
   createEditor,
   Editor,
   Transforms,
   Element,
   BaseEditor,
   Range,
} from "slate";
import {
   Button,
   ButtonGroup,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Link,
   Tooltip,
} from "@nextui-org/react";
import renderer from "slate-md-serializer/lib/renderer";
import { RightArrow } from "@icons";
import UploadIcon from "@components/icons/UploadIcon";
import { ChatGroup } from "@openapi";
import MessageTextEditorToolbar from "@components/chat-group/messages/editor/MessageTextEditorToolbar";
import { useSendGroupChatMessageMutation } from "@web/api";
import { useCurrentChatGroup } from "@hooks";

console.log(renderer.serialize);

export interface MessageTextEditorProps {
   chatGroup: ChatGroup;
}

export const CustomEditor = {
   isBoldMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.bold ?? false;
   },

   isItalicMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.italic ?? false;
   },

   isCodeBlockActive(editor: BaseEditor & ReactEditor) {
      const [match] = Editor.nodes(editor, {
         match: (n) => n.type === "code",
      });

      return !!match;
   },
   serialize(node: any): string {
      if (Editor.isEditor(node)) {
         const children = node.children.map(CustomEditor.serialize);
         return children.join("");
      }

      if (Array.isArray(node.children)) {
         console.log(node);
         const children = node?.children?.map(CustomEditor.serialize).join("");
         switch (node.type) {
            case "paragraph":
               return `\n${children}\n`;
            case "heading-one":
               return `# ${children}\n`;
            case "heading-two":
               return `## ${children}\n`;
            // Add more cases for other element types if needed
            default:
               return children;
         }
      }
      if (node.bold) return `<b>${node.text}</b>`;

      if ("text" in node) {
         return node.text;
      }

      return "";
   },

   clear(editor: BaseEditor & ReactEditor) {
      editor.removeNodes({ match: (n) => true });
   },

   isVoid(editor: BaseEditor & ReactEditor) {
      return !(
         // editor.children.length &&
         (editor.children[0] as Element)?.children[0]?.text?.length
      );
   },

   toggleBoldMark(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isBoldMarkActive(editor);
      if (isActive) {
         Editor.removeMark(editor, "bold");
      } else {
         Editor.addMark(editor, "bold", true);
      }
   },

   toggleItalicMark(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isItalicMarkActive(editor);
      if (isActive) {
         Editor.removeMark(editor, "italic");
      } else {
         Editor.addMark(editor, "italic", true);
      }
   },

   toggleCodeBlock(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isCodeBlockActive(editor);
      Transforms.setNodes(
         editor,
         { type: isActive ? null : "code" },
         { match: (n) => Editor.isBlock(editor, n) }
      );
   },

   toggleStrikethroughMark(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isStrikethroughMarkActive(editor);
      if (isActive) {
         Editor.removeMark(editor, "strikethrough");
      } else {
         Editor.addMark(editor, "strikethrough", true);
      }
   },
   isStrikethroughMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.strikethrough ?? false;
   },

   addLink(editor: BaseEditor & ReactEditor, title: string, href: string) {
      const link = {
         type: "link",
         href,
         children: [{ text: title || href }],
      };

      Transforms.insertNodes(editor, link);
      Transforms.collapse(editor, { edge: "end" });
   },
};

CustomEditor.serialize.bind(CustomEditor);

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
   console.log(props);
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
      console.log(props);

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
      await sendMessage({
         content: getTextFromNode(editor),
         chatGroupId: groupId,
      });
   }

   return (
      <Slate
         onChange={(value) => {
            const isAstChange = editor.operations.some(
               (op) => "set_selection" !== op.type
            );
            if (CustomEditor.isVoid(editor)) setDisableSendMessageButton(true);
            else setDisableSendMessageButton(false);

            if (isAstChange) {
               const content = JSON.stringify(value);
               localStorage.setItem("content", content);
            }
         }}
         editor={editor}
         initialValue={initialValue}
      >
         <div className={`relative h-fit w-full`}>
            <Editable
               placeholder={`Message in ${chatGroup?.name}`}
               className={`bg-zinc-900 min-h-[140px] relative text-medium px-4 pt-12 rounded-medium text-white border-default-200 border-1 !active:border-default-300 !focus:border-default-300`}
               renderElement={renderElement}
               renderLeaf={renderLeaf}
               onKeyDown={(e) => {
                  if (!e.ctrlKey) return;
                  console.log(e.key, e.ctrlKey);

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
            <Dropdown size={`sm`} placement={"top"}>
               <DropdownTrigger className={`absolute z-10 left-3 bottom-3 `}>
                  <Button
                     variant={"shadow"}
                     className={`text-foreground p-0`}
                     color={"primary"}
                     radius={"full"}
                     size={"sm"}
                     startContent={
                        <span className={`fill-foreground text-medium`}>+</span>
                     }
                     isIconOnly
                  />
               </DropdownTrigger>
               <DropdownMenu
                  onAction={(key) => {}}
                  variant={"flat"}
                  color={"default"}
               >
                  <DropdownItem
                     description={"Description"}
                     startContent={
                        <UploadIcon className={`fill-foreground`} size={20} />
                     }
                  >
                     Upload File
                  </DropdownItem>
               </DropdownMenu>
            </Dropdown>
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
                     variant={"solid"}
                     onPress={handleSendMessage}
                     isDisabled={disableSendMessageButton}
                     className={`z-10 items-center px-4 !gap-1 pr-2 text-white`}
                     size={"sm"}
                     endContent={
                        <RightArrow className={`fill-foreground`} size={24} />
                     }
                  >
                     Send
                  </Button>
               </Tooltip>
            </div>
         </div>
      </Slate>
   );
};

const Toolbar = () => {
   const editor = useSlate();
   return <ButtonGroup size={"sm"}></ButtonGroup>;
};

export default MessageTextEditor;

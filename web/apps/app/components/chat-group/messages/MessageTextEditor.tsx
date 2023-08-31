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
   NodeInterface,
   Node,
} from "slate";
import {
   Button,
   ButtonGroup,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Tooltip,
} from "@nextui-org/react";
import { RightArrow } from "@icons";
import UploadIcon from "@components/icons/UploadIcon";
import { ChatGroup } from "@openapi";

export interface MessageTextEditorProps {
   chatGroup: ChatGroup;
}

const CustomEditor = {
   isBoldMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks ? marks.bold === true : false;
   },

   isCodeBlockActive(editor: BaseEditor & ReactEditor) {
      const [match] = Editor.nodes(editor, {
         match: (n) => n.type === "code",
      });

      return !!match;
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

   toggleCodeBlock(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isCodeBlockActive(editor);
      Transforms.setNodes(
         editor,
         { type: isActive ? null : "code" },
         { match: (n) => Editor.isBlock(editor, n) }
      );
   },
};

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
   return (
      <span
         {...props.attributes}
         style={{ fontWeight: props.leaf.bold ? "bold" : "normal" }}
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
   const [disableSendMessageButton, setDisableSendMessageButton] =
      useState(true);
   console.log("Is disabled: ", disableSendMessageButton);

   // Define a rendering function based on the element passed to `props`. We use
   // `useCallback` here to memoize the function for subsequent renders.
   const renderElement = useCallback((props) => {
      switch (props.element.type) {
         case "code":
            return <CodeElement {...props} />;
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

   console.log(editor.children);
   return (
      <Slate
         onChange={(value) => {
            const isAstChange = editor.operations.some(
               (op) => "set_selection" !== op.type
            );
            console.log("Is void: ", CustomEditor.isVoid(editor));
            if (CustomEditor.isVoid(editor)) setDisableSendMessageButton(true);
            else setDisableSendMessageButton(false);

            console.log(value);
            if (isAstChange) {
               const content = JSON.stringify(value);
               localStorage.setItem("content", content);
            }
         }}
         editor={editor}
         initialValue={initialValue}
      >
         <div className={`flex my-4 items-center gap-8`}>
            <Toolbar />
         </div>
         <div className={`relative`}>
            <Editable
               placeholder={`Message in ${chatGroup?.name}`}
               className={`bg-zinc-900 min-h-[50px] relative text-medium px-4 pl-16 py-4 rounded-medium text-white border-white border-1`}
               renderElement={renderElement}
               renderLeaf={renderLeaf}
               onKeyDown={(e) => {
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
                  }
               }}
            />
            <Dropdown size={`sm`} placement={"top"}>
               <DropdownTrigger
                  className={`absolute z-10 -translate-y-1/2 left-4 top-1/2 `}
               >
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
               <DropdownMenu variant={"flat"} color={"default"}>
                  <DropdownItem
                     description={"Description"}
                     startContent={
                        <UploadIcon className={`fill-foreground`} size={20} />
                     }
                  >
                     {" "}
                     Upload File
                  </DropdownItem>
               </DropdownMenu>
            </Dropdown>
            <div
               className={`flex z-10 absolute -translate-y-1/2 top-1/2 right-4 items-center gap-2`}
            >
               <Tooltip
                  showArrow
                  delay={100}
                  closeDelay={100}
                  offset={5}
                  size={"sm"}
                  placement={"top"}
                  color={"default"}
                  content={"Send message"}
               >
                  <Button
                     color={"primary"}
                     variant={"flat"}
                     onPress={(_) =>
                        console.log(getTextFromNode(editor), editor.children)
                     }
                     isDisabled={disableSendMessageButton}
                     className={`z-10 bg-default-300 items-center px-0 !gap-0 text-white `}
                     size={"sm"}
                     isIconOnly
                     startContent={
                        <RightArrow className={`fill-foreground`} size={24} />
                     }
                  />
               </Tooltip>
               <Button
                  isDisabled={CustomEditor.isVoid(editor)}
                  isIconOnly
                  color={"danger"}
                  size={"sm"}
                  onPress={(_) => {
                     CustomEditor.clear(editor);
                  }}
               >
                  X
               </Button>
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

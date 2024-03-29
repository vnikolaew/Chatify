import { BaseEditor, Editor, Element, Transforms } from "slate";
import { ReactEditor } from "slate-react";
import { Chip, Link } from "@nextui-org/react";
import React from "react";

export const CustomEditor = {
   isBoldMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.bold ?? false;
   },

   isItalicMarkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.italic ?? false;
   },

   isLinkActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.link ?? false;
   },

   isUserMentionActive(editor: BaseEditor & ReactEditor) {
      const marks = Editor.marks(editor);
      return marks?.[`user-mention`] ?? false;
   },

   isCodeBlockActive(editor: BaseEditor & ReactEditor) {
      const [match] = Editor.nodes(editor, {
         match: (n) => n.type === "code",
      });
      const marks = Editor.marks(editor);

      return !!match || marks?.code;
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

   placeCursorAtEnd(editor: BaseEditor & ReactEditor) {
      const lastNode = editor.children[editor.children.length - 1];
      const lastNodePath = ReactEditor.findPath(editor, lastNode);
      const endPoint = Editor.end(editor, lastNodePath);
      Transforms.select(editor, endPoint);
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

   toggleLink(editor: BaseEditor & ReactEditor, href?: string) {
      const isActive = CustomEditor.isLinkActive(editor);
      if (isActive) {
         Editor.removeMark(editor, "link");
      } else {
         Editor.addMark(editor, "link", true);
         Editor.addMark(editor, `href`, href);
      }
   },

   toggleCodeBlock(editor: BaseEditor & ReactEditor) {
      const isActive = CustomEditor.isCodeBlockActive(editor);
      console.log(`Is code block active: `, isActive);

      if (isActive) {
         Editor.removeMark(editor, "code");
      } else {
         Editor.addMark(editor, "code", true);
      }
   },

   toggleUserMention(editor: BaseEditor & ReactEditor) {
      const mark = `userMention`;
      const isActive = CustomEditor.isUserMentionActive(editor);

      if (isActive) {
         Editor.removeMark(editor, mark);
      } else {
         Editor.addMark(editor, mark, true);
      }
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
         link: true,
         href: (!href.startsWith(`http`) ? `https://` : ``) + href,
         children: [{ text: title || href }],
      };

      // Transforms.insertNodes(editor, link);
      Transforms.insertNodes(editor, link);
      Transforms.collapse(editor, { edge: "end" });
   },

   insertLineBreak(editor: BaseEditor & ReactEditor) {
      Transforms.insertNodes(editor, {
         children: [{ text: `` }],
         type: `line-break`,
      });
   },
};

// Define a React component renderer for our code blocks.
export const CodeElement = (props) => {
   return (
      <pre {...props.attributes}>
         <code>{props.children}</code>
      </pre>
   );
};

// Define a React component to render leaves with bold text.
export const Leaf = (props) => {
   console.log({ props });
   if (props.leaf.link) {
      const properHref = (!props.leaf.href.startsWith(`http`) ? `https://` : ``) + props.leaf.href;
      return (
         <Link
            tabIndex={0}
            role={`link`} onClick={_ => window.open(properHref, `_blank`)}
            className={`text-primary-500 tap-highlight-transparent cursor-pointer hover:underline`}
            underline={"hover"}
            color={"primary"}
            target={`_blank`}
            isExternal
            size={"sm"}
            href={properHref}
            /*{...props.attributes}*/
         >
            {props.children}
         </Link>
      );
   }

   if (props.leaf.userMention) {
      return <Chip variant={`shadow`} size={`sm`} color={`primary`}>{props.children}</Chip>;
   }

   if (props.leaf.code) {
      return (
         <code {...props.attributes}>{props.children}</code>
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

CustomEditor.serialize.bind(CustomEditor);

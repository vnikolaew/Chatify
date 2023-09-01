import { BaseEditor, Editor, Element, Transforms } from "slate";
import { ReactEditor } from "slate-react";

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

"use client";
import React from "react";
import { Button, Divider, Tooltip } from "@nextui-org/react";
import BoldIcon from "@components/icons/BoldIcon";
import ItalicIcon from "@components/icons/ItalicIcon";
import StrikethroughIcon from "@components/icons/StrikethroughIcon";
import CodeIcon from "@components/icons/CodeIcon";
import { useSlate } from "slate-react";
import { CustomEditor } from "./editor";

export interface MessageTextEditorToolbarProps {}

const MessageTextEditorToolbar = ({}: MessageTextEditorToolbarProps) => {
   const editor = useSlate();
   return (
      <div
         className={`flex flex-col items-start gap-1 absolute left-3 top-3 w-full`}
      >
         <div className={`flex items-center gap-2 `}>
            <ToolbarButton
               onPress={(_) => CustomEditor.toggleBoldMark(editor)}
               isActive={editor.getMarks()?.bold}
               icon={<BoldIcon className={`fill-foreground`} size={16} />}
               text={`Bold`}
            />
            <Divider
               orientation={"vertical"}
               className={`h-3 text-default-300 bg-default-300 rounded-full w-[0.5px]`}
            />
            <ToolbarButton
               isActive={editor.getMarks()?.italic}
               onPress={(_) => CustomEditor.toggleItalicMark(editor)}
               icon={<ItalicIcon className={`fill-foreground`} size={16} />}
               text={`Italic`}
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
               text={`Strikethrough`}
            />
            <ToolbarButton
               onPress={(_) => CustomEditor.toggleCodeBlock(editor)}
               icon={<CodeIcon className={`stroke-foreground`} size={16} />}
               text={`Code`}
            />
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

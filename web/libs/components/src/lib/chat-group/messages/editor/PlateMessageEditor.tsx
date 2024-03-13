"use client";
import React from "react";
import { Plate } from "@udecode/plate-common";
import { Editor } from "./@/components/plate-ui/editor";
import { plugins } from "./plate-plugins";
import { CommentsProvider } from "@udecode/plate-comments";

export interface PlateMessageEditorProps {}

const PlateMessageEditor = ({}: PlateMessageEditorProps) => {
   return (
      <CommentsProvider>
         <Plate plugins={plugins}>
            <Editor placeholder={`Type ...`} />
         </Plate>
      </CommentsProvider>
   );
};

export default PlateMessageEditor;

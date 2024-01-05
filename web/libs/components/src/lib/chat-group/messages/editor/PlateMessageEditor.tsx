"use client";
import React from "react";
import { Plate } from "@udecode/plate-common";
import { Editor } from "./@/components/plate-ui/editor";
import { plugins } from "./plate-plugins";

export interface PlateMessageEditorProps {

}

const PlateMessageEditor = ({}: PlateMessageEditorProps) => {
   return (
      <Plate plugins={plugins}>
         <Editor placeholder={`Type ...`} />
      </Plate>
   );
};

export default PlateMessageEditor;

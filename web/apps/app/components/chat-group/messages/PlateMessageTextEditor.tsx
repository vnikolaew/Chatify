"use client";
import React, { useState } from "react";
import { Plate } from "@udecode/plate-common";
import {
   createBoldPlugin,
   createCodePlugin,
   createItalicPlugin,
   createStrikethroughPlugin,
   createUnderlinePlugin,
} from "@udecode/plate-basic-marks";
import { createBlockquotePlugin } from "@udecode/plate-block-quote";
import { createCodeBlockPlugin } from "@udecode/plate-code-block";
import { createHeadingPlugin } from "@udecode/plate-heading";
import { createParagraphPlugin } from "@udecode/plate-paragraph";
import { createPlugins, PlatePlugin } from "@udecode/plate";

export interface PlateMessageTextEditorProps {}

const editableProps = {
   placeholder: "Type ...",
};

const plugins: PlatePlugin[] = createPlugins(
   [
      createParagraphPlugin(),
      createBlockquotePlugin(),
      createCodeBlockPlugin(),
      createHeadingPlugin(),

      createBoldPlugin(),
      createItalicPlugin(),
      createUnderlinePlugin(),
      createStrikethroughPlugin(),
      createCodePlugin(),
   ],
   {}
);

const initialValue = [
   {
      type: "p",
      children: [
         {
            text: "This is editable plain text with react and history plugins, just like a <textarea>!",
         },
      ],
   },
];

const PlateMessageTextEditor = ({}: PlateMessageTextEditorProps) => {
   const [debugValue, setDebugValue] = useState<any | null>(null);

   return (
      <div className={`w-full`}>
         <Plate
            onChange={setDebugValue}
            plugins={plugins}
            initialValue={initialValue}
            editableProps={editableProps}
         >
            Debug value:
            <br />
            {JSON.stringify(debugValue)}
         </Plate>
      </div>
   );
};

export default PlateMessageTextEditor;

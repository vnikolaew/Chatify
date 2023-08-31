"use client";
import React from "react";

export interface EmojiProps
   extends React.DetailedHTMLProps<
      React.HTMLAttributes<HTMLSpanElement>,
      HTMLSpanElement
   > {
   code: number;
}

export const Emoji = ({ code, ...rest }: EmojiProps) => {
   return (
      <span
         dangerouslySetInnerHTML={{
            __html: `&#${code};`,
         }}
         {...rest}
      ></span>
   );
};

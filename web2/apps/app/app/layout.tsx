import { Metadata } from "next";
import "./styles.css";
import React, { PropsWithChildren } from "react";

export const metadata: Metadata = {
   title: "Welcome to app!",
   description: "Chatify - converse with your fellas!",
} satisfies Metadata;

function ChatifyLayout({ children }: PropsWithChildren) {
   return (
      <html>
         <body>
            <main className="app">{children}</main>
         </body>
      </html>
   );
}

export default ChatifyLayout;

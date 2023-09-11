"use client";
import React from "react";
import { ChatGroupFeedEntry as TChatGroupFeedEntry } from "@openapi";
import { Reorder, motion } from "framer-motion";
import ChatGroupFeedEntry from "./ChatGroupFeedEntry";
import { ScrollShadow } from "@nextui-org/react";

export interface ChatGroupFeedEntriesProps {
   feedEntries: TChatGroupFeedEntry[];
}

const ChatGroupFeedEntries = ({ feedEntries }: ChatGroupFeedEntriesProps) => {
   return (
      <ScrollShadow size={60} className={`max-h-[80vh] w-full`}>
         <Reorder.Group
            className={`w-full`}
            onReorder={undefined!}
            values={feedEntries ?? []}
         >
            {feedEntries?.map((e, i, { length }) => (
               <Reorder.Item key={e?.chatGroup?.id} value={e?.chatGroup?.id}>
                  <motion.div
                     layout
                     initial={{ opacity: 0, y: 20 }}
                     animate={{ opacity: 1, y: 0 }}
                     exit={{ opacity: 0, y: -20 }}
                     transition={{
                        duration: 0.3,
                        staggerChildren: 1,
                        type: `spring`,
                     }}
                     className={i === length - 1 ? `mb-12 w-full` : `w-full`}
                     key={(e as ChatGroupFeedEntry)?.chatGroup?.id}
                  >
                     <ChatGroupFeedEntry feedEntry={e} />
                  </motion.div>
               </Reorder.Item>
            ))}
         </Reorder.Group>
      </ScrollShadow>
   );
};

export default ChatGroupFeedEntries;

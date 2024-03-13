"use client";
import React, { useCallback, useMemo } from "react";
import { InfiniteData } from "@tanstack/react-query";
import { ChatGroupMessageEntry, CursorPaged } from "@openapi";
import groupBy from "lodash.groupby";
import toPairs from "lodash.topairs";
import { Chip } from "@nextui-org/react";
import moment from "moment/moment";
import { ChatMessageEntry, ChatMessageEntryProps } from "@web/components";
import { useCurrentUserId } from "@web/hooks";

export interface ChatMessageEntriesProps {
   messages: InfiniteData<CursorPaged<ChatGroupMessageEntry>>;
}

function parseDate(date: string): Date | null {
   const parts = date.split("-"); // Split the input string by '-'

   if (parts.length === 3) {
      // Ensure that we have exactly three parts: day, month, and year
      const day = parseInt(parts[0], 10); // Parse day as an integer
      const month = parseInt(parts[1], 10); // Parse month (subtract 1 as months are zero-based)
      const year = parseInt(parts[2], 10); // Parse year

      if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
         // Check if parsing was successful
         const parsedDate = new Date(year, month, day);

         if (!isNaN(parsedDate.getTime())) {
            // Check if the parsed date is valid
            return parsedDate;
         }
      }
   }

   // Return null for invalid input or parsing errors
   return null;
}

const ChatMessageEntries = React.forwardRef<
   HTMLDivElement,
   ChatMessageEntriesProps
>(({ messages }: ChatMessageEntriesProps, ref) => {
   const meId = useCurrentUserId();
   const messagesByDate = useMemo<
      (readonly [Date, ChatGroupMessageEntry[]])[]
   >(() => {
      const groupedMessages = groupBy<ChatGroupMessageEntry>(
         messages?.pages?.flatMap((_) => _.items),
         (message: ChatGroupMessageEntry) => {
            const date = new Date(message!.message!.createdAt!);
            return `${date.getDate()}-${date.getMonth()}-${date.getFullYear()}`;
         }
      );
      return toPairs(groupedMessages)
         .map(([date, entry]) => [parseDate(date), entry] as const)
         .sort(([dateA], [dateB]) => dateA!.getTime() - dateB!.getTime());
   }, [messages?.pages]);

   const getMessageEntryProps = useCallback(
      (
         message: ChatGroupMessageEntry,
         messageIndex: number,
         isLatest: boolean,
         isFirstInDateGroup: boolean
      ) => ({
         ...(isLatest && { className: `mb-4 w-full` }),
         ...(messageIndex === 0 && isFirstInDateGroup && { ref }),
         ...(messageIndex === 0 && isFirstInDateGroup && { showReplies: true }),
      }),
      [ref]
   );

   return (
      <div className={`flex w-full flex-col items-center gap-4`}>
         {messagesByDate.map(([date, messages], dateIndex, arr2) => (
            <div
               className={`flex w-full flex-col items-start gap-2`}
               key={date!.toISOString()}
            >
               <Chip
                  className={`!text-xxs self-center px-4 text-white`}
                  classNames={{
                     content: `!text-xxs`,
                  }}
                  size={`sm`}
                  content={date!.toISOString()}
                  color={`primary`}
                  variant={`shadow`}
               >
                  {moment(date, "DD-M-YYYY").format(
                     date!.getFullYear() === new Date().getFullYear()
                        ? "dddd, MMMM Do"
                        : `MMMM Do, YYYY`
                  )}
               </Chip>
               {messages?.map((message, i, arr) => {
                  const isMe = message!.senderInfo!.userId === meId;
                  const isLatest =
                     i === arr.length - 1 && dateIndex === arr2.length - 1;
                  return (
                     <ChatMessageEntry
                        className={`w-full`}
                        message={message}
                        key={i}
                        isMe={isMe}
                        {...getMessageEntryProps(
                           message,
                           i,
                           isLatest,
                           dateIndex === 0
                        )}
                     />
                  );
               })}
            </div>
         ))}
      </div>
   );
});

export default ChatMessageEntries;

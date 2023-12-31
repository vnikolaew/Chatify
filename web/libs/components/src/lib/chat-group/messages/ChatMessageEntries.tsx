"use client";
import React, { useMemo } from "react";
import { InfiniteData } from "@tanstack/react-query";
import { ChatGroupMessageEntry, CursorPaged } from "@openapi";
import groupBy from "lodash.groupby";
import toPairs from "lodash.topairs";
import { Chip } from "@nextui-org/react";
import moment from "moment/moment";
import { ChatMessageEntry } from "@web/components";
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

const ChatMessageEntries = React.forwardRef<HTMLDivElement, ChatMessageEntriesProps>(({ messages }: ChatMessageEntriesProps, ref) => {
    const meId = useCurrentUserId();
    const messagesByDate = useMemo(() => {
        const groupedMessages = groupBy<ChatGroupMessageEntry>(messages?.pages?.flatMap(_ => _.items),
            (message: ChatGroupMessageEntry) => {
                const date = new Date(message!.message!.createdAt!);
                return `${date.getDate()}-${date.getMonth()}-${date.getFullYear()}`;
            });
        return toPairs(groupedMessages)
            .map(([date, entry]) => [parseDate(date), entry] as const)
            .sort(([dateA], [dateB]) => dateA!.getTime() - dateB!.getTime());

    }, [messages?.pages]);

    return (
        <div className={`flex flex-col gap-4 items-center w-full`}>
            {messagesByDate.map(([date, messages], dateIndex, arr2) => (
                <div className={`flex w-full items-start flex-col gap-2`} key={date!.toISOString()}>
                    <Chip className={`text-xs px-4 self-center`} size={`sm`} content={date!.toISOString()}
                          color={`primary`}
                          variant={`shadow`}>{moment(date, "DD-M-YYYY").format(date!.getFullYear() === new Date().getFullYear() ? "dddd, MMMM Do" : `MMMM Do, YYYY`)}</Chip>
                    {messages?.map((message, i, arr) => {
                        const isMe = message!.senderInfo!.userId === meId;
                        const isLatest = i === arr.length - 1 && dateIndex === arr2.length - 1;
                        return (
                            <ChatMessageEntry
                                className={`w-full`}
                                message={message}
                                key={i}
                                isMe={isMe}
                                {...(isLatest && { className: `mb-4 w-full` })}
                                {...(i === 0 && dateIndex === 0 && { ref })}
                                {...(i === 0 && dateIndex === 0 && { showReplies: true })}
                            />
                        );
                    })
                    }
                </div>
            ))}
        </div>
    );
});

export default ChatMessageEntries;

"use client";
import React, { useMemo } from "react";
import { ChatMessageReaction } from "@openapi";
import { useCurrentUserId } from "@web/hooks";
import { Emoji } from "@web/components";

export interface ReactionsSummaryTooltipContentProps {
   reactions: ChatMessageReaction[];
}

export const ReactionsSummaryTooltipContent = ({
   reactions,
}: ReactionsSummaryTooltipContentProps) => {
   const meId = useCurrentUserId();
   const reactionCode = reactions?.[0]?.reactionCode;

   const messageReactors = useMemo(() => {
      if (!reactions) return ``;

      if (reactions?.length >= 3) {
         return `${reactions
            .slice(0, 2)
            .map((r) =>
               r.userId === meId ? `${r.username} (you)` : r.username
            )
            .join(", ")} and ${reactions.length - 2} other${
            reactions.length > 3 ? "s" : ""
         }`;
      } else {
         if (reactions.length === 1) {
            return `${
               reactions[0].userId === meId
                  ? "You (click to remove)"
                  : reactions[0].username
            }`;
         }
         const lastReaction = reactions.at(-1);

         return `${reactions
            ?.slice(0, -1)
            ?.map((r, i) =>
               r.userId === meId
                  ? `${i === 0 ? "You" : `you`} (click to remove)`
                  : r.username
            )
            ?.join(", ")} and ${
            lastReaction.userId === meId
               ? `you (click to remove)`
               : lastReaction.username
         }`;
      }
   }, [reactions, meId]);

   return (
      <div className={`text-default-500 py-2  font-light`}>
         <span className={`text-foreground font-normal`}>
            {messageReactors}
         </span>{" "}
         reacted with <Emoji code={reactionCode} />
      </div>
   );
};

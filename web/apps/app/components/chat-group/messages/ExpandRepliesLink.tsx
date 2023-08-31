"use client";
import React, { Fragment } from "react";
import { Link, Tooltip } from "@nextui-org/react";

export interface ExpandRepliesLinkProps {
   expanded: boolean;
   onPress?: (e: any) => any;
   totalReplies: number;
}

const TOOLTIP_DELAY = 100;

export const ExpandRepliesLink = ({
   expanded,
   onPress,
   totalReplies,
}: ExpandRepliesLinkProps) => {
   return expanded ? (
      <Link
         className={`text-xs cursor-pointer ml-2`}
         onPress={onPress}
         underline={"hover"}
         color={"primary"}
      >
         Hide
      </Link>
   ) : (
      <Tooltip
         showArrow
         offset={2}
         delay={TOOLTIP_DELAY}
         closeDelay={TOOLTIP_DELAY}
         classNames={{
            base: `text-xs px-3 py-0`,
         }}
         placement={"bottom"}
         color={"default"}
         content={"Show"}
         size={"sm"}
      >
         <Link
            className={`text-xs cursor-pointer ml-2`}
            onPress={onPress}
            underline={"hover"}
            color={"primary"}
         >
            {!expanded ? (
               <Fragment>
                  {totalReplies} {totalReplies > 1 ? `replies` : `reply`}
               </Fragment>
            ) : (
               "Hide"
            )}
         </Link>
      </Tooltip>
   );
};

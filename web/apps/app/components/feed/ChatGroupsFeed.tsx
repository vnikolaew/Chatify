"use client";
import React from "react";
import { useGetChatGroupsFeedQuery } from "@web/api";
import {
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Input,
   Skeleton,
} from "@nextui-org/react";
import HamburgerMenuIcon from "../icons/HamburgerMenuIcon";
import SearchIcon from "../icons/SearchIcon";
import ChatGroupFeedEntry from "./ChatGroupFeedEntry";

export interface ChatGroupsFeedProps {}

const ChatGroupsFeed = ({}: ChatGroupsFeedProps) => {
   const {
      data: feedEntries,
      isLoading,
      error,
   } = useGetChatGroupsFeedQuery({
      offset: 0,
      limit: 10,
   });
   console.log(feedEntries);

   return (
      <aside
         className={`border-r-1 border-r-default-200 flex w-full flex-col items-center gap-2`}
      >
         <div className={`flex w-full items-center p-2 gap-2`}>
            <Dropdown offset={10} showArrow>
               <DropdownTrigger>
                  <Button
                     className={`bg-transparent border-none hover:bg-default-200`}
                     variant={"faded"}
                     size={"md"}
                     radius={"full"}
                     color={"default"}
                     isIconOnly
                  >
                     <HamburgerMenuIcon
                        className={`cursor-pointer`}
                        size={24}
                     />
                  </Button>
               </DropdownTrigger>
               <DropdownMenu aria-label={"Options"}>
                  <DropdownItem key={"1"}>Option 1</DropdownItem>
                  <DropdownItem key={"2"}>Option 2</DropdownItem>
                  <DropdownItem key={"3"}>Option 3</DropdownItem>
               </DropdownMenu>
            </Dropdown>
            <div className={`flex-1 text-medium`}>
               <Input
                  className={`w-3/4 px-2`}
                  classNames={{
                     inputWrapper:
                        "hover:border-foreground-500 px-4 group-data-[focus=true]:border-primary-500 border-1 border-transparent",
                  }}
                  color={"default"}
                  radius={"full"}
                  size={"md"}
                  startContent={
                     <SearchIcon
                        className={`mr-1 group-data-[focus=true]:fill-primary-500`}
                        fill={"currentColor"}
                        size={20}
                     />
                  }
                  placeholder={"Search"}
               />
            </div>
         </div>
         <div className={`w-full pr-4 flex flex-col gap-2 p-2 items-center`}>
            {isLoading ? (
               <>
                  {Array.from({ length: 10 }).map((_, i) => (
                     <div
                        key={i}
                        className={`flex items-center w-full gap-4 p-2`}
                     >
                        <Skeleton className={`w-14 h-14 rounded-full`} />
                        <div
                           className={`flex flex-1 flex-col items-center gap-2`}
                        >
                           <div
                              className={`flex w-full items-center justify-between`}
                           >
                              <Skeleton className={`w-3/5 h-3 rounded-full`} />
                              <Skeleton className={`w-1/6 h-3 rounded-full`} />
                           </div>
                           <Skeleton className={`w-full h-4 rounded-full`} />
                        </div>
                     </div>
                  ))}
               </>
            ) : (
               feedEntries.map((e, i) => (
                  <ChatGroupFeedEntry key={i} feedEntry={e} />
               ))
            )}
         </div>
      </aside>
   );
};

export default ChatGroupsFeed;

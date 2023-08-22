"use client";
import React, { Fragment, useMemo, useState } from "react";
import { useGetChatGroupsFeedQuery } from "@web/api";
import {
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Input,
   Link,
   ScrollShadow,
   Skeleton,
} from "@nextui-org/react";
import HamburgerMenuIcon from "../icons/HamburgerMenuIcon";
import SearchIcon from "../icons/SearchIcon";
import ChatGroupFeedEntry from "./ChatGroupFeedEntry";
import { useIsUserLoggedIn } from "../../hooks/useIsUserLoggedIn";
import { useDebounce } from "../../hooks/useDebounce";
import { useSearchChatGroupsByName } from "@web/api";
import NotSentIcon from "../icons/NotSentIcon";

export interface ChatGroupsFeedProps {}

const ChatGroupsFeed = ({}: ChatGroupsFeedProps) => {
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const [searchTerm, setSearchTerm] = useState("");
   const debouncedSearch = useDebounce(searchTerm, 2000);
   const {
      data: searchEntries,
      isLoading: searchLoading,
      isFetching: searchFetching,
      error: searchError,
   } = useSearchChatGroupsByName(
      { query: debouncedSearch },
      { enabled: debouncedSearch?.length >= 3 }
   );

   const {
      data: feedEntries,
      isLoading,
      isFetching,
      error,
   } = useGetChatGroupsFeedQuery(
      {
         offset: 0,
         limit: 10,
      },
      { enabled: isUserLoggedIn }
   );

   const filteredEntries = useMemo(() => {
      if (!debouncedSearch || !searchEntries) return feedEntries;

      const searchEntryIds = new Set(searchEntries?.data.map((_) => _.id));
      return feedEntries?.filter((e) => searchEntryIds.has(e.chatGroup.id));
   }, [debouncedSearch, searchEntries?.data, feedEntries]);

   console.log(feedEntries);
   console.log(`q = ${debouncedSearch}`, filteredEntries);
   console.log(searchEntries);

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
                  value={searchTerm}
                  isClearable
                  onValueChange={setSearchTerm}
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
         <ScrollShadow
            size={50}
            className={`w-full pr-4 flex flex-col gap-2 p-2 items-center`}
         >
            {(isLoading || searchLoading) && (isFetching || searchFetching) && (
               <Fragment>
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
               </Fragment>
            )}
            {!!filteredEntries && filteredEntries.length === 0 ? (
               <div
                  className={`mt-12 flex-col flex items-center justify-center gap-2 text-default-300 text-large`}
               >
                  <NotSentIcon className={`fill-default-200`} size={50} />
                  <span>No chat groups found.</span>
                  <Link></Link>
                  <Button
                     href={`/chat-groups/create`}
                     as={Link}
                     size={"sm"}
                     variant={"solid"}
                     color={"primary"}
                  >
                     Create Chat group
                  </Button>
               </div>
            ) : (
               (filteredEntries ?? feedEntries)?.map((e, i) => (
                  <ChatGroupFeedEntry key={i} feedEntry={e} />
               ))
            )}
         </ScrollShadow>
      </aside>
   );
};

export default ChatGroupsFeed;

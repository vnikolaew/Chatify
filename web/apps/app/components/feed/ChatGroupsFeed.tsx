"use client";
import React, { Fragment, useMemo, useState } from "react";
import { useGetChatGroupsFeedQuery, useGetStarredChatGroupsFeedQuery } from "@web/api";
import {
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Input,
   Link,
   Skeleton,
} from "@nextui-org/react";
import { useIsUserLoggedIn, useDebounce } from "@hooks";
import { useSearchChatGroupsByName } from "@web/api";
import { HamburgerMenuIcon, NotSentIcon, SearchIcon } from "@icons";
import ChatGroupFeedEntries from "@components/feed/ChatGroupFeedEntries";
import { useTranslations } from "next-intl";
import { MessageCircle, Star } from "lucide-react";

export interface ChatGroupsFeedProps {
}

enum FeedMode {
   ALL,
   STARRED
}

const ChatGroupsFeed = ({}: ChatGroupsFeedProps) => {
   const { isUserLoggedIn } = useIsUserLoggedIn();
   const [searchTerm, setSearchTerm] = useState("");
   const debouncedSearch = useDebounce(searchTerm, 2000);
   const t = useTranslations("FeedSidebar");

   const {
      data: searchEntries,
      isLoading: searchLoading,
      isFetching: searchFetching,
      error: searchError,
   } = useSearchChatGroupsByName(
      { query: debouncedSearch },
      { enabled: debouncedSearch?.length >= 3 },
   );

   const [feedMode, setFeedMode] = useState(FeedMode.ALL);
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
      { enabled: isUserLoggedIn && feedMode === FeedMode.ALL },
   );

   const {
      data: starredGroupFeedEntries,
      isLoading: starredLoading,
      isFetching: starredFetching,
      isRefetching: starredRefetching,
   } = useGetStarredChatGroupsFeedQuery({ enabled: isUserLoggedIn && feedMode === FeedMode.STARRED });

   const filteredEntries = useMemo(() => {
      if (!debouncedSearch || !searchEntries) return feedMode === FeedMode.ALL ? feedEntries : starredGroupFeedEntries;

      const searchEntryIds = new Set(searchEntries?.map((_) => _.id));
      return feedEntries?.filter((e) => searchEntryIds.has(e.chatGroup.id));
   }, [debouncedSearch, searchEntries, feedEntries, feedMode, starredGroupFeedEntries]);

   const showSkeletons = useMemo(() =>
         feedMode === FeedMode.ALL ? (isLoading && isFetching)
            : ((starredLoading && starredFetching) || starredRefetching),
      [feedMode, isLoading, isFetching, starredLoading, starredFetching, starredRefetching]);

   // @ts-ignore
   return (
      <aside
         className={`border-r-1 rounded-medium border-r-default-200 flex w-full flex-col items-center gap-2`}
      >
         <section
            className={`flex h-fit w-full items-center p-2 gap-1 rounded-medium `}
         >
            <Dropdown classNames={{}} offset={10} showArrow>
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
               <DropdownMenu
                  onAction={key => {
                     switch (key) {
                        case `0`: {
                           setFeedMode(FeedMode.ALL);
                           break;
                        }
                        case `1`: {
                           setFeedMode(FeedMode.STARRED);
                           break;
                        }
                     }
                  }}
                  aria-label={"Options"}>
                  <DropdownItem
                     className={`!text-xs ${feedMode === FeedMode.ALL && `!bg-default-200`}`}
                     startContent={<MessageCircle strokeWidth={3} size={16} className={`stroke-default-700`} />}
                     key={`0`}>
                     View all
                  </DropdownItem>
                  <DropdownItem
                     className={`!text-xs ${feedMode === FeedMode.STARRED && `!bg-default-200`}`}
                     startContent={<Star size={16} className={`fill-primary-500 stroke-primary-500`} />}
                     key={`1`}>
                     View starred
                  </DropdownItem>

               </DropdownMenu>
            </Dropdown>
            <div className={`flex-1 text-medium`}>
               <Input
                  value={searchTerm}
                  isClearable
                  onValueChange={setSearchTerm}
                  className={`w-full px-2`}
                  classNames={{
                     inputWrapper:
                        "hover:border-foreground-500 px-4 !py-1 group-data-[focus=true]:border-primary-500 border-1 border-transparent",
                     innerWrapper: `py-0`,
                  }}
                  color={"default"}
                  radius={"full"}
                  size={"sm"}
                  startContent={
                     <SearchIcon
                        className={`mr-1 group-data-[focus=true]:fill-primary-500`}
                        fill={"currentColor"}
                        size={20}
                     />
                  }
                  placeholder={t(`SearchPlaceholder`)}
               />
            </div>
         </section>
         <div
            className={`w-full pr-4 flex flex-col gap-2 p-2 items-center rounded-md border-b-1 border-b-default-200`}
         >
            {showSkeletons && (
               <Fragment>
                  {Array.from({ length: 10 }).map((_, i) => (
                     <LoadingFeedEntry key={i} />
                  ))}
               </Fragment>
            )}
            {!!filteredEntries && filteredEntries.length === 0 ? (
               <div
                  className={`my-16 h-auto flex-col flex items-center justify-center gap-2 text-default-300 text-large`}
               >
                  <NotSentIcon className={`fill-default-200`} size={50} />
                  <span>No chat groups found.</span>
                  {feedMode === FeedMode.ALL && (
                     <Button
                        href={`/chat-groups/create`}
                        as={Link}
                        size={"sm"}
                        variant={"solid"}
                        color={"primary"}
                     >
                        Create Chat group
                     </Button>
                  )}
               </div>
            ) : (
               <ChatGroupFeedEntries
                  feedEntries={filteredEntries ?? (feedMode === FeedMode.ALL ? feedEntries : starredGroupFeedEntries) ?? []}
               />
            )}
         </div>
      </aside>
   );
};

const LoadingFeedEntry = () => {
   return (
      <div
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
   );

};

export default ChatGroupsFeed;

"use client";
import React, { PropsWithChildren } from "react";
import {
   Button,
   Dropdown,
   DropdownItem,
   DropdownMenu,
   DropdownTrigger,
   Input,
} from "@nextui-org/react";
import SearchIcon from "../../components/icons/SearchIcon";
import HamburgerMenuIcon from "../../components/icons/HamburgerMenuIcon";
import { useGetChatGroupsFeedQuery } from "@web/api";

export interface LayoutProps extends PropsWithChildren {}

const Layout = ({ children }: LayoutProps) => {
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
      <div className={`flex items-start gap-0`}>
         <aside className={`grow-[1] flex flex-col items-center gap-2`}>
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
            <div className={`w-full flex flex-col p-2 items-center`}>
               Sidebar
            </div>
         </aside>
         <div className={`grow-[3]`}>{children}</div>
      </div>
   );
};

export default Layout;

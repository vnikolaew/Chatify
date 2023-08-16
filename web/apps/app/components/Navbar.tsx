"use client";
import React from "react";
import {
   Link,
   Navbar,
   NavbarBrand,
   NavbarContent,
   NavbarItem,
} from "@nextui-org/react";

const MainNavbar = () => {
   return (
      <Navbar
         classNames={{
            item: ["data-[active=true]:text-primary"],
            base: "py-2",
         }}
         className={`w-full mx-0 !max-w-[3000px]`}
         isBordered
      >
         <NavbarBrand className={`text-3xl`}>
            <Link>
               Chatify
            </Link>
         </NavbarBrand>
         <NavbarContent>
            <NavbarItem>
               <Link
                  size={"lg"}
                  className={`text-foreground group-[data-active=true]:text-primary`}
                  href={`/`}
               >
                  Link 1
               </Link>
            </NavbarItem>
            <NavbarItem className={`group`} isActive>
               <Link
                  size={"lg"}
                  className={`text-foreground group-[data-active=true]:text-primary`}
                  href={`/`}
               >
                  Link 2
               </Link>
            </NavbarItem>
            <NavbarItem>
               <Link
                  size={"lg"}
                  className={`text-foreground group-[data-active=true]:text-primary`}
                  href={`/`}
               >
                  Link 3
               </Link>
            </NavbarItem>
         </NavbarContent>
      </Navbar>
   );
};

export default MainNavbar;
